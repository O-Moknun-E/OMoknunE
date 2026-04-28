using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Roomitem : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text countText;
    public GameObject passwordPanel; // 비밀번호 입력 패널

    public void Setup(string roomName, int count)
    {
        nameText.text = roomName;
        countText.text = count + "/2";
    }
    

    public void OnClickRoom()
    {
        passwordPanel.SetActive(true);
    }
}
