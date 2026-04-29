using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PrivateroomUI : MonoBehaviour
{
    public GameObject panel;
    public Roomlistmanager Roomlistmanager;
    public TMP_InputField passwordInput;

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    int roomCount = 1;

    
    
    // 2. 실제 방 생성 (확인 버튼용)
    public void ConfirmPrivateRoom()
    {
        string password = passwordInput.text;

        if (password == "")
        {
            Debug.Log("비밀번호 입력 안함");
            return;
        }

        string roomName = "Private Room " + roomCount;

        // 1. 방 리스트에 추가
        Roomlistmanager.CreateRoom(roomName, 1, password);

        roomCount++;

        // 2. 패널 닫기
        panel.SetActive(false);

        // 3. 게임씬 이동 ⭐
        SceneManager.LoadScene("Gamescreen");

        Debug.Log("비번: " + password);
    }

}

