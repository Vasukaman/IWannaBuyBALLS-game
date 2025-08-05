// Filename: PlayingState.cs
// Location: _Project/Scripts/Gameplay/States/
using UnityEngine;

public class PlayingState : IState
{
    public void Enter()
    {
        Time.timeScale = 1f;
        Debug.Log("Entered Playing State");
    }

    public void Exit() { }
    public void Tick() { } // Gameplay logic happens in your other components
}