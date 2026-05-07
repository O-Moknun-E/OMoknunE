using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkOmokManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private BoardInteraction _boardInteraction;
    [SerializeField] private Sprite[] _stoneSkins;

    // 전달 데이터: x좌표, y좌표, 진영번호
    public static event Action<int, int, StoneType> OnStonePlaced;

    private int _mySkinIndex = 0;
    private StoneType _myPlayerType = StoneType.Black;

    //외부(스킬 효과 등)에서 내 진영을 확인할 수 있게 열어줍니다.
    public StoneType MyPlayerType => _myPlayerType;

    private bool _isMasterTurn = true;

    private string _loadedSkillName = "";

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
        if (_boardInteraction == null || _stoneSkins.Length == 0) return;

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
        _boardInteraction.ChangeStoneSkin(_stoneSkins[_mySkinIndex]);
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

    // 외부(스킬 효과)에서 스킨 이미지를 가져갈 수 있게 해주는 함수
    public Sprite GetStoneSkin(int index)
    {
        if (index >= 0 && index < _stoneSkins.Length) return _stoneSkins[index];
        return null;
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

        if (!_hasUsedSkillThisTurn)
        {
            // 1번 키 : 스킬 장전
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _loadedSkillName = "FogSkill"; // 장전
                if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(true);
                Debug.Log("<color=cyan>[System] 안개 스킬 장전 오목판을 클릭하세요</color>");
            }

            // 2번 키 : 스킬 장전
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _loadedSkillName = "FakeStoneSkill";
                if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(true);
                Debug.Log("<color=cyan>[System] 가짜 돌 스킬 장전 오목판을 클릭하세요</color>");
            }

            // 3번 키 : 스킬 장전
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _loadedSkillName = "SealSkill";
                if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(true);
                Debug.Log("<color=cyan>[System] 봉인 결계 스킬 장전 오목판을 클릭하세요</color>");
            }
        }
        else
        {
            // 유저가 스킬을 썼는데 또 1, 2, 3번을 누를 경우 로그 확인
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("<color=red>[System] 이미 이번 턴에 스킬을 사용했습니다! 오목을 두어 턴을 넘기세요.</color>");
            }
        }
        // 마우스 우클릭(1)을 누르면 스킬 장전 취소 (일반 돌 두기로 복귀)
        if (Input.GetMouseButtonDown(1) && !string.IsNullOrEmpty(_loadedSkillName))
        {
            _loadedSkillName = "";
            if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);
            Debug.Log("<color=red>[System] 스킬 장전 취소 일반 착수 모드로 돌아갑니다</color>");
        }

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
        Sprite stoneSkin = _stoneSkins[skinID];
        _boardInteraction.PlaceStoneRemote(x, y, stoneSkin);

        _isMasterTurn = !_isMasterTurn;
        CheckAndApplyTurn();

        _hasUsedSkillThisTurn = false;

        OnStonePlaced?.Invoke(x, y, playerType);
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
        AchievementManager.Instance.achievementTracker.UpdatePlayerGameCount(); //민정추가
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");// 후에 메인메뉴 씬으로 수정 필요 
    }

}