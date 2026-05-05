using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagicShopPanel : MonoBehaviour
{
    [Header("탭")]
    public GameObject shopPanel;
    public GameObject inventoryPanel;
    public Button shopTabBtn;
    public Button inventoryTabBtn;

    [Header("상점")]
    public Transform shopContent;      // Scroll View > Content
    public GameObject magicCardPrefab; // 카드 프리팹

    [Header("보유 마법")]
    public Transform inventoryContent;
    public GameObject inventorySlotPrefab;

    [Header("골드")]
    public TextMeshProUGUI goldText;
    private int currentGold = 1200;

    void Start()
    {
        shopTabBtn.onClick.AddListener(() => SwitchTab(true));
        inventoryTabBtn.onClick.AddListener(() => SwitchTab(false));
        RefreshGoldUI();
        SwitchTab(true); // 처음엔 상점 탭
        SpawnShopCards();
    }

    void SwitchTab(bool isShop)
    {
        shopPanel.SetActive(isShop);
        inventoryPanel.SetActive(!isShop);
        if (!isShop) RefreshInventory();
    }

    void RefreshGoldUI() => goldText.text = $"◆ {currentGold:N0} G";

    public void BuyMagic(MagicData data)
    {
        if (currentGold < data.price) { Debug.Log("골드 부족!"); return; }
        currentGold -= data.price;
        data.owned = true;
        RefreshGoldUI();
        SpawnShopCards(); // 카드 새로고침
    }

    void SpawnShopCards()
    {
        foreach (Transform child in shopContent) Destroy(child.gameObject);
        foreach (var magic in MagicDatabase.allMagics)
        {
            var card = Instantiate(magicCardPrefab, shopContent);
            card.GetComponent<MagicCard>().Setup(magic, this);
        }
    }

    void RefreshInventory()
    {
        foreach (Transform child in inventoryContent) Destroy(child.gameObject);
        foreach (var magic in MagicDatabase.allMagics)
        {
            if (magic.owned)
            {
                var slot = Instantiate(inventorySlotPrefab, inventoryContent);
                slot.GetComponent<InventorySlot>().Setup(magic);
            }
        }
    }
}