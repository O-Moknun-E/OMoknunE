using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopCardUI : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    public void Setup(CardData data, bool alreadyOwned, Action onBuy)
    {
        if (data.cardImage != null)
            cardImage.sprite = data.cardImage;

        cardNameText.text = data.cardName;
        priceText.text = data.price + "G";

        buyButton.onClick.RemoveAllListeners();

        if (alreadyOwned)
        {
            buyButtonText.text = "보유중";
            buyButton.interactable = false;
        }
        else
        {
            buyButtonText.text = "구매";
            buyButton.interactable = true;
            buyButton.onClick.AddListener(() => onBuy());
        }
    }
}