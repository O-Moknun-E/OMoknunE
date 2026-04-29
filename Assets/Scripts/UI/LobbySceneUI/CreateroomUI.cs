using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateroomUI : MonoBehaviour
{

    public TMP_InputField roomNameInput;
    public TMP_InputField passwordInput;

    public Toggle isPrivateToggle;
    public GameObject passwordField;

    public Roomlistmanager Roomlistmanager;

    int roomCount = 1;

    public void OnToggleChanged()
    {
        passwordField.SetActive(isPrivateToggle.isOn);
    }

    public GameObject panel;

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void CreateRoom()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
            roomName = "Room" + roomCount;

        int type = isPrivateToggle.isOn ? 1 : 0;

        string password = passwordInput.text;

        Roomlistmanager.CreateRoom(roomName, type,password );

        


        // 나중에 password도 같이 넘길 수 있음

        roomCount++;

        // 초기화
        roomNameInput.text = "";
        passwordInput.text = "";
        isPrivateToggle.isOn = false;
        passwordField.SetActive(false);
    }
}

