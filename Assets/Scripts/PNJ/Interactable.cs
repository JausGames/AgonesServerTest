using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected Canvas interactionTextCanvas;
    public void ShowInteractionText()
    {
        interactionTextCanvas.enabled = true;
    }
    public void HideInteractionText()
    {
        interactionTextCanvas.enabled = false;
    }

    abstract public void Interact(Player player);
    abstract public void CloseInteract(Player player);
}
