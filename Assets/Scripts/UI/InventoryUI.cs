using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Slot slotPrefab;
    public GameObject itemBox;


    [SerializeField]
    private Inventory inven;
    private List<Slot> slotPool = new List<Slot>();

    private ITEM_TYPE currentFilter = ITEM_TYPE.None;

    public void ChangeType(int index)
    {
        currentFilter = (ITEM_TYPE)index;
        MatchingItem(); // 필터가 바뀌었으니 다시 그림
    }

    public void MatchingItem()
    {

        foreach (var slot in slotPool)
        {
            slot.gameObject.SetActive(false);
        }

        if(inven.Items == null) return;

        int slotIndex = 0;
        foreach (var item in inven.Items)
        {

            Slot currentSlot;

            if(slotIndex < slotPool.Count)
                currentSlot = slotPool[slotIndex];  
            else
            {
                currentSlot = Instantiate(slotPrefab, itemBox.transform);
                slotPool.Add(currentSlot);
            }
            currentSlot.gameObject.SetActive(true);
            currentSlot.SetItem(item.ItemImage, item.DisplayName); 

            slotIndex++;
        }
    }


}
