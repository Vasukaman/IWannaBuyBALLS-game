// Filename: IConnectionSource.cs
using System;
using UnityEngine;

namespace Gameplay.Interfaces
{
    /// <summary>
    /// Represents any object that can act as a source for a visual connection.
    /// It provides a start and end point for the connection.
    /// </summary>
    public interface IConnectionSource
    {
     

        Transform StartTransform { get; }
        Transform TargetTransform { get; }
    }
}