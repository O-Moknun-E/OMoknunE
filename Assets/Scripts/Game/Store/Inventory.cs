using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;


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
        return items.Exists(item => item.itemId == targetItemId);
    }


    private void OnLoadItemSuccess(GetUserInventoryResult result)
    {
        var inventoryData = result.Inventory;

        if (items.Count > inventoryData.Count)
        {
            for (int i = items.Count - 1; i >= inventoryData.Count; i--)
            {
                Destroy(items[i].gameObject);
                items.RemoveAt(i);
            }
        }
        for (int i = 0; i < inventoryData.Count; i++)
        {
            var instance = inventoryData[i];
            ItemData data = ItemManager.Instance.GetItemData(instance.ItemId);
            if (data == null) continue;

            if (i < items.Count)
            {
                items[i].Initialize(instance, data);
                Debug.Log($"데이터 갱신: {items[i].displayName}");
            }
            else
            {
                if (data.itemPrefab != null)
                {
                    GameObject obj = Instantiate(data.itemPrefab, transform);
                    Item newItem = obj.GetComponent<Item>();
                    newItem.Initialize(instance, data);
                    items.Add(newItem);
                    Debug.Log($"신규 생성: {newItem.displayName}");
                }
            }
        }
    }
}
