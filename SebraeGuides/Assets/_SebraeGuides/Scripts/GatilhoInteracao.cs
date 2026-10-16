using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SebraeGuides
{
    /// <summary>
    /// Coloque no mesmo objeto de um XR Grab Interactable, XR Simple Interactable
    /// ou XR Socket Interactor. Quando o objeto é selecionado, toca um som, vibra o
    /// controle, dispara 'aoAtivar' e envia o evento para o ControleVisita.
    /// </summary>
    [DisallowMultipleComponent]
    public class GatilhoInteracao : MonoBehaviour
    {
        [Tooltip("Evento enviado ao ControleVisita (ex.: pegou_cracha). Pode ficar vazio.")]
        public string evento;

        public bool somenteUmaVez = true;
        public AudioClip som;

        [Range(0f, 1f)]
        public float vibracao = 0.4f;

        public UnityEvent aoAtivar = new UnityEvent();

        XRBaseInteractable interagivel;
        XRBaseInteractor interator;
        bool disparado;

        void Awake()
        {
            interagivel = GetComponent<XRBaseInteractable>();
            interator = GetComponent<XRBaseInteractor>();
            if (!interagivel && !interator)
                Debug.LogWarning($"[Gatilho] {name} não tem XR Interactable nem XR Interactor.", this);
        }

        void OnEnable()
        {
            if (interagivel) interagivel.selectEntered.AddListener(AoSelecionar);
            if (interator) interator.selectEntered.AddListener(AoSelecionar);
        }

        void OnDisable()
        {
            if (interagivel) interagivel.selectEntered.RemoveListener(AoSelecionar);
            if (interator) interator.selectEntered.RemoveListener(AoSelecionar);
        }

        void AoSelecionar(SelectEnterEventArgs args)
        {
            if (disparado && somenteUmaVez) return;
            disparado = true;

            if (vibracao > 0f && args.interactorObject is XRBaseInputInteractor controle)
                controle.SendHapticImpulse(vibracao, 0.1f);

            if (som) AudioSource.PlayClipAtPoint(som, transform.position, 0.8f);

            aoAtivar?.Invoke();
            if (!string.IsNullOrEmpty(evento)) ControleVisita.Enviar(evento);
        }

        /// <summary>Permite disparar de novo (útil para testes).</summary>
        public void Rearmar()
        {
            disparado = false;
        }
    }
}
