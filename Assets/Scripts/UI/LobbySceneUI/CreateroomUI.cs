using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class CreateroomUI : MonoBehaviour  //비공개/공개때문에 갈아엎음 기존스크립트는 맨 아래 주석으로 달아놓음
{
    public TMP_InputField roomNameInput;
    public TMP_InputField passwordInput;

    // 기존 Toggle 대신 사용할 텍스트
    public TMP_Text privateText; // ← private/public 글자

    // 비밀번호 입력창
    public GameObject passwordField;

    // 자물쇠 이미지
    public Image lockImage;

    // 자물쇠 스프라이트
    public Sprite lockSprite;     // 잠긴 자물쇠
    public Sprite unlockSprite;   // 열린 자물쇠

    // 현재 private 상태 저장
    bool isPrivate = true;

    public Roomlistmanager Roomlistmanager;

    int roomCount = 1;

    void Start()
    {
        UpdatePrivacyUI();
    }

    public GameObject panel;

    // -----------------------------
    // 패널 열기
    // -----------------------------
    public void OpenPanel()
    {
        panel.SetActive(true);

        // 패널 열 때 UI 초기화
        UpdatePrivacyUI();
    }

    // -----------------------------
    // 패널 닫기
    // -----------------------------
    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    // -----------------------------
    // 화살표 버튼 눌렀을 때
    // private ↔ public 변경
    // -----------------------------
    public void ChangePrivacy()
    {
        isPrivate = !isPrivate;

        UpdatePrivacyUI();
    }

    // -----------------------------
    // UI 변경 함수
    // -----------------------------
    void UpdatePrivacyUI()
    {
        if (isPrivate)
        {
            // private 상태
            privateText.text = "private";

            // 비밀번호 입력창 켜기
            passwordField.SetActive(true);

            // 잠긴 자물쇠
            lockImage.sprite = lockSprite;
        }
        else
        {
            // public 상태
            privateText.text = "public";

            // 비밀번호 입력창 끄기
            passwordField.SetActive(false);

            // 열린 자물쇠
            lockImage.sprite = unlockSprite;

            // public이면 비밀번호 삭제
            passwordInput.text = "";
        }
    }

    // -----------------------------
    // 방 만들기
    // -----------------------------
    public void CreateRoom()
    {
        string roomName = roomNameInput.text;

        if (string.IsNullOrEmpty(roomName))
            roomName = "Room" + roomCount;

        // private = 1
        // public = 0
        int type = isPrivate ? 1 : 0;

        string password = passwordInput.text;

        Roomlistmanager.CreateRoom(roomName, type, password);

        roomCount++;

        // 초기화
        roomNameInput.text = "";
        passwordInput.text = "";

        // 기본값 private
        isPrivate = true;

        UpdatePrivacyUI();
    }
}

////using UnityEngine;
////using TMPro;
////using UnityEngine.UI;


////public class CreateroomUI : MonoBehaviour
////{

////    public TMP_InputField roomNameInput;
////    public TMP_InputField passwordInput;

////    public Toggle isPrivateToggle;
////    public GameObject passwordField;

////    public Roomlistmanager Roomlistmanager;

////    int roomCount = 1;


////    public void OnToggleChanged()
////    {
////        passwordField.SetActive(isPrivateToggle.isOn);

////    }

////    public GameObject panel;

////    public void OpenPanel()
////    {
////        panel.SetActive(true);
////    }

////    public void ClosePanel()
////    {
////        panel.SetActive(false);
////    }

////    public void CreateRoom()
////    {
////        string roomName = roomNameInput.text;
////        if (string.IsNullOrEmpty(roomName))
////            roomName = "Room" + roomCount;

////        int type = isPrivateToggle.isOn ? 1 : 0;

////        string password = passwordInput.text;

////        Roomlistmanager.CreateRoom(roomName, type, password);




////        // 나중에 password도 같이 넘길 수 있음

////        roomCount++;

////        // 초기화
////        roomNameInput.text = "";
////        passwordInput.text = "";
////        isPrivateToggle.isOn = false;
////        passwordField.SetActive(false);
////    }
////}



