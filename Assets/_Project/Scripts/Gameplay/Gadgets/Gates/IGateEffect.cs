// Filename: IGateEffect.cs
using Gameplay.BallSystem;

namespace Gameplay.Gadgets.Effects
{
    public interface IGateEffect
    {
        void Apply(BallData ballData, int ammount);
    }
}
