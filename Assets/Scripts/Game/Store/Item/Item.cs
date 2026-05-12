using System;
using UnityEngine;
using UnityEngine.UI;


[Flags]
public enum ITEM_TYPE
{
    None = 0,
    Stone = 1 << 0,   
    Bord = 1 << 1,    
    Picture = 1 << 2, 
    All = Stone | Bord | Picture
}

public interface IUsable
{
    void Use();
}


public  class Item : MonoBehaviour
{
    public string itemId { get; protected set; }
    public string displayName { get; protected set; }
    public Sprite itemImage{ get; protected set; }
    public ITEM_TYPE type { get; protected set; }

    protected uint price;

    [SerializeField] protected ItemData data;

    public void Initialize(PlayFab.ClientModels.ItemInstance instance, ItemData meta)
    {
        itemId = instance.ItemId;
        displayName = instance.DisplayName;
        itemImage = meta.itemIcon;
        price = instance.UnitPrice;
        type = meta.itemType; 
        data = meta;
    }

}
