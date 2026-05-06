using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OwnedCardUI : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI cardNameText;

    public void Setup(CardData data)
    {
        if (data.cardImage != null)
            cardImage.sprite = data.cardImage;

        cardNameText.text = data.cardName;
    }
}