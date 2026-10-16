using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SebraeGuides
{
    /// <summary>
    /// Socket do mural de ideias: só aceita objetos que tenham o componente PostIt,
    /// para o crachá não grudar no quadro.
    /// </summary>
    public class SocketMuralIdeias : XRSocketInteractor
    {
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return base.CanHover(interactable) && EhPostIt(interactable);
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return base.CanSelect(interactable) && EhPostIt(interactable);
        }

        static bool EhPostIt(IXRInteractable interactable)
        {
            return interactable != null && interactable.transform != null &&
                   interactable.transform.GetComponent<PostIt>() != null;
        }
    }
}
