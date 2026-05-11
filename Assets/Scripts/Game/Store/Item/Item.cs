using System;
using UnityEngine;
using UnityEngine.UI;


[Flags]
public enum ITEM_TYPE
{ 
    None = 0,
    Stone = 1 <<0,
    Bord = 1 << 1,
    Picture = 1 << 2,
    All = 1 << 3,
}

public interface IUsable
{
    void Use();
}


public  class Item : MonoBehaviour
{
    protected string itemId;
    protected string displayName;
    protected Sprite itemImage;
    protected uint price;

    [SerializeField] protected ITEM_TYPE Type;
    [SerializeField] protected ItemData data;

    public string ItemID => itemId;
    public Sprite ItemImage => itemImage;
    public string DisplayName => displayName;


    public Item(PlayFab.ClientModels.ItemInstance instance, ItemData meta)
    {
        itemId = instance.ItemId;
        displayName = instance.DisplayName;
        itemImage = meta.itemIcon;
        price = instance.UnitPrice;
        data = meta;
    }

    public  void Initialize(PlayFab.ClientModels.ItemInstance instance, ItemData meta)
    {
        itemId = instance.ItemId;
        displayName = instance.DisplayName;
        data = meta;
    }
}
