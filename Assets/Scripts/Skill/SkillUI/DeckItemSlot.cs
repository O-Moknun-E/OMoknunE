using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckItemSlot : MonoBehaviour
{
    [Header("UI 연결")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI costText;

    private SkillBase _skill;

    public void SetSlot(SkillBase skill)
    {
        _skill = skill;
        if (iconImage != null) iconImage.sprite = skill.Icon;
        if (nameText != null) nameText.text = skill.DisplayName;
        if (descText != null) descText.text = skill.Description;
        if (costText != null) costText.text = skill.Cost.ToString(); // 표시용으로 남겨둠
    }

    // 덱에 있는 카드를 클릭했을 때 실행할 함수
    public void OnClickDeckCard()
    {
        if (_skill == null) return;

        NetworkOmokManager netManager = FindFirstObjectByType<NetworkOmokManager>();
        if (netManager != null)
        {
            netManager.LoadSkillFromDeck(_skill.name, transform.parent.gameObject);
        }
    }
}