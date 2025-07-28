using System;
using System.Numerics;
using UnityEngine;

namespace Game.Economy
{
    /// <summary>
    /// When something connects to something else, we can use it to get needed tranforms
    /// </summary>
    public interface IActivator
    {
        Transform GetStartTransform { get; }
        Transform GetTargetTransform { get; }

        public event Action OnActivate;


    }
}
