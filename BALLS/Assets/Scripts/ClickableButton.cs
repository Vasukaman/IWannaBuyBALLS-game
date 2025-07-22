using UnityEngine;
using UnityEngine.Events;

public class ClickableButton : MonoBehaviour, IButton
{
    public event UnityAction OnClicked;

    // Call this method from your actual button component
    public void TriggerClick()
    {
        Debug.Log("Clicked!");
        OnClicked?.Invoke();
    }

    private void OnMouseDown()
    {
        TriggerClick();
    }

    }