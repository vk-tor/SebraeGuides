using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SebraeGuides
{
    /// <summary>
    /// O guia: vira para o visitante, troca de lugar entre as paradas
    /// e avisa quando é selecionado (botão lateral apontando para ele).
    /// </summary>
    public class GuiaNPC : MonoBehaviour
    {
        [Tooltip("Parte visual do guia. Encolhe e cresce na troca de lugar. Troque o conteúdo pelo modelo final.")]
        public Transform modelo;

        public bool olharParaVisitante = true;
        public float velocidadeGiro = 5f;
        public float duracaoTroca = 0.25f;

        public event Action AoSelecionar;

        XRBaseInteractable interagivel;
        Transform cabeca;
        Vector3 escalaOriginal = Vector3.one;
        Coroutine troca;

        void Awake()
        {
            interagivel = GetComponentInChildren<XRBaseInteractable>(true);
            if (modelo) escalaOriginal = modelo.localScale;
        }

        void OnEnable()
        {
            if (interagivel) interagivel.selectEntered.AddListener(AoSerSelecionado);
        }

        void OnDisable()
        {
            if (interagivel) interagivel.selectEntered.RemoveListener(AoSerSelecionado);
        }

        void AoSerSelecionado(SelectEnterEventArgs args)
        {
            AoSelecionar?.Invoke();
        }

        public void Posicionar(Transform alvo)
        {
            if (!alvo) return;
            transform.SetPositionAndRotation(alvo.position, alvo.rotation);
        }

        public void IrPara(Transform alvo)
        {
            if (!alvo) return;
            if (troca != null) StopCoroutine(troca);
            troca = StartCoroutine(Trocar(alvo));
        }

        IEnumerator Trocar(Transform alvo)
        {
            yield return Escalar(1f, 0f);
            Posicionar(alvo);
            yield return Escalar(0f, 1f);
            troca = null;
        }

        IEnumerator Escalar(float de, float para)
        {
            if (!modelo) yield break;
            float tempo = 0f;
            while (tempo < duracaoTroca)
            {
                tempo += Time.deltaTime;
                float k = Mathf.Clamp01(tempo / duracaoTroca);
                modelo.localScale = escalaOriginal * Mathf.Lerp(de, para, k);
                yield return null;
            }
            modelo.localScale = escalaOriginal * para;
        }

        void Update()
        {
            if (!olharParaVisitante) return;
            if (!cabeca)
            {
                var cam = Camera.main;
                if (!cam) return;
                cabeca = cam.transform;
            }

            Vector3 direcao = cabeca.position - transform.position;
            direcao.y = 0f;
            if (direcao.sqrMagnitude < 0.01f) return;

            var alvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, alvo, Time.deltaTime * velocidadeGiro);
        }
    }
}
