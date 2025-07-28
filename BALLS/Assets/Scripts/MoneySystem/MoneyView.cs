using UnityEngine;
using TMPro;            // or UnityEngine.UI if you prefer Text
using Game.Economy;
using Reflex.Attributes;

[RequireComponent(typeof(TMP_Text))]
public class MoneyView : MonoBehaviour
{
    [Inject] private IMoneyService _moneyService;
    private TMP_Text _text;

    void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        _moneyService.OnBalanceChanged += UpdateDisplay;
        UpdateDisplay(_moneyService.CurrentBalance);
    }

    void OnDisable()
    {
        _moneyService.OnBalanceChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int newBalance)
    {
        _text.text = $"{newBalance}$";
    }
}
