using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayList : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Replay _replay;
    [SerializeField] private Transform _contentParent;      // 스크롤 뷰의 Content
    [SerializeField] private GameObject _replayItemPrefab;  // 리플레이 항목 프리팹

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _emptyText;    // 리플레이가 없을 때 표시할 텍스트

    private List<ReplayItemUI> _replayItems = new List<ReplayItemUI>();

    public static Replay.ReplayData SelectedReplayData { get; private set; }    // 다른 씬에서 선택된 리플레이 데이터를 참조할 수 있도록 static으로 선언

    private void Start()
    {
        LoadreplayList();
    }

    /// <summary>
    /// PlayFab에서 리플레이 목록 가져오기
    /// </summary>
    private void LoadreplayList()
    {
        // replay script가 없으면 무시
        if (_replay == null)
        {
            Debug.LogError("Replay 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        // 기존 목록 초기화
        ClearReplayList();

        // PlayFab에서 리플레이 목록 요청
        _replay.GetReplayListFromPlayFab(
            onSuccess: (metadataList) =>
            {
                // 리플레이가 없는 경우
                if(metadataList == null || metadataList.Count == 0)
                {
                    ShowEmptyMessage(true);
                    return;
                }

                // 리플레이 목록 표시
                ShowEmptyMessage(false);
                DisplayReplayList(metadataList);
            },
            onError: (error) =>
            {

            });
    }

    /// <summary>
    /// 리플레이 목록을 UI에 표시
    /// </summary>
    /// <param name="metadataList">리플레이 메타데이터 목록</param>
    private void DisplayReplayList(List<Replay.ReplayMetadata> metadataList)
    {
        // 최신 리플레이가 위로 오도록 정렬
        metadataList.Sort((a, b) => b.createdAt.CompareTo(a.createdAt));

        foreach(var metadata in metadataList)
        {
            // 프리팹 생성
            GameObject itemObj = Instantiate(_replayItemPrefab, _contentParent);
            ReplayItemUI itemUI = itemObj.GetComponent<ReplayItemUI>();

            if(itemUI != null)
            {
                // 데이터 설정
                itemUI.SetData(metadata);

                // 클릭 이벤트 연결
                itemUI.OnPlayClicked += () => LoadAndPlayReplay(metadata.replayID);

                _replayItems.Add(itemUI);
            }
        }
    }

    /// <summary>
    /// 기존 리플레이 목록 제거
    /// </summary>
    private void ClearReplayList()
    {
        foreach(var item in _replayItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        _replayItems.Clear();
    }

    /// <summary>
    /// 빈 목록 메시지 표시
    /// </summary>
    /// <param name="show">메시지를 표시할지 여부</param>
    /// <param name="meesage">표시할 메시지</param>
    private void ShowEmptyMessage(bool show, string meesage = "저장된 리플레이가 없습니다.")
    {
        if(_emptyText != null)
        {
            _emptyText.text = meesage;
            _emptyText.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// 리플레이 불러오기
    /// </summary>
    public void LoadAndPlayReplay(string replayID)
    {
        Debug.Log($"리플레이 불러오는 중: {replayID}");

        _replay.LoadReplayFromPlayFab(replayID,
            onSuccess: (replayData) =>
            {
                // 불러온 리플레이 데이터 저장
                SelectedReplayData = replayData;

                Debug.Log("리플레이 불러오기 완료!");

                SceneManager.LoadScene("ReplaySceneUI");
            },
            onError: (error) =>
            {
                Debug.LogError($"리플레이 불러오기 실패: {error}");
            });
    }
}
