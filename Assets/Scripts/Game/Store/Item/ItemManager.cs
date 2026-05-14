using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    public Action OnLoadItems;

    private Dictionary<string, ItemData> itemDict = new Dictionary<string, ItemData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void LoadItemDatas() //리소스 폴더에 있는 아이템들을 불러와서 읽음
    {
        if (itemDict == null) itemDict = new Dictionary<string, ItemData>();

        itemDict.Clear();

        var datas = Resources.LoadAll<ItemData>("Items");
        foreach (var data in datas)
        {
            if (itemDict.ContainsKey(data.itemId))
            {
                Debug.LogWarning($"중복된 아이템 ID 발견: {data.itemId}. 건너뜀.");
                continue;
            }
            itemDict.Add(data.itemId, data);
        }
        OnLoadItems?.Invoke();
    }

    public ItemData GetItemData(string id)
    {
        if (itemDict.TryGetValue(id, out var data)) return data;

        return null;
    }
}
