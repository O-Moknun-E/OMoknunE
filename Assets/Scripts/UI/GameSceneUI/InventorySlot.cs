using UnityEngine;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public TextMeshProUGUI InventorynameText;
    public TextMeshProUGUI InventorydescText;

    public void Setup(MagicData data)
    {
        InventorynameText.text = data.magicName;
        InventorydescText.text = data.description;
    }
}