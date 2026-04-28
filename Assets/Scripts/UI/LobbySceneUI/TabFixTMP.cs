using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabFixTMP : TMP_InputField
{
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        // Tab 키 감지
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Tab 입력 무시하고 이벤트 넘김
            eventData.Use();
            return;
        }

        base.OnUpdateSelected(eventData);
    }
}