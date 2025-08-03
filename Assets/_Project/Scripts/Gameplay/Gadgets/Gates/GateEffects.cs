// Filename: GateEffects.cs
using Gameplay.BallSystem;
using UnityEngine;

namespace Gameplay.Gadgets.Effects
{
    [System.Serializable]
    public class AddValueEffect : IGateEffect
    {
        [Tooltip("The amount to add to the ball's price.")]
        public int AmountToAdd;

        public void Apply(BallData ballData) => ballData.AddPrice(AmountToAdd);
    }

    [System.Serializable]
    public class MultiplyValueEffect : IGateEffect
    {
        [Tooltip("The value to multiply the ball's price by.")]
        public float Multiplier;

        public void Apply(BallData ballData) => ballData.MultiplyPrice(Multiplier);
    }

    [System.Serializable]
    public class SubtractValueEffect : IGateEffect
    {
        [Tooltip("The amount to subtract from the ball's price.")]
        public int AmountToSubtract;

        public void Apply(BallData ballData) => ballData.SubtractPrice(AmountToSubtract);
    }
}