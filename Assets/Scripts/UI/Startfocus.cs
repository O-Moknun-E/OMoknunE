using UnityEngine;
using UnityEngine.EventSystems;


public class StartFocus : MonoBehaviour
{
    public GameObject firstInput;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstInput);
    }
}
