using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

public class StoreUI : MonoBehaviour
{
    public Slot slotPrefab; 
    public GameObject itemBox;
    public GameObject CheckPopup;
    public TextMeshProUGUI currnteMoney;

    [SerializeField] private Store store;
    private List<Slot> slotPool = new List<Slot>();
    private List<CatalogItem> allStoreItems = new List<CatalogItem>(); 
    private ITEM_TYPE currentFilter = ITEM_TYPE.All;

    private void OnEnable()
    {
        store.OnStoreLoaded += StoreLoaded;
        store.LoadStoreCatalog(); // 상점 열 때 로드
        store.OnBuyItem += GetUserMoney;
    }

    private void OnDisable()
    {
        store.OnStoreLoaded -= StoreLoaded;
        store.OnBuyItem -= GetUserMoney;
    }

    private void StoreLoaded(List<CatalogItem> catalog)
    {
        allStoreItems = catalog;
        UpdateStoreUI();
    }

    public void ChangeType(int index)
    {
        currentFilter = (ITEM_TYPE)index;
        UpdateStoreUI();
    }

    public void UpdateStoreUI()
    {
        ResetAllSlots();
        GetUserMoney();

        var filteredItems = GetFilteredCatalog();

        for (int i = 0; i < filteredItems.Count; i++)
        {
            Slot slot = GetOrCreateSlot(i);
            var catalogItem = filteredItems[i];

            ItemData data = ItemManager.Instance.GetItemData(catalogItem.ItemId);
            if (data != null)
            {
                uint price = catalogItem.VirtualCurrencyPrices.ContainsKey("SD")
                             ? catalogItem.VirtualCurrencyPrices["SD"] : 0;

                slot.SetStoreSlot(data.itemIcon, $"{data.itemName}\n{price} SD", data.itemId, (int)price);
                slot.gameObject.SetActive(true);
            }
        }
    }

    private List<CatalogItem> GetFilteredCatalog()
    {
        if (currentFilter == ITEM_TYPE.All) return allStoreItems;

        List<CatalogItem> filteredList = new List<CatalogItem>();
        foreach (var item in allStoreItems)
        {
            ItemData data = ItemManager.Instance.GetItemData(item.ItemId);
            if (data != null && (data.itemType & currentFilter) != 0)
            {
                filteredList.Add(item);
            }
        }
        return filteredList;
    }

    private void ResetAllSlots()
    {
        foreach (var slot in slotPool) slot.gameObject.SetActive(false);
    }

    private Slot GetOrCreateSlot(int index)
    {
        if (index < slotPool.Count) return slotPool[index];
        Slot newSlot = Instantiate(slotPrefab, itemBox.transform);
        slotPool.Add(newSlot);
        return newSlot;
    }

    public void GetUserMoney()
    {
        var request = new GetUserInventoryRequest();

        PlayFabClientAPI.GetUserInventory(request,
            result => {
                if (result.VirtualCurrency.TryGetValue("SD", out int balance))
                {
                    currnteMoney.text = $"{balance} SD";
                    Debug.Log("금액변경");
                }
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
}
