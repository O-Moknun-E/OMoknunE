using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MagicCardShopUI : MonoBehaviour
{
    [Header("탭 패널")]
    public GameObject shopPanel;
    public GameObject inventoryPanel;

    [Header("탭 버튼")]
    public Button shopTabButton;
    public Button inventoryTabButton;

    [Header("골드")]
    public TextMeshProUGUI goldText;
    public int gold = 1000;

    [Header("카드 데이터 11개")]
    public CardData[] cards = new CardData[11];

    [Header("프리팹")]
    public GameObject shopCardPrefab;
    public GameObject ownedCardPrefab;

    [Header("카드 생성 위치")]
    public Transform shopContent;
    public Transform inventoryContent;

    private List<int> purchasedCardIds = new List<int>();

    void Start()
    {
        shopTabButton.onClick.AddListener(OpenShop);
        inventoryTabButton.onClick.AddListener(OpenInventory);

        UpdateGoldUI();
        LoadShopCards();
        OpenShop();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        inventoryPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        shopPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        LoadInventory();
    }

    void LoadShopCards()
    {
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            GameObject obj = Instantiate(shopCardPrefab, shopContent);
            ShopCardUI ui = obj.GetComponent<ShopCardUI>();
            ui.Setup(cards[i], purchasedCardIds.Contains(i), () => BuyCard(index));
        }
    }

    void BuyCard(int cardIndex)
    {
        if (purchasedCardIds.Contains(cardIndex))
        {
            Debug.Log("이미 구매한 카드!");
            return;
        }

        if (gold < cards[cardIndex].price)
        {
            Debug.Log("골드 부족!");
            return;
        }

        gold -= cards[cardIndex].price;
        purchasedCardIds.Add(cardIndex);
        UpdateGoldUI();
        LoadShopCards();
    }

    void LoadInventory()
    {
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);

        foreach (int id in purchasedCardIds)
        {
            GameObject obj = Instantiate(ownedCardPrefab, inventoryContent);
            OwnedCardUI ui = obj.GetComponent<OwnedCardUI>();
            ui.Setup(cards[id]);
        }
    }

    void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = "골드: " + gold + "G";
    }
}