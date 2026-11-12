using System.Collections;
using TMPro;
using UnityEngine;

namespace SebraeGuides
{
    /// <summary>
    /// Post-it do coworking, o único objeto que o mural de ideias aceita. Começa em branco;
    /// o MuralDeIdeias escreve nele a ideia que o visitante escolheu no balão do guia.
    /// </summary>
    public class PostIt : MonoBehaviour
    {
        [Tooltip("Texto na frente do post-it, onde a ideia aparece.")]
        public TMP_Text texto;

        [Tooltip("Parte visual que dá um pulinho quando a ideia é escrita. Vazio = o próprio objeto.")]
        public Transform visual;

        public bool Escrito { get; private set; }
        public string Ideia { get; private set; }

        void Awake()
        {
            if (!visual) visual = transform;
            if (texto && !Escrito) texto.text = string.Empty;
        }

        public void Escrever(string ideia)
        {
            Ideia = ideia;
            Escrito = !string.IsNullOrEmpty(ideia);
            if (texto) texto.text = ideia;
            if (isActiveAndEnabled) StartCoroutine(Pular());
        }

        IEnumerator Pular()
        {
            var normal = visual.localScale;
            const float duracao = 0.3f;
            for (float t = 0f; t < duracao; t += Time.deltaTime)
            {
                float onda = Mathf.Sin(t / duracao * Mathf.PI); // 0 → 1 → 0
                visual.localScale = normal * (1f + 0.2f * onda);
                yield return null;
            }
            visual.localScale = normal;
        }
    }
}
