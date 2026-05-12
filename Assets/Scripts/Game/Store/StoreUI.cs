using UnityEngine;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private Store store; // Store 스크립트 참조
   // [SerializeField] private StoreSlot slotPrefab; // 상점용 슬롯 프리팹
    [SerializeField] private GameObject itemBox; // 슬롯이 담길 부모 오브젝트

    private void OnEnable()
    {
        UpdateStoreUI();
    }

    public void UpdateStoreUI()
    {
        // 기존 슬롯 초기화 (인벤토리 UI의 ResetAllSlots 방식 참고)
        foreach (Transform child in itemBox.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 아이템 매니저에서 모든 아이템 데이터를 가져와 표시
        // 여기서는 예시로 "Stone", "Bord", "Picture" 등의 데이터를 순회함
        // 실제로는 ItemManager에 전체 리스트를 반환하는 함수를 추가하는 것이 좋음
    }

    // 버튼 클릭 시 호출될 함수 (인스펙터에서 연결)
    public void OnClickPurchase(string itemId, int price)
    {
        store.BuyItem(itemId, price);
    }
}
