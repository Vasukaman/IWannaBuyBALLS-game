using System.Numerics;
using UnityEngine;

namespace Game.Economy
{
    /// <summary>
    /// When something connects to something else, we can use it to get needed tranforms
    /// </summary>
    public interface ICanConnect
    {
        Transform GetStartTransform { get; }
        Transform GetEndTransform { get; }

  
    }
}
