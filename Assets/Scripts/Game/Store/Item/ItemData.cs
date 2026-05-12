using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public uint itemPrice;
    public string itemId;
    public string itemName;
    public GameObject itemPrefab;
    public Sprite itemIcon;
    public ITEM_TYPE itemType;
}
