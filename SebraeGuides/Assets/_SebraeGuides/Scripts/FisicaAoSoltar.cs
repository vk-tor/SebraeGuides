using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SebraeGuides
{
    /// <summary>
    /// Deixa o objeto parado no lugar (sem gravidade) até o visitante pegar.
    /// Depois de solto, ele passa a cair normalmente. Evita que objetos pequenos
    /// afundem no balcão ou na estante antes de serem usados.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FisicaAoSoltar : MonoBehaviour
    {
        Rigidbody corpo;
        XRBaseInteractable interagivel;

        void Awake()
        {
            corpo = GetComponent<Rigidbody>();
            interagivel = GetComponent<XRBaseInteractable>();
            corpo.isKinematic = true;
            corpo.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
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
            if (interagivel.isSelected) return;
            corpo.isKinematic = false;
            corpo.WakeUp();
        }
    }
}
