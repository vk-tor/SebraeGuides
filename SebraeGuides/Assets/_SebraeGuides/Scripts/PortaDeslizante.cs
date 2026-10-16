using System.Collections;
using UnityEngine;

namespace SebraeGuides
{
    /// <summary>
    /// Desliza a porta de vidro (e a maçaneta) e, no fim, envia 'porta_aberta'.
    /// </summary>
    public class PortaDeslizante : MonoBehaviour
    {
        [Tooltip("Tudo o que se move junto: a porta do modelo e a maçaneta.")]
        public Transform[] partes;

        [Tooltip("Quanto a porta anda, em metros, no espaço do mundo.")]
        public Vector3 deslocamento = new Vector3(-1.6f, 0f, 0f);

        public float duracao = 1.2f;
        public string evento = "porta_aberta";
        public AudioClip som;

        bool aberta;

        public void Abrir()
        {
            if (aberta) return;
            aberta = true;
            if (som) AudioSource.PlayClipAtPoint(som, transform.position, 0.8f);
            StartCoroutine(Deslizar());
        }

        IEnumerator Deslizar()
        {
            int total = partes != null ? partes.Length : 0;
            var inicio = new Vector3[total];
            for (int i = 0; i < total; i++)
            {
                if (partes[i]) inicio[i] = partes[i].position;
            }

            float tempo = 0f;
            while (tempo < duracao)
            {
                tempo += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, tempo / duracao);
                for (int i = 0; i < total; i++)
                {
                    if (partes[i]) partes[i].position = inicio[i] + deslocamento * k;
                }
                yield return null;
            }

            if (!string.IsNullOrEmpty(evento)) ControleVisita.Enviar(evento);
        }
    }
}
