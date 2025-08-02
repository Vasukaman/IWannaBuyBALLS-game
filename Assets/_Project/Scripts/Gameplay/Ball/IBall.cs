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
