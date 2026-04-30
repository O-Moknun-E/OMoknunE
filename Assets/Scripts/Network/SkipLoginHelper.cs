using UnityEngine;
using System.Collections;

public class SkipLoginHelper : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject loginPanel;
    public GameObject lobbyPanel;
    public GameObject startPanel; //민정추가

    void Start()
    {
        //코루틴 실행
        StartCoroutine(CheckAndSkipLogin());
    }

    private IEnumerator CheckAndSkipLogin()
    {
        // PlayFabManager가 로그인 창을 켜는 작업이 다 끝난 직후에 코드 실행
        yield return new WaitForEndOfFrame();

        // 게임을 끝내고 돌아온 상태라면 즉시 로비로 이동
        if (NetworkOmokManager.IsReturningFromGame)
        {
            if (startPanel != null) loginPanel.SetActive(false);//민정추가
            if (loginPanel != null) loginPanel.SetActive(false);
            if (lobbyPanel != null) lobbyPanel.SetActive(true);

            // 다음번을 위해 초기화
            NetworkOmokManager.IsReturningFromGame = false;

            Debug.Log("로그인 스킵 성공!");
        }
    }
}