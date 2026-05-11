using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image blindImage;
    public Image itemImage;
    public TextMeshProUGUI itemName;

    public void SetItem(Sprite itemImage,string itemName)
    {
        blindImage.gameObject.SetActive(false); 
        this.itemImage.sprite = itemImage;
        this.itemName.text = itemName;
    }


}
