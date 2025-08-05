// Filename: GameManager.cs
// Location: _Project/Scripts/Core/
using Reflex.Attributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Inject] private GameStateMachine _fsm;

    private void Start()
    {
        // When the game starts, immediately enter the Playing state.
        _fsm.Enter<PlayingState>();
    }

    private void Update()
    {
        _fsm.Tick();
    }
}