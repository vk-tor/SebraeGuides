using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SebraeGuides
{
    /// <summary>
    /// Balão de fala do guia. Mostra o texto de cada fala do roteiro, toca o áudio
    /// e cria um botão para cada opção. Botões com evento avisam o ControleVisita.
    /// </summary>
    public class DialogoGuia : MonoBehaviour
    {
        [Header("Dados")]
        public RoteiroGuia roteiro;

        [Header("Interface (montada pelo menu Sebrae Guides)")]
        public GameObject balao;
        public TMP_Text textoNome;
        public TMP_Text textoFala;
        public TMP_Text textoDica;
        public TMP_Text textoContador;
        public RectTransform areaBotoes;
        public Button modeloBotao;

        [Header("Áudio")]
        public AudioSource voz;
        public AudioClip somClique;

        /// <summary>Disparado quando o visitante aperta um botão que tem evento.</summary>
        public event Action<string> AoEvento;

        public string FalaAtual { get; private set; }

        readonly List<Button> botoesCriados = new List<Button>();
        Vector3 posicaoBalao;

        void Awake()
        {
            if (modeloBotao) modeloBotao.gameObject.SetActive(false);
            if (roteiro && textoNome) textoNome.text = roteiro.nomeDoGuia;
            if (balao)
            {
                posicaoBalao = balao.transform.localPosition;
                balao.SetActive(false);
            }
        }

        /// <summary>Põe o balão do lado direito (padrão) ou esquerdo do guia, visto pelo visitante.</summary>
        public void DefinirLadoDoBalao(bool esquerda)
        {
            if (!balao) return;
            var posicao = posicaoBalao;
            posicao.x = Mathf.Abs(posicao.x) * (esquerda ? -1f : 1f);
            balao.transform.localPosition = posicao;
        }

        /// <summary>Mostra a fala com o ID informado.</summary>
        public void Mostrar(string idFala)
        {
            if (!roteiro)
            {
                Debug.LogWarning("[Diálogo] Nenhum roteiro ligado ao DialogoGuia.");
                return;
            }

            var fala = roteiro.Buscar(idFala);
            if (fala == null) return;

            FalaAtual = fala.id;
            if (balao) balao.SetActive(true);
            if (textoFala) textoFala.text = fala.texto;

            LimparBotoes();
            bool temOpcoes = fala.opcoes != null && fala.opcoes.Count > 0;
            if (temOpcoes)
            {
                foreach (var opcao in fala.opcoes) CriarBotao(opcao);
            }

            if (textoDica)
            {
                bool mostrarDica = !temOpcoes && !string.IsNullOrEmpty(fala.dica);
                textoDica.gameObject.SetActive(mostrarDica);
                textoDica.text = mostrarDica ? fala.dica : string.Empty;
            }

            if (voz)
            {
                voz.Stop();
                if (fala.audio)
                {
                    voz.clip = fala.audio;
                    voz.Play();
                }
            }
        }

        /// <summary>Mostra de novo a última fala (usado quando o visitante seleciona o guia).</summary>
        public void Repetir()
        {
            if (!string.IsNullOrEmpty(FalaAtual)) Mostrar(FalaAtual);
        }

        public void Fechar()
        {
            if (balao) balao.SetActive(false);
            if (voz) voz.Stop();
        }

        public void DefinirContador(string texto)
        {
            if (textoContador) textoContador.text = texto;
        }

        void CriarBotao(OpcaoFala opcao)
        {
            if (!modeloBotao || !areaBotoes) return;

            var botao = Instantiate(modeloBotao, areaBotoes);
            botao.gameObject.SetActive(true);
            botao.name = "Botao - " + opcao.rotulo;

            var rotulo = botao.GetComponentInChildren<TMP_Text>(true);
            if (rotulo) rotulo.text = opcao.rotulo;

            botao.onClick.RemoveAllListeners();
            var escolhida = opcao;
            botao.onClick.AddListener(() => Escolher(escolhida));
            botoesCriados.Add(botao);
        }

        void LimparBotoes()
        {
            foreach (var botao in botoesCriados)
            {
                if (botao) Destroy(botao.gameObject);
            }
            botoesCriados.Clear();
        }

        void Escolher(OpcaoFala opcao)
        {
            Debug.Log($"[Diálogo] Botão apertado: {opcao.rotulo} (fala {FalaAtual})");
            if (somClique) AudioSource.PlayClipAtPoint(somClique, transform.position, 0.6f);

            string proxima = opcao.proximaFala;
            string evento = opcao.evento;

            if (!string.IsNullOrEmpty(proxima)) Mostrar(proxima);
            else Fechar();

            if (!string.IsNullOrEmpty(evento)) AoEvento?.Invoke(evento);
        }
    }
}
