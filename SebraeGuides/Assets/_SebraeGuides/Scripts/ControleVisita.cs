using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SebraeGuides
{
    /// <summary>Uma parada da visita (recepção, arena, coworking...).</summary>
    [Serializable]
    public class ParadaVisita
    {
        [Tooltip("Nome curto usado nos eventos. Um botão com o evento ir_<chave> leva o visitante para esta parada.")]
        public string chave;

        [Tooltip("Ponto de teleporte (Teleport Anchor) desta parada. Fica desligado até a parada ser liberada.")]
        public GameObject pontoTeleporte;

        [Tooltip("Onde o guia fica nesta parada.")]
        public Transform posicaoGuia;

        [Tooltip("Marque quando há parede do lado direito do guia: o balão passa para o lado esquerdo.")]
        public bool balaoAEsquerda;

        public PontoDeInteresse poi;

        [Tooltip("Fala que o guia diz quando o visitante chega.")]
        public string falaAoChegar;

        [Tooltip("Evento que conclui esta parada (ex.: tv_ligada). Vazio = conclui assim que o visitante chega.")]
        public string eventoDeConclusao;

        [Tooltip("Fala mostrada quando a parada é concluída.")]
        public string falaAoConcluir;

        [Tooltip("Objetos ligados quando o visitante chega (ex.: o post-it).")]
        public List<GameObject> ativarAoChegar = new List<GameObject>();

        [Tooltip("Objetos ligados quando a parada é concluída (ex.: o teleporte dentro da sala).")]
        public List<GameObject> ativarAoConcluir = new List<GameObject>();
    }

    /// <summary>
    /// Controla o loop da visita: início → paradas → volta à recepção → fim.
    /// Recebe os eventos das interações e dos botões do guia.
    /// </summary>
    public class ControleVisita : MonoBehaviour
    {
        public static ControleVisita Instancia { get; private set; }

        [Header("Referências")]
        public DialogoGuia dialogo;
        public GuiaNPC guia;
        public MarcadorProximaParada marcador;

        [Header("Paradas, na ordem da visita (a primeira é o início e o fim)")]
        public List<ParadaVisita> paradas = new List<ParadaVisita>();

        [Header("Falas especiais (IDs do roteiro)")]
        public string falaInicial = "R1";
        public string falaIndoParaProxima = "AVISO_IR";
        public string falaFinal = "E1";
        public string falaDespedida = "FIM";

        [Header("Tempos")]
        public float atrasoInicial = 1.5f;
        public float segundosAteFechar = 4f;

        int atual;
        bool voltando;
        bool terminou;
        readonly HashSet<int> chegou = new HashSet<int>();
        readonly HashSet<int> concluidas = new HashSet<int>();

        /// <summary>Qualquer script pode avisar o controle: ControleVisita.Enviar("tv_ligada").</summary>
        public static void Enviar(string evento)
        {
            if (Instancia != null) Instancia.ReceberEvento(evento);
            else Debug.LogWarning($"[Visita] Evento '{evento}' enviado, mas não há ControleVisita ativo na cena.");
        }

        void Awake()
        {
            Instancia = this;
        }

        void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
        }

        void OnEnable()
        {
            if (dialogo) dialogo.AoEvento += ReceberEvento;
            if (guia) guia.AoSelecionar += AoSelecionarGuia;
            foreach (var parada in paradas)
            {
                if (parada != null && parada.poi) parada.poi.AoEntrar += AoEntrarNoPoi;
            }
        }

        void OnDisable()
        {
            if (dialogo) dialogo.AoEvento -= ReceberEvento;
            if (guia) guia.AoSelecionar -= AoSelecionarGuia;
            foreach (var parada in paradas)
            {
                if (parada != null && parada.poi) parada.poi.AoEntrar -= AoEntrarNoPoi;
            }
        }

        IEnumerator Start()
        {
            for (int i = 0; i < paradas.Count; i++)
            {
                var parada = paradas[i];
                if (parada == null) continue;
                if (parada.pontoTeleporte) parada.pontoTeleporte.SetActive(i == 0);
                Ligar(parada.ativarAoChegar, false);
                Ligar(parada.ativarAoConcluir, false);
            }

            if (marcador) marcador.Esconder();

            if (paradas.Count > 0 && paradas[0] != null)
            {
                atual = 0;
                if (guia && paradas[0].posicaoGuia) guia.Posicionar(paradas[0].posicaoGuia);
                if (dialogo) dialogo.DefinirLadoDoBalao(paradas[0].balaoAEsquerda);
                Chegar(0, false);
            }

            AtualizarContador();

            yield return new WaitForSeconds(atrasoInicial);
            if (dialogo) dialogo.Mostrar(falaInicial);
        }

        public void ReceberEvento(string evento)
        {
            if (string.IsNullOrEmpty(evento)) return;
            Debug.Log($"[Visita] Evento recebido: {evento}");

            if (evento.StartsWith("ir_"))
            {
                string chave = evento.Substring(3);
                if (chave == "retorno")
                {
                    Voltar();
                    return;
                }

                int indice = paradas.FindIndex(p => p != null && p.chave == chave);
                if (indice >= 0) IrPara(indice);
                else Debug.LogWarning($"[Visita] Não existe parada com a chave '{chave}'.");
                return;
            }

            switch (evento)
            {
                case "mais":
                    if (Valida(atual) && paradas[atual].poi) paradas[atual].poi.MostrarDetalhes();
                    return;
                case "recomecar":
                    Recomecar();
                    return;
                case "encerrar":
                    StartCoroutine(Encerrar());
                    return;
            }

            if (!voltando && Valida(atual) && paradas[atual].eventoDeConclusao == evento)
            {
                Concluir(atual);
            }
            else
            {
                string nome = Valida(atual) ? paradas[atual].chave : "-";
                Debug.Log($"[Visita] Evento '{evento}' ignorado agora (parada atual: {nome}).");
            }
        }

        void IrPara(int indice)
        {
            atual = indice;
            voltando = false;
            var parada = paradas[indice];

            if (parada.pontoTeleporte) parada.pontoTeleporte.SetActive(true);
            if (guia && parada.posicaoGuia) guia.IrPara(parada.posicaoGuia);
            TrocarLadoDoBalao(parada.balaoAEsquerda);
            if (marcador && parada.pontoTeleporte) marcador.MostrarEm(parada.pontoTeleporte.transform);
            if (dialogo) dialogo.Mostrar(falaIndoParaProxima);

            if (parada.poi && parada.poi.JogadorDentro) AoEntrarNoPoi(parada.poi);
        }

        void AoEntrarNoPoi(PontoDeInteresse poi)
        {
            int indice = paradas.FindIndex(p => p != null && p.poi == poi);
            if (indice < 0) return;

            if (voltando && indice == 0)
            {
                Finalizar();
                return;
            }

            if (indice == atual && !chegou.Contains(indice)) Chegar(indice, true);
        }

        void Chegar(int indice, bool falar)
        {
            chegou.Add(indice);
            var parada = paradas[indice];

            if (marcador) marcador.Esconder();
            if (parada.poi) parada.poi.MostrarPainel();
            Ligar(parada.ativarAoChegar, true);

            if (falar && dialogo && !string.IsNullOrEmpty(parada.falaAoChegar)) dialogo.Mostrar(parada.falaAoChegar);
            if (string.IsNullOrEmpty(parada.eventoDeConclusao)) Concluir(indice);
        }

        void Concluir(int indice)
        {
            if (concluidas.Contains(indice)) return;
            concluidas.Add(indice);

            var parada = paradas[indice];
            Ligar(parada.ativarAoConcluir, true);
            AtualizarContador();

            if (dialogo && !string.IsNullOrEmpty(parada.falaAoConcluir)) dialogo.Mostrar(parada.falaAoConcluir);
        }

        void Voltar()
        {
            if (!Valida(0)) return;
            voltando = true;
            atual = 0;
            var inicio = paradas[0];

            if (guia && inicio.posicaoGuia) guia.IrPara(inicio.posicaoGuia);
            TrocarLadoDoBalao(inicio.balaoAEsquerda);
            if (marcador && inicio.pontoTeleporte) marcador.MostrarEm(inicio.pontoTeleporte.transform);
            if (dialogo) dialogo.Mostrar(falaIndoParaProxima);

            if (inicio.poi && inicio.poi.JogadorDentro) Finalizar();
        }

        void Finalizar()
        {
            if (terminou) return;
            terminou = true;
            if (marcador) marcador.Esconder();
            if (dialogo) dialogo.Mostrar(falaFinal);
        }

        IEnumerator Encerrar()
        {
            if (dialogo) dialogo.Mostrar(falaDespedida);
            yield return new WaitForSeconds(segundosAteFechar);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Recomecar()
        {
            var cena = SceneManager.GetActiveScene();
            if (cena.buildIndex >= 0) SceneManager.LoadScene(cena.buildIndex);
            else SceneManager.LoadScene(cena.name);
        }

        // Troca o lado do balão no momento em que o guia reaparece no novo lugar.
        void TrocarLadoDoBalao(bool esquerda)
        {
            if (dialogo) StartCoroutine(TrocarLadoDepois(esquerda, guia ? guia.duracaoTroca : 0f));
        }

        IEnumerator TrocarLadoDepois(bool esquerda, float espera)
        {
            if (espera > 0f) yield return new WaitForSeconds(espera);
            dialogo.DefinirLadoDoBalao(esquerda);
        }

        void AtualizarContador()
        {
            if (dialogo) dialogo.DefinirContador($"Espaços visitados: {concluidas.Count}/{paradas.Count}");
        }

        void AoSelecionarGuia()
        {
            if (dialogo) dialogo.Repetir();
        }

        bool Valida(int indice)
        {
            return indice >= 0 && indice < paradas.Count && paradas[indice] != null;
        }

        static void Ligar(List<GameObject> objetos, bool ligado)
        {
            if (objetos == null) return;
            foreach (var objeto in objetos)
            {
                if (objeto) objeto.SetActive(ligado);
            }
        }
    }
}
