// Filename: ClickableButton.cs
using Core.Events;
using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// A simple component that raises a specified GameEvent when its collider is clicked.
    /// </summary>

    public class ClickableButton : MonoBehaviour
    {
        [Header("Event Channel")]
        [Tooltip("The GameEvent asset to raise when this button is clicked.")]
        [SerializeField] private GameEvent _interactionEvent;

        public void TriggerClick()
        {
            if (_interactionEvent != null)
            {
                _interactionEvent.Raise();
            }
        }
    }
}