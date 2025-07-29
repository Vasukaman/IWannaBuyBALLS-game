using System;
using System.Numerics;
using UnityEngine;


    /// <summary>
    /// When something connects to something else, we can use it to get needed tranforms
    /// </summary>
    public interface ICanConnect
    {
        Transform GetStartTransform { get; }
        Transform GetTargetTransform { get; }

        public event Action OnActivate;


    }

