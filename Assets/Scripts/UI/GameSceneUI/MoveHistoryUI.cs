using UnityEngine;
using TMPro;

public class MoveHistoryUI : MonoBehaviour
{
    // ✅ 다른 스크립트에서 쉽게 접근하기 위한 싱글톤
    // 이제 어디서든 MoveHistoryUI.instance 사용 가능
    public static MoveHistoryUI instance;

    public Transform content;          // Scroll View의 Content
    public GameObject moveItemPrefab;  // 수순 한 줄 프리팹

    int moveCount = 1;

    // ✅ 게임 시작될 때 자기 자신을 instance에 저장
    void Awake()
    {
        instance = this;
    }

    // ✅ 수순 추가 함수
    // player = 플레이어 이름
    // x, y = 착수 좌표
    public void AddMove(string player, int x, int y)
    {
        // 프리팹 생성해서 Content 안에 넣기
        GameObject item = Instantiate(moveItemPrefab, content);

        // 프리팹 안 TMP_Text 가져오기
        TMP_Text text = item.GetComponent<TMP_Text>();

        // 수순 표시
        // 예: 1. Black (3,5)
        text.text = moveCount + ". " + player + " (" + x + "," + y + ")";

        // 다음 번호 증가
        moveCount++;
    }
}

//using UnityEngine;
//using TMPro;

//public class MoveHistoryUI : MonoBehaviour
//{
//    public Transform content;        // Content
//    public GameObject moveItemPrefab; // 프리팹

//    int moveCount = 1;

//    public void AddMove(string player, int x, int y)
//    {
//        GameObject item = Instantiate(moveItemPrefab, content);

//        TMP_Text text = item.GetComponent<TMP_Text>();
//        text.text = moveCount + ". " + player + " (" + x + "," + y + ")";

//        moveCount++;


//    }

//}