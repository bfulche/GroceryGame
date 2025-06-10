using UnityEngine;

namespace GroceryGame.Core
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void OnStartInteract(GameObject player);
        void OnEndInteract(GameObject player);
        void OnStartHover();
        void OnEndHover();
        bool CanInteract();
        bool IsHoldable();
    }
}