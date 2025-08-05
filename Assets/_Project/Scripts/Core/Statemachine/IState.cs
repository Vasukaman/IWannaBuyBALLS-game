// Filename: IState.cs
// Location: _Project/Scripts/Core/StateMachine/
public interface IState
{
    void Enter();
    void Exit();
    void Tick(); // For frame-by-frame logic
}