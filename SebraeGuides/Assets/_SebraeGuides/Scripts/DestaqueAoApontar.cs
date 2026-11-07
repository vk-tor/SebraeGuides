using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SebraeGuides
{
    /// <summary>
    /// Mostra ao visitante que o raio está em cima do objeto: a parte visual cresce um pouco
    /// e o controle dá uma vibração curta. Volta ao normal quando o raio sai ou o objeto é pego.
    /// </summary>
    public class DestaqueAoApontar : MonoBehaviour
    {
        [Tooltip("Parte que cresce. Vazio = o próprio objeto.")]
        public Transform alvo;

        public float escalaDestaque = 1.15f;

        [Range(0f, 1f)]
        public float vibracao = 0.15f;

        XRBaseInteractable interagivel;
        Vector3 escalaNormal = Vector3.one;

        void Awake()
        {
            interagivel = GetComponent<XRBaseInteractable>();
            if (!alvo) alvo = transform;
            escalaNormal = alvo.localScale;
        }

        void OnEnable()
        {
            if (!interagivel) return;
            interagivel.hoverEntered.AddListener(AoApontar);
            interagivel.hoverExited.AddListener(AoSair);
            interagivel.selectEntered.AddListener(AoPegar);
        }

        void OnDisable()
        {
            if (interagivel)
            {
                interagivel.hoverEntered.RemoveListener(AoApontar);
                interagivel.hoverExited.RemoveListener(AoSair);
                interagivel.selectEntered.RemoveListener(AoPegar);
            }
            if (alvo) alvo.localScale = escalaNormal;
        }

        void AoApontar(HoverEnterEventArgs args)
        {
            if (interagivel.isSelected) return;
            alvo.localScale = escalaNormal * escalaDestaque;
            if (vibracao > 0f && args.interactorObject is XRBaseInputInteractor controle)
                controle.SendHapticImpulse(vibracao, 0.05f);
        }

        void AoSair(HoverExitEventArgs args)
        {
            if (!interagivel.isHovered) alvo.localScale = escalaNormal;
        }

        void AoPegar(SelectEnterEventArgs args)
        {
            alvo.localScale = escalaNormal;
        }
    }
}
