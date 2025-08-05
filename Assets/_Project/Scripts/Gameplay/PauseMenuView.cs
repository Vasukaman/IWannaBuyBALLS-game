// Filename: PauseMenuView.cs
// Location: _Project/Scripts/UI/
using Core.Events;
using Reflex.Attributes;
using UnityEngine;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenuRoot;
    [Inject] private EventChannels _eventChannels;

    private void Awake()
    {
        _eventChannels.OnGamePaused.RegisterListener(Show);
        _eventChannels.OnGameResumed.RegisterListener(Hide);
        Hide(); // Start with the menu hidden
    }

    private void OnDestroy()
    {
        _eventChannels.OnGamePaused.UnregisterListener(Show);
        _eventChannels.OnGameResumed.UnregisterListener(Hide);
    }

    private void Show() => _pauseMenuRoot.SetActive(true);
    private void Hide() => _pauseMenuRoot.SetActive(false);
}