using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SebraeGuides
{
    /// <summary>
    /// Socket do mural de ideias: só aceita o post-it, e só depois que a ideia foi escrita nele.
    /// Assim o crachá não gruda no quadro e não dá para colar um post-it em branco.
    /// </summary>
    public class SocketMuralIdeias : XRSocketInteractor
    {
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return base.CanHover(interactable) && EhPostItEscrito(interactable);
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return base.CanSelect(interactable) && EhPostItEscrito(interactable);
        }

        static bool EhPostItEscrito(IXRInteractable interactable)
        {
            if (interactable == null || interactable.transform == null) return false;
            var postIt = interactable.transform.GetComponent<PostIt>();
            return postIt != null && postIt.Escrito;
        }
    }
}
