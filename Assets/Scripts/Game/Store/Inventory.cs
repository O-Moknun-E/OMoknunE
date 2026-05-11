using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    private List<Item> items = new List<Item>();

    public List<Item>Items => items;

    public void LoadItem() //가지고 있는 아이템 리스트를 불러옴
    {
        var request = new GetUserInventoryRequest();

        PlayFabClientAPI.GetUserInventory(request,
            result => OnLoadItemSuccess(result),
            error => Debug.LogError(error.GenerateErrorReport()));
    }

    private void OnEnable()
    {
        if (ItemManager.Instance != null)
            ItemManager.Instance.OnLoadItems += LoadItem;
        else
        {
            gameObject.AddComponent<ItemManager>();
            ItemManager.Instance.OnLoadItems += LoadItem;
        }

    }

    private void OnDisable()
    {
        ItemManager.Instance.OnLoadItems -= LoadItem;
    }


    public bool HasItem(string targetItemId) //플레이어가 해당 아이템이 있는지 확인
    {
        return items.Exists(item => item.ItemID == targetItemId);
    }


    private void OnLoadItemSuccess(GetUserInventoryResult result)
    {
        items.Clear();

        foreach (var instance in result.Inventory)
        {
            ItemData so = ItemManager.Instance.GetItemData(instance.ItemId);

            if (so != null)
            {
                Item newItem = new Item(instance, so);
                items.Add(newItem);
                Debug.Log(newItem.ItemID);
            }
        }

    }
}
