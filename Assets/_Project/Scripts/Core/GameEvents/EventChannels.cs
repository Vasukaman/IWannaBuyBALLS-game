// Filename: EventChannels.cs
using Core.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "EventChannels", menuName = "Game Events/Event Channel Library")]
public class EventChannels : ScriptableObject
{
    [Header("Ball Events")]
    public IntGameEvent OnBallSold; // Assign your BallSoldEvent.asset here
    public GameEvent OnBallSpawned; // Assign your BallSpawnedEvent.asset here

    [Header("Gadget Events")]
    public GameEvent OnManualActivation; // Assign your ManualActivationEvent.asset here

    // Add new event channels here as your game grows...
}