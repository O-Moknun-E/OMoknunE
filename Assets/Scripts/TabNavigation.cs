using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    void Update()
    {
        // Tab 눌렸을 때
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current == null)
            {
                Debug.Log("현재 선택된 UI 없음");
                return;
            }

            Selectable selectable = current.GetComponent<Selectable>();
            if (selectable == null)
            {
                Debug.Log("Selectable 아님");
                return;
            }

            Selectable next;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                next = selectable.FindSelectableOnUp();
            else
                next = selectable.FindSelectableOnDown();

            if (next != null)
            {
                Debug.Log("다음 선택: " + next.name);

                EventSystem.current.SetSelectedGameObject(next.gameObject);

                // TMP/InputField 둘 다 대응
                var input = next.GetComponent<InputField>();
                if (input != null) input.ActivateInputField();

                var tmp = next.GetComponent<TMPro.TMP_InputField>();
                if (tmp != null) tmp.ActivateInputField();
            }
            else
            {
                Debug.Log("다음 selectable 없음");
            }
        }
    }
}