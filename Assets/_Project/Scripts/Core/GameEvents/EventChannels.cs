// Filename: EventChannels.cs
using Core.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "EventChannels", menuName = "Game Events/Event Channel Library")]
public class EventChannels : ScriptableObject
{
    [Header("Ball Events")]
    public IntGameEvent OnBallSold; 
    public GameEvent OnBallSpawned;
    public BallGameEvent OnBallMerged;


    [Header("Gadget Events")]
    public GameEvent OnManualActivation; 
    public GameEvent OnGamePaused;
    public GameEvent OnGameResumed;

}