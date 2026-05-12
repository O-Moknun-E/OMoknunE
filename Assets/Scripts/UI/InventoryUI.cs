using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Slot slotPrefab;
    public GameObject itemBox;


    [SerializeField]
    private Inventory inven;
    private List<Slot> slotPool = new List<Slot>();

    private ITEM_TYPE currentFilter = ITEM_TYPE.All;

    private void OnEnable()
    {
        UpdateInventoryUI();
    }

    public void ChangeType(int index)
    {

        currentFilter = (ITEM_TYPE)index;
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        ResetAllSlots();

        if (inven == null || inven.Items == null) return;

        var filteredItems = GetFilteredItems();

        for (int i = 0; i < filteredItems.Count; i++)
        {
            Slot slot = GetOrCreateSlot(i);

            slot.SetItem(filteredItems[i].itemImage, filteredItems[i].displayName);
            slot.gameObject.SetActive(true);
        }
    }

    private List<Item> GetFilteredItems()
    {
        List<Item> filteredList = new List<Item>();

        if (inven == null || inven.Items == null) return filteredList;

        if (currentFilter == ITEM_TYPE.All) return inven.Items;


        foreach (Item item in inven.Items)
        {
            if (item == null) continue;

            if ((item.type & currentFilter) != 0)
                filteredList.Add(item);
        }

        return filteredList;
    }

    private void ResetAllSlots()
    {
        foreach (var slot in slotPool)
        {
            slot.gameObject.SetActive(false);
        }
    }

    private Slot GetOrCreateSlot(int index)
    {
        if (index < slotPool.Count)
        {
            return slotPool[index];
        }

        Slot newSlot = Instantiate(slotPrefab, itemBox.transform);
        slotPool.Add(newSlot);
        return newSlot;
    }

}
