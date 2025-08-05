// Filename: PausedState.cs
// Location: _Project/Scripts/Gameplay/States/
using Core.Events;
using UnityEngine;

public class PausedState : IState
{
    private readonly EventChannels _eventChannels;

    // We can give states the dependencies they need!
    public PausedState(EventChannels eventChannels)
    {
        _eventChannels = eventChannels;
    }

    public void Enter()
    {
        Time.timeScale = 0f;
        _eventChannels.OnGamePaused.Raise(); // Announce that the game is paused
        Debug.Log("Entered Paused State");
    }

    public void Exit()
    {
        _eventChannels.OnGameResumed.Raise(); // Announce that the game is resuming
    }

    public void Tick() { }
}