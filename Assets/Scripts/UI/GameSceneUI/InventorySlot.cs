// InventorySlot.cs - 새 파일로 만드세요
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public int itemID;           // 이 슬롯이 어떤 아이템인지
    public Image itemImage;      // 아이템 아이콘 이미지
    public TextMeshProUGUI quantityText; // 수량 텍스트
    public GameObject quantityBackground; // 수량 뱃지 배경 (선택사항)

    // 슬롯 업데이트 (외부에서 호출)
    //public void UpdateSlot(int quantity, Sprite icon = null)
    //{
    //    if (icon != null)
    //        itemImage.sprite = icon;

    //    if (quantity > 0)
    //    {
    //        // 아이템 있으면 보이게
    //        gameObject.SetActive(true);
    //        quantityText.text = "x" + quantity.ToString();
    //    }
    //    else
    //    {
    //        // 수량 0이면 숨기기 (취향에 따라 그냥 "x0" 표시해도 됨)
    //        gameObject.SetActive(false);
    //    }
    public void UpdateSlot(int quantity, Sprite icon = null)
    {
        if (quantity > 0)
        {
            gameObject.SetActive(true);  // 구매하면 나타남
            quantityText.text = "x" + quantity.ToString();
            if (icon != null)
                itemImage.sprite = icon;
        }
        else
        {
            gameObject.SetActive(false); // 0개면 숨김
        }
    }
}
