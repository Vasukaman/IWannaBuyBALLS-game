// Filename: PauseController.cs
// Location: _Project/Scripts/Gameplay/Input/
using Reflex.Attributes;
using UnityEngine;



//THis script is in wrong place and should use events or smt.
//I will change it later, but I'm too tired with the project and need to finish it ASAP
public class PauseController : MonoBehaviour
{
    [Inject] private GameStateMachine _fsm;
    private bool _isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                _fsm.Enter<PlayingState>();
                _isPaused = false;
            }
            else
            {
                _fsm.Enter<PausedState>();
                _isPaused = true;
            }
        }
    }
}