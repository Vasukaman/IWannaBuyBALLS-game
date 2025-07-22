// Ball.cs - Core functionality
using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [Header("Pricing")]
    [SerializeField] private int basePrice = 10;
    [SerializeField] protected TMP_Text priceText;

    private IBallFactory _ballFactory;
    private int _currentPrice;
    private CircleCollider2D _collider;
    public Color color = Color.white;
    public event Action<Ball> OnRequestDespawn;
    public event Action<int> OnPriceChanged;
    public event Action<Ball> OnDespawned;
    public event Action<Ball> OnInitialize;
    public int CurrentPrice => _currentPrice;
    public IBallFactory BallFactory => _ballFactory;
    public CircleCollider2D Collider => _collider;
    public float Radius => _collider.radius * transform.lossyScale.x;

    protected virtual void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        ResetToBase();
    }

    public void SetBallFactory(IBallFactory factory) => _ballFactory = factory;
    public void ResetToBase() => SetPrice(basePrice);
    public void SetBasePrice(int newPrice) => basePrice = newPrice;

   public void Initialize()
    {
        ResetToBase();
        OnInitialize.Invoke(this);
    }
    public void SetPrice(int price)
    {
        _currentPrice = price;
        OnPriceChanged?.Invoke(_currentPrice);
        UpdateTextPrice();
    }

    protected void UpdateTextPrice()
    {
        if (priceText) priceText.text = _currentPrice.ToString();
    }

    public void ModifyPrice(Func<int, int> modifier)
    {
        int old = _currentPrice;
        _currentPrice = Mathf.Max(0, modifier(_currentPrice));
        if (_currentPrice != old) OnPriceChanged?.Invoke(_currentPrice);
        UpdateTextPrice();
    }

    public void AddPrice(int amount) => ModifyPrice(old => old + amount);
    public void MultiplyPrice(float multiplier) => ModifyPrice(old => Mathf.RoundToInt(old * multiplier));
    public void SubtractPrice(int amount) => ModifyPrice(old => old - amount);

    public void Despawn()
    {
        OnDespawned?.Invoke(this);
        OnRequestDespawn?.Invoke(this);
    }

    public float GetShaderTrueSize() => Radius;
}