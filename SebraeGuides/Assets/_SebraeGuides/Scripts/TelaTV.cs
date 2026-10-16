using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SebraeGuides
{
    /// <summary>
    /// Tela da TV da arena. Ao ligar, passa as fotos (ou só as legendas, se ainda
    /// não houver fotos) e avisa o ControleVisita com o evento 'tv_ligada'.
    /// </summary>
    public class TelaTV : MonoBehaviour
    {
        [Tooltip("Objeto com o conteúdo da tela. Fica desligado até a TV ser ligada.")]
        public GameObject tela;
        public Image imagem;
        public TMP_Text legenda;

        [Tooltip("Fotos dos eventos (importe as imagens como Sprite).")]
        public Sprite[] fotos;

        [TextArea(1, 3)]
        public string[] legendas;

        public float segundosPorFoto = 3f;
        public string evento = "tv_ligada";
        public AudioClip som;

        Coroutine rotina;

        void Awake()
        {
            if (tela) tela.SetActive(false);
        }

        public void Ligar()
        {
            if (tela) tela.SetActive(true);
            if (rotina != null) StopCoroutine(rotina);
            rotina = StartCoroutine(Passar());
            if (som) AudioSource.PlayClipAtPoint(som, transform.position, 0.8f);
            if (!string.IsNullOrEmpty(evento)) ControleVisita.Enviar(evento);
        }

        IEnumerator Passar()
        {
            int totalFotos = fotos != null ? fotos.Length : 0;
            int totalLegendas = legendas != null ? legendas.Length : 0;
            int total = Mathf.Max(totalFotos, totalLegendas);
            if (total == 0) yield break;

            int i = 0;
            while (true)
            {
                if (imagem)
                {
                    bool temFoto = i < totalFotos && fotos[i] != null;
                    imagem.enabled = temFoto;
                    if (temFoto) imagem.sprite = fotos[i];
                }
                if (legenda) legenda.text = i < totalLegendas ? legendas[i] : string.Empty;

                yield return new WaitForSeconds(segundosPorFoto);
                i = (i + 1) % total;
            }
        }
    }
}
