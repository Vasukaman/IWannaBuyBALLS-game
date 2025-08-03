using Gameplay.BallSystem;
using UnityEngine;

namespace Gameplay.Gadgets.Effects
{
    // The [System.Serializable] attribute is ESSENTIAL for this to work.
    [System.Serializable]
    public class AddValueEffect : IGateEffect
    {
        public void Apply(BallData ballData, int ammount) => ballData.AddPrice(ammount);
    }

    [System.Serializable]
    public class MultiplyValueEffect : IGateEffect
    {
        public void Apply(BallData ballData, int ammount) => ballData.MultiplyPrice(ammount);
    }

    [System.Serializable]
    public class SubtractValueEffect : IGateEffect
    {
        public void Apply(BallData ballData, int ammount) => ballData.SubtractPrice(ammount);
    }
}
