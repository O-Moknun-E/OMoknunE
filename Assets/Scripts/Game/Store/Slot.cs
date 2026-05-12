using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Slot : MonoBehaviour
{
    private int price;
    private string itemId;
    [SerializeField]private Image itemImage;
    [SerializeField]private TextMeshProUGUI itemName;

    private ITEM_TYPE type = new ITEM_TYPE();

    public void SetStoreSlot(Sprite itemImage, string itemName, string itemId, int price = 0)
    {
        this.itemImage.sprite = itemImage;
        this.itemName.text = itemName;
        this.price = price;
        this.itemId = itemId;
    }

    public void SetInvenSlot(Sprite itemImage, string itemName, string itemId, ITEM_TYPE type)
    {
        this.itemImage.sprite = itemImage;
        this.itemName.text = itemName;
        this.itemId = itemId;
        this.type = type;
    }

    public void OnClickBuy()
    {
        BuyPopup.Instance.OpenPopup(itemId, price, itemName.text);
    }

    public void OnCilckChoice()
    {
        switch (type)
        {
            case ITEM_TYPE.Stone:
                PlayerEquipItem.Instance.StoneItem(itemImage.sprite);
                break;
            case ITEM_TYPE.Bord:
                PlayerEquipItem.Instance.BordItem(itemImage.sprite);
                break;
            case ITEM_TYPE.Picture:
                PlayerEquipItem.Instance.PictureItem(itemImage.sprite);
                break;
        }
    }


}
