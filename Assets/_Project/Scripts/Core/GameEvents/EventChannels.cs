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
    public SoundEffectGameEvent OnBallCollisionSound;//TODO: I don't like how I made this. Prob should make it more general later


    [Header("Gadget Events")]
    public GameEvent OnManualActivation; 
    public GameEvent OnGamePaused;
    public GameEvent OnGameResumed;
    public GadgetGameEvent OnGadgetPlaced;


}