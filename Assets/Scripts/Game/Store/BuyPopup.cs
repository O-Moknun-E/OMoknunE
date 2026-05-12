using TMPro;
using UnityEngine;

public class BuyPopup : MonoBehaviour
{
    public static BuyPopup Instance; // 싱글톤

    [SerializeField] private Store store;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private GameObject panel;

    private string currentId;
    private int currentPrice;

    private void Awake() => Instance = this;

    public void OpenPopup(string id, int price, string name)
    {
        currentId = id;
        currentPrice = price;
        confirmText.text = $"{name}을(를) {price}원에 구매하시겠습니까?";
        panel.SetActive(true);
    }

    public void OnClickConfirm()
    {
        store.BuyItem(currentId, (uint)currentPrice);
        panel.SetActive(false);
    }

    public void OnClickCancel() => panel.SetActive(false);
}
