using System;
using System.Collections;
using UnityEngine;

namespace SebraeGuides
{
    /// <summary>
    /// Anima o guia junto com o balão: gesticula enquanto uma fala está sendo "dita"
    /// e acena nas falas de boas-vindas e de despedida.
    /// O Animator (Guia.controller) é criado pelo menu Sebrae Guides com os estados
    /// Parado, Falando e Acenando.
    /// </summary>
    public class AnimacaoGuia : MonoBehaviour
    {
        public Animator animador;
        public DialogoGuia dialogo;

        [Tooltip("IDs das falas em que o guia acena.")]
        public string[] falasComAceno = { "R1", "E1", "FIM" };

        [Header("Duração do gesto quando a fala não tem áudio")]
        [Tooltip("Segundos por letra do texto da fala.")]
        public float segundosPorLetra = 0.055f;
        public float duracaoMinima = 2f;
        public float duracaoMaxima = 9f;

        static readonly int ParamFalando = Animator.StringToHash("Falando");
        static readonly int ParamAcenar = Animator.StringToHash("Acenar");

        Coroutine gesto;

        void OnEnable()
        {
            if (!dialogo) return;
            dialogo.AoMostrarFala += AoMostrarFala;
            dialogo.AoFechar += Parar;
        }

        void OnDisable()
        {
            if (dialogo)
            {
                dialogo.AoMostrarFala -= AoMostrarFala;
                dialogo.AoFechar -= Parar;
            }
            Parar();
        }

        void AoMostrarFala(Fala fala)
        {
            if (!animador || fala == null) return;

            if (Array.IndexOf(falasComAceno, fala.id) >= 0) animador.SetTrigger(ParamAcenar);

            if (gesto != null) StopCoroutine(gesto);
            gesto = StartCoroutine(Falar(Duracao(fala)));
        }

        float Duracao(Fala fala)
        {
            if (fala.audio) return fala.audio.length;
            int letras = string.IsNullOrEmpty(fala.texto) ? 0 : fala.texto.Length;
            return Mathf.Clamp(letras * segundosPorLetra, duracaoMinima, duracaoMaxima);
        }

        IEnumerator Falar(float segundos)
        {
            animador.SetBool(ParamFalando, true);
            yield return new WaitForSeconds(segundos);
            animador.SetBool(ParamFalando, false);
            gesto = null;
        }

        void Parar()
        {
            if (gesto != null) StopCoroutine(gesto);
            gesto = null;
            if (animador) animador.SetBool(ParamFalando, false);
        }
    }
}
