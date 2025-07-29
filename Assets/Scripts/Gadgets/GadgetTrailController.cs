// Filename: GadgetTrailController.cs
using BallMania.VFX; // Your VFX namespace
using UnityEngine;

namespace BallMania.Gameplay // Your gameplay namespace
{
    [RequireComponent(typeof(ICanConnect), typeof(WaveTrailRenderer))]
    public class GadgetTrailController : MonoBehaviour
    {
        // Private references fetched in Awake
        private ICanConnect _connector;
        private WaveTrailRenderer _trailRenderer;

        private void Awake()
        {
            // Get the components on this GameObject
            _connector = GetComponent<ICanConnect>();
            _trailRenderer = GetComponent<WaveTrailRenderer>();
        }

        private void Update()
        {
            // The controller's only job:
            // Get the target from the game logic and assign it to the visual effect.
            if (_connector != null)
            {
                _trailRenderer.target = _connector.GetTargetTransform;
            }
        }
    }
}