using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Button _button;
    [SerializeField] private CanvasGroup _canvasGroup;

    private GadgetData _gadgetData;

    public void Initialize(GadgetData data, int price, System.Action<GadgetData> onClick)
    {
        _gadgetData = data;
        _iconImage.sprite = data.icon;
        _nameText.text = data.displayName;

        _button.onClick.AddListener(() => onClick(data));
        UpdateDisplay(price, true); // Initial update
    }

    public void UpdateDisplay(int price, bool canAfford)
    {
        _priceText.text = $"{price}";
        _priceText.color = canAfford ? Color.green : Color.red;
        _canvasGroup.alpha = canAfford ? 1f : 0.5f;
        _button.interactable = canAfford;
    }
}