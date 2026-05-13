using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    public Image iconImage;            
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;   
    public TextMeshProUGUI costText;   

    private SkillBase _assignedSkill;   // 이 슬롯에 배정된 스킬 데이터

    // 매니저가 스킬 데이터를 넣어줄 때 호출하는 함수
    public void SetSlot(SkillBase skill)
    {
        _assignedSkill = skill;

        if (skill != null)
        {
            // 우리가 SkillBase에 만든 변수들을 UI에 꽂아줍니다!
            if (iconImage != null) iconImage.sprite = skill.Icon;
            if (nameText != null) nameText.text = skill.DisplayName;
            if (descText != null) descText.text = skill.Description;
            if (costText != null) costText.text = skill.Cost.ToString();

            gameObject.SetActive(true); // 데이터가 있으면 활성화
        }
        else
        {
            gameObject.SetActive(false); // 데이터가 없으면 슬롯 숨기기
        }
    }

    // 아이템 클릭 시 실행될 함수
    public void OnClickSlot()
    {
        if (_assignedSkill == null) return;

        // 부모인 ShopPanelManager를 찾아 현재 슬롯이 선택되었음을 알림
        ShopPanelManager manager = GetComponentInParent<ShopPanelManager>();
        if (manager != null)
        {
            manager.OnSelectSlot(this);
        }
    }

    public SkillBase GetSkill()
    {
        return _assignedSkill;
    }
}