using UnityEngine;
using UnityEngine.UI;

public class ShopPanelManager : MonoBehaviour
{
    public ShopItemSlot[] itemSlots;
    public Button enchantButton;      // ENCHANT 버튼 연결

    private ShopItemSlot _selectedSlot; // 현재 선택된 슬롯

    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.7f, 0.7f, 0.7f);
    void Start()
    {
        InitializeShop();
    }

    public void InitializeShop()
    {
        SkillBase[] allSkills = Resources.LoadAll<SkillBase>("Skills");
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < allSkills.Length) itemSlots[i].SetSlot(allSkills[i]);
            else itemSlots[i].SetSlot(null);
        }
    }

    // 슬롯이 클릭되었을 때 호출
    public void OnSelectSlot(ShopItemSlot slot)
    {
        if (_selectedSlot != null && _selectedSlot.iconImage != null)
        {
            _selectedSlot.iconImage.color = normalColor;
        }

        _selectedSlot = slot;

        if (_selectedSlot != null && _selectedSlot.iconImage != null)
        {
            _selectedSlot.iconImage.color = selectedColor;
        }

        Debug.Log($"{slot.name} 슬롯 선택됨");
    }

    // ENCHANT 버튼 클릭 시 실행
    public void OnEnchantClick()
    {
        if (_selectedSlot == null)
        {
            Debug.Log("먼저 스킬을 선택하세요");
            return;
        }

        SkillBase skill = _selectedSlot.GetSkill();
        if (skill == null) return;

        NetworkOmokManager netManager = Object.FindFirstObjectByType<NetworkOmokManager>();
        PlayerType myType = (netManager.MyPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

        Player myPlayer = OmokManager.Instance.GetPlayer(myType);

        // 플레이어를 못 찾았다면 에러 방지
        if (myPlayer == null)
        {
            Debug.LogError("내 플레이어 정보를 찾을 수 없습니다");
            return;
        }

        if (myPlayer.CurrentMana >= skill.Cost)
        {
            myPlayer.AddMana(-skill.Cost);
            SkillDeckManager.Instance.AddSkillToDeck(skill);

            OmokManager.Instance.ForceUpdateManaUI();
            Debug.Log($"{skill.DisplayName} 구입 완료");
        }
        else
        {
            Debug.Log("마나가 부족합니다");
        }
    }
}