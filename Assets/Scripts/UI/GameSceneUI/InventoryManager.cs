// InventoryManager.cs - 새 파일로 만드세요
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance; // 어디서든 접근 가능하게

    public GameObject inventoryPanel;        // 인벤토리 UI 패널
    public InventorySlot[] slots;            // 인벤토리 슬롯 배열
    public Sprite[] itemIcons;              // 아이템 아이콘들 (Inspector에서 연결)
    public GameObject shopPanel; //상점창
    // itemIcons[0] = 아이템ID 1번 아이콘
    // itemIcons[1] = 아이템ID 2번 아이콘 ... 이런 식으로
    private bool showingShop = true;

    public void TogglePanels()
    {
        if (showingShop)
        {
            // 상점 끄고 인벤토리 켜기
            shopPanel.SetActive(false);
            inventoryPanel.SetActive(true);
        }
        else
        {
            // 인벤토리 끄고 상점 켜기
            inventoryPanel.SetActive(false);
            shopPanel.SetActive(true);
        }

        showingShop = !showingShop;
    }
    void Awake()
    {
        // 싱글톤 패턴 - ShopManager에서 쉽게 호출하려고
        instance = this;
    }

    void Start()
    {
        {
            shopPanel.SetActive(true);
            inventoryPanel.SetActive(false);
        }
        //// 게임 시작하면 모든 슬롯 숨기기
        //foreach (InventorySlot slot in slots)
        //{
        //    slot.gameObject.SetActive(false);
        //}

    }

    // ShopManagerScript의 Buy()에서 이 함수를 호출할 거예요
    public void UpdateInventory(int itemID, int quantity)
    {
        // slots 배열은 0부터 시작하니까 itemID-1로 접근
        int slotIndex = itemID - 1;

        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            Sprite icon = null;
            if (slotIndex < itemIcons.Length)
                icon = itemIcons[slotIndex];

            slots[slotIndex].UpdateSlot(quantity, icon);
        }
    }

    //// 인벤토리 열기/닫기 토글
    //public void ToggleInventory()
    //{
    //    inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    //}
}