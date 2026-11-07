using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SebraeGuides
{
    /// <summary>
    /// Quando o visitante solta o objeto, ele encolhe e some, como se fosse guardado.
    /// Usado no crachá: antes ele caía no chão e ficava perdido.
    /// </summary>
    public class GuardarAoSoltar : MonoBehaviour
    {
        public float duracao = 0.3f;

        XRBaseInteractable interagivel;
        bool guardando;

        void Awake()
        {
            interagivel = GetComponent<XRBaseInteractable>();
        }

        void OnEnable()
        {
            if (interagivel) interagivel.selectExited.AddListener(AoSoltar);
        }

        void OnDisable()
        {
            if (interagivel) interagivel.selectExited.RemoveListener(AoSoltar);
        }

        void AoSoltar(SelectExitEventArgs args)
        {
            if (guardando || interagivel.isSelected) return;
            StartCoroutine(Guardar());
        }

        IEnumerator Guardar()
        {
            guardando = true;

            // Espera um quadro antes de desligar a interação: desligar dentro do próprio evento de soltar
            // atrapalha o XR Interaction Manager.
            yield return null;
            if (interagivel) interagivel.enabled = false;

            Vector3 escala = transform.localScale;
            float tempo = 0f;
            while (tempo < duracao)
            {
                tempo += Time.deltaTime;
                transform.localScale = escala * (1f - Mathf.Clamp01(tempo / duracao));
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
