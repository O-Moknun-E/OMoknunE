using UnityEngine;

public class SkillDeckManager : MonoBehaviour
{
    public static SkillDeckManager Instance;
    public Transform deckContainer;      // 하단 덱의 컨테이너
    public GameObject skillCardPrefab;   // 하단 덱에 들어갈 카드 프리팹 (ShopItemSlot이 붙어있는 프리팹)

    void Awake() => Instance = this;

    public void AddSkillToDeck(SkillBase skill)
    {
        GameObject newCard = Instantiate(skillCardPrefab, deckContainer);

        DeckItemSlot cardSlot = newCard.GetComponentInChildren<DeckItemSlot>();
        if (cardSlot != null)
        {
            cardSlot.SetSlot(skill); // 상점과 똑같이 아이콘, 이름, 설명 세팅
        }
        else
        {
            Debug.LogError("생성된 카드에서 DeckItemSlot 스크립트를 찾을 수 없습니다");
        }
    }
}