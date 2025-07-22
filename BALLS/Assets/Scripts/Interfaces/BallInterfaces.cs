using System;
using UnityEngine;

public interface IBall
{
    int CurrentPrice { get; }
    float Radius { get; }
    Transform Transform { get; }
    void MultiplyPrice(float multiplier);
    void Despawn();
    event Action<IBall> OnDespawned;
    event Action<int> OnPriceChanged;
    void RaisePriceChanged(int newPrice); // <-- Add this helper;
}

public interface IBallMerger
{
    void HandleCollision(Collision2D collision);
    void StartMergeWith(IBall other);
}

public interface IBallVisualizer
{
    void UpdateMergeShaderParams(IBall target, IBall other, float weight, float otherRadius);
    void ResetMergeShaderParams();
    void VisualizeDebugMerge(IBall debugTargetBall, float debugWeight);
}

public interface IBallPricing
{
    int CurrentPrice { get; }
    void ResetToBase();
    void SetBasePrice(int newPrice);
    void ModifyPrice(Func<int, int> modifier);
    void AddPrice(int amount);
    void MultiplyPrice(float mul);
    void SubtractPrice(int amount);
}