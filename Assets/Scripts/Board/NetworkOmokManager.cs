using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkOmokManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private BoardInteraction _boardInteraction;

    // 전달 데이터: x좌표, y좌표, 진영번호
    public static event Action<int, int, StoneType> OnStonePlaced;

    private int _mySkinIndex = 0;
    private StoneType _myPlayerType = StoneType.Black;

    //외부(스킬 효과 등)에서 내 진영을 확인할 수 있게 열어줍니다.
    public StoneType MyPlayerType => _myPlayerType;

    private bool _isMasterTurn = true;

    private string _loadedSkillName = "";

    private int _silencedTurnsLeft = 0; // 침묵 턴 변수

    [Header("게임 오버 UI")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _resultText;

    public static bool IsReturningFromGame = false;

    //스킬 사용 여부 체크
    private bool _hasUsedSkillThisTurn = false;
    
    public override void OnEnable()
    {
        base.OnEnable();
        if (_boardInteraction != null)
            _boardInteraction.OnStoneClicked += HandleBoardClick;
        if (OmokManager.Instance != null)
            OmokManager.Instance.OnGameOver += ShowGameOverUI;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (_boardInteraction != null)
            _boardInteraction.OnStoneClicked -= HandleBoardClick;
        if (OmokManager.Instance != null)
            OmokManager.Instance.OnGameOver -= ShowGameOverUI;
    }

    private void Start()
    {
        if (OmokManager.Instance != null)
        {
            OmokManager.Instance.InitGame();
        }

        _mySkinIndex = PlayerPrefs.GetInt("MySkinID", 0);

        if (PhotonNetwork.InRoom) SetupGame();
        else
        {
            if (_boardInteraction != null) _boardInteraction.SetMyTurn(false);
        }
        // 방장(MasterClient) 한 명만 이 명령을 서버에 내림
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            // 1. IsOpen = false : 아무도 이 방에 들어올 수 없음
            PhotonNetwork.CurrentRoom.IsOpen = false;

            // 2. IsVisible = false : 로비 목록에서 아예 방 이름 지움
            PhotonNetwork.CurrentRoom.IsVisible = false;

            Debug.Log("게임이 시작되어 방을 비공개로 잠갔습니다");
        }
    }

    public override void OnJoinedRoom() => SetupGame();

    private void SetupGame()
    {
        if (_boardInteraction == null || StoneSkinRegistry.Instance.GetStoneSkinCount() == 0) return;

        if (PhotonNetwork.IsMasterClient)
        {
            _myPlayerType = StoneType.Black;
            _mySkinIndex = 0;
        }
        else
        {
            _myPlayerType = StoneType.White;
            _mySkinIndex = 1;
        }
        _boardInteraction.ChangeStoneSkin(StoneSkinRegistry.Instance.GetStoneSkin(_mySkinIndex));
        CheckAndApplyTurn();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("새 플레이어 입장 게임을 준비합니다.");
        CheckAndApplyTurn();
    }

    private void CheckAndApplyTurn()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            _boardInteraction.SetMyTurn(false);
            return;
        }

        // 현재 내 턴인지 여부 계산
        bool isMyTurnNow = (_isMasterTurn && _myPlayerType == StoneType.Black) ||
                           (!_isMasterTurn && _myPlayerType == StoneType.White);

        _boardInteraction.SetMyTurn(isMyTurnNow);
    }

    private void HandleBoardClick(int x, int y)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.LogWarning("상대방이 아직 입장하지 않았습니다.");
            return;
        }
        // 핵심 분기: 장전된 스킬 존재 확인
        if (!string.IsNullOrEmpty(_loadedSkillName))
        {
            // 1. 장전된 스킬이 있다면 클릭한 좌표로 생성
            UseSkill(_loadedSkillName, x, y);

            _hasUsedSkillThisTurn = true;

            // 2. 쏘고 나면 빈손으로 만들기 (연발 방지)
            _loadedSkillName = "";
            if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);
        }
        else
        {
            // 장전된 스킬이 없다면 평소처럼 돌 두는 통신
            photonView.RPC("RPC_ReceiveAndDrawStone", RpcTarget.All, x, y, _myPlayerType, _mySkinIndex);
        }
    }

    public void ApplySilence(int turns)
    {
        _silencedTurnsLeft = turns;

        _loadedSkillName = "";
        if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);

        Debug.Log($"<color=red>[System] 침묵에 걸렸습니다! 앞으로 {_silencedTurnsLeft}턴 동안 스킬을 사용할 수 없습니다.</color>");
    }

    // 나중에 UI 상점 버튼이 누를 함수
    public void UIButton_LoadSkill(string skillName)
    {
        _loadedSkillName = skillName;
        Debug.Log($"{skillName} 장전 완료!");
    }

    //// UI제작 시 스킬 장전 방식 변경
    //public void LoadSkill(string skillName)
    //{
    //    _loadedSkillName = skillName;

    //    // 오목판에게 스킬 장전 상태라고 알려줌 (반투명 돌 사라짐)
    //    if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(true);

    //    Debug.Log($"<color=cyan>[System] {skillName} 장전 완료! 오목판을 클릭하세요.</color>");
    //}
    //public void CancelLoadedSkill()
    //{
    //    _loadedSkillName = "";
    //    if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);
    //    Debug.Log("<color=red>[System] 스킬 장전 취소! 일반 착수 모드로 돌아갑니다.</color>");
    //}

    //== 스킬 테스트용 Update 메서드 ==
    //UI가 완성되면 위의 함수로 변경 예정
    private void Update()
    {
        // 스킬 테스트 로직 정리
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        // 내 턴일 때만 키보드 입력 허용
        bool isMyTurnNow = (_isMasterTurn && _myPlayerType == StoneType.Black) ||
                           (!_isMasterTurn && _myPlayerType == StoneType.White);

        if (!isMyTurnNow) return;

        // 마우스 우클릭(1)을 누르면 스킬 장전 취소 (일반 돌 두기로 복귀)
        if (Input.GetMouseButtonDown(1) && !string.IsNullOrEmpty(_loadedSkillName))
        {
            _loadedSkillName = "";
            if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);
            Debug.Log("<color=red>[System] 스킬 장전 취소 일반 착수 모드로 돌아갑니다</color>");
        }

        // 스킬 장전 (TryLoadSkill 함수를 사용해 마나, 침묵, 사용 여부를 한 번에 검사)
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryLoadSkill("FogSkill");
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryLoadSkill("FakeStoneSkill");
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryLoadSkill("SealSkill");
        if (Input.GetKeyDown(KeyCode.Alpha4)) TryLoadSkill("SilenceSkill");
        if (Input.GetKeyDown(KeyCode.Alpha5)) TryLoadSkill("ManaTrapSkill");
        if (Input.GetKeyDown(KeyCode.Alpha6)) TryLoadSkill("TimeOverloadSkill");

        // 엔터키 입력 시 스킬 사용 (침묵 스킬은 좌표 필요 없으므로 바로 발동)
        if (Input.GetKeyDown(KeyCode.Return) && (_loadedSkillName == "SilenceSkill" || _loadedSkillName == "TimeOverloadSkill"))
        {
            // 좌표가 필요 없는 스킬이므로 -1, -1 이라는 가짜 좌표를 던짐
            UseSkill(_loadedSkillName, -1, -1);

            _hasUsedSkillThisTurn = true;
            _loadedSkillName = "";
            if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);

            Debug.Log("<color=yellow>[System] 엔터 키 입력 즉발 스킬 발동 완료</color>");
        }
    }

    // 스킬 장전 조건을 미리 검사하는 함수
    private void TryLoadSkill(string skillName)
    {
        // 침묵 상태인지 먼저 검사
        if (_silencedTurnsLeft > 0)
        {
            Debug.Log($"<color=red>[System] 침묵 상태입니다 남은 턴: {_silencedTurnsLeft}</color>");
            return;
        }

        // 이미 이번 턴에 스킬을 썼는지 검사
        if (_hasUsedSkillThisTurn)
        {
            Debug.Log("<color=red>[System] 이미 이번 턴에 스킬을 사용했습니다 오목을 두세요.</color>");
            return;
        }

        // 마나가 충분한지 검사
        SkillBase skill = Resources.Load<SkillBase>("Skills/" + skillName);
        if (skill != null && OmokManager.Instance != null)
        {
            PlayerType myType = (_myPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;
            Player myPlayer = OmokManager.Instance.GetPlayer(myType);

            // 현재 마나가 스킬 코스트보다 적다면 장전 거부
            if (myPlayer != null && myPlayer.CurrentMana < skill.Cost)
            {
                Debug.Log($"<color=red>[System] 마나가 부족합니다 (필요 마나: {skill.Cost} / 현재 마나: {myPlayer.CurrentMana})</color>");
                return;
            }
        }

        // 스킬 장전
        _loadedSkillName = skillName;
        if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(true);

        // 스킬 종류에 따라 알맞은 안내 로그 출력
        if (skillName == "SilenceSkill" || skillName == "TimeOverloadSkill")
        {
            Debug.Log($"<color=cyan>[System] {skillName} 장전 완료 <Enter> 키를 눌러 발동하세요</color>");
        }
        Debug.Log($"<color=cyan>[System] {skillName} 스킬 장전 완료! 오목판을 클릭하세요.</color>");
    }

    public void UseSkill(string skillName, int x, int y)
    {
        PlayerType myType = (_myPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;
        string path = "Skills/" + skillName;

        // _mySkinIndex 를 맨 뒤에 추가해서 전송
        photonView.RPC("RPC_ExecuteSkill", RpcTarget.All, path, x, y, myType, _mySkinIndex);
    }

    [PunRPC]
    public void RPC_ReceiveAndDrawStone(int x, int y, StoneType playerType, int skinID)
    {
        Sprite stoneSkin = StoneSkinRegistry.Instance.GetStoneSkin(skinID);
        _boardInteraction.PlaceStoneRemote(x, y, stoneSkin);

        _isMasterTurn = !_isMasterTurn;
        CheckAndApplyTurn();

        _hasUsedSkillThisTurn = false;

        OnStonePlaced?.Invoke(x, y, playerType);

        // 전역 이벤트(TurnDuration 등 범용)
        GameEvents.TriggerStonePlaced(x, y, playerType);
    }

    [PunRPC]
    public void RPC_ExecuteSkill(string skillPath, int x, int y, PlayerType casterType, int skinID)
    {
        SkillBase skill = Resources.Load<SkillBase>(skillPath);
        if (skill == null) return;

        // 아까 수정한 SetTarget으로 skinID 전달
        skill.SetTarget(x, y, casterType, skinID);

        if (OmokManager.Instance.TryUseMagic(skill))
        {
            Debug.Log($"[Skill] {casterType}가 {skill.Name}를 ({x}, {y})에 시전!");
        }
    }

    // 게임 종료 시 승자 정보를 받아서 UI를 띄워주는 함수
    private void ShowGameOverUI(StoneType winner)
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);

            if (_resultText != null)
            {
                if (winner == StoneType.Black)
                    _resultText.text = "Black Win / White Lose";
                else if (winner == StoneType.White)
                    _resultText.text = "White Win / Black Lose";
            }
        }
    }
    public void ReturnToMainMenu()
    {
        IsReturningFromGame = true;

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");// 후에 메인메뉴 씬으로 수정 필요 
    }


    //======유니티 내장 UI 이용 테스틑 시간/마나======
    // 유니티 내장 UI 함수로 화면에 시간/마나 띄우기
    private void OnGUI()
    {
        // 씬 로딩 안됐거나, OmokManager가 없거나, 혼자 있을 땐 무시
        if (OmokManager.Instance == null || !PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 35;
        style.fontStyle = FontStyle.Bold;

        // 남은 시간 통일
        float timeLeft = Mathf.Max(0, OmokManager.Instance.CurrentTurnTimeLimit - OmokManager.Instance.TurnTimer);

        bool isMyTurnNow = (_isMasterTurn && _myPlayerType == StoneType.Black) ||
                           (!_isMasterTurn && _myPlayerType == StoneType.White);

        if (isMyTurnNow)
        {
            style.normal.textColor = timeLeft < 5f ? Color.red : Color.yellow; // 5초 남으면 빨간색!
            GUI.Label(new Rect(20, 20, 500, 50), $"[내 턴] 남은 시간: {timeLeft:F1}초", style);
        }
        else
        {
            //상대 턴일 때도 똑같이 남은 시간으로 표시
            style.normal.textColor = Color.gray;
            GUI.Label(new Rect(20, 20, 500, 50), $"[상대 턴] 남은 시간: {timeLeft:F1}초", style);
        }

        // 마나 표시
        PlayerType myType = (_myPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;
        Player myPlayer = OmokManager.Instance.GetPlayer(myType);

        if (myPlayer != null)
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(20, Screen.height - 70, 500, 50), $"💎 내 마나: {myPlayer.CurrentMana}", style);
        }
    }
}