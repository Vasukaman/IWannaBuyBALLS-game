using UnityEngine;

[RequireComponent(typeof(Ball))]
public class BallScaler : MonoBehaviour
{
    [SerializeField] private float baseScale = 1f;
    [SerializeField] private float scalePerPrice = 0.01f;

    private Ball _ball;

    private void Awake()
    {
        _ball = GetComponent<Ball>();
        _ball.OnPriceChanged += HandlePriceChanged;
        HandlePriceChanged(_ball.CurrentPrice); // Set scale at start
    }

    private void OnDestroy()
    {
        if (_ball != null)
            _ball.OnPriceChanged -= HandlePriceChanged;
    }

    private void HandlePriceChanged(int newPrice)
    {
        float scale = baseScale + newPrice * scalePerPrice;
        transform.localScale = Vector3.one * scale;
    }
}
