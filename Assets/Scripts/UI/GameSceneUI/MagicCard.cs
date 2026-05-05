using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagicCard : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descText;
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    private MagicData magicData;
    private MagicShopPanel shopPanel;

    public void Setup(MagicData data, MagicShopPanel panel)
    {
        magicData = data;
        shopPanel = panel;

        nameText.text = data.magicName;
        priceText.text = $"◆ {data.price} G";
        descText.text = data.description;

        buyButton.onClick.RemoveAllListeners();

        if (data.owned)
        {
            buyButtonText.text = "보유 중";
            buyButton.interactable = false;
        }
        else
        {
            buyButtonText.text = "구매";
            buyButton.interactable = true;
            buyButton.onClick.AddListener(() => shopPanel.BuyMagic(magicData));
        }
    }
}

