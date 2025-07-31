// Filename: GadgetSellTrigger.cs
using Gameplay.Gadgets;
using System;
using UnityEngine;

namespace Gameplay.Gadgets
{
    [RequireComponent(typeof(Collider2D))]
    public class GadgetSellTrigger : MonoBehaviour
    {
        public event Action<IGadgetSellable> OnGadgetEntered;
        public event Action<IGadgetSellable> OnGadgetExited;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IGadgetSellable>(out var sellable))
            {
                OnGadgetEntered?.Invoke(sellable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<IGadgetSellable>(out var sellable))
            {
                OnGadgetExited?.Invoke(sellable);
            }
        }
    }
}