using System;
using TMPro;
using UnityEngine;

namespace SebraeGuides
{
    /// <summary>
    /// Um ponto de interesse. Detecta quando a cabeça do visitante chega perto
    /// e controla o painel em UI World Space com o conteúdo do espaço.
    /// </summary>
    public class PontoDeInteresse : MonoBehaviour
    {
        [Header("Conteúdo do painel")]
        public string titulo = "Ponto de interesse";

        [TextArea(3, 6)]
        public string resumo;

        [TextArea(3, 8)]
        [Tooltip("Texto extra mostrado quando o visitante aperta 'Quero saber mais'.")]
        public string detalhes;

        [Header("Detecção")]
        [Tooltip("Distância (m), no chão, entre este objeto e a cabeça do visitante para contar como chegada.")]
        public float raio = 2f;

        [Header("Interface (montada pelo menu Sebrae Guides)")]
        public GameObject painel;
        public TMP_Text textoTitulo;
        public TMP_Text textoCorpo;

        public bool JogadorDentro { get; private set; }

        public event Action<PontoDeInteresse> AoEntrar;

        Transform cabeca;
        bool detalhado;

        // Awake (e não Start) para o painel já estar escondido quando o ControleVisita abrir o da recepção.
        void Awake()
        {
            AtualizarTextos();
            if (painel) painel.SetActive(false);
        }

        void Update()
        {
            if (!cabeca)
            {
                var cam = Camera.main;
                if (!cam) return;
                cabeca = cam.transform;
            }

            Vector3 distancia = cabeca.position - transform.position;
            distancia.y = 0f;
            bool dentro = distancia.sqrMagnitude <= raio * raio;

            if (dentro && !JogadorDentro)
            {
                JogadorDentro = true;
                AoEntrar?.Invoke(this);
            }
            else if (!dentro)
            {
                JogadorDentro = false;
            }
        }

        public void MostrarPainel()
        {
            if (painel) painel.SetActive(true);
        }

        public void MostrarDetalhes()
        {
            detalhado = true;
            AtualizarTextos();
            MostrarPainel();
        }

        void AtualizarTextos()
        {
            if (textoTitulo) textoTitulo.text = titulo;
            if (textoCorpo)
            {
                bool comDetalhes = detalhado && !string.IsNullOrEmpty(detalhes);
                textoCorpo.text = comDetalhes ? resumo + "\n\n" + detalhes : resumo;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.08f, 0.72f, 0.65f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, raio);
        }
    }
}
