using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class PlayerEquipItem : PersistentSingleton<PlayerEquipItem>
{

    public Sprite customPicture { get; private set; }
    public Sprite customStone { get; private set; }
    public Sprite customBord { get; private set; }

    private const string StoneKey = "EquippedStoneID";
    private const string PictureKey = "EquippedPictureID";
    private const string BordKey = "EquippedBordID";


    private void OnEnable()
    {
        PlayFabManager.Instance.OnLogin += LoadEquippedSkinsFromServer;
    }

    private void OnDisable()
    {
        PlayFabManager.Instance.OnLogin -= LoadEquippedSkinsFromServer;
    }
    public void PictureItem(Sprite picture) => this.customPicture = picture;

    public void StoneItem(Sprite stone = null)
    {
        this.customStone = stone;
        int skinID = SkinRegistry.Instance.GetStoneID(stone);

        SaveEquippedSkinToServer(skinID);
    }

    public void BordItem(Sprite bord) => this.customBord = bord;

    private void SaveEquippedSkinToServer(int skinID)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { StoneKey, skinID.ToString() } }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("스킨 저장 성공"),
            error => Debug.LogError("스킨 저장 실패"));
    }

    // [추가] 서버에서 숫자 ID 불러와서 적용
    public void LoadEquippedSkinsFromServer()
    {
        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey(StoneKey))
            {
                // 문자열로 온 밸류를 숫자로 바꿈
                if (int.TryParse(result.Data[StoneKey].Value, out int skinID))
                {
                    // 기존 Registry의 GetStoneSkin을 써서 스프라이트 복구
                    customStone = SkinRegistry.Instance.GetStoneSkin(skinID);
                    Debug.Log($"서버에서 스킨 ID {skinID} 로드 완료");
                }
            }
        }, error => Debug.LogError("서버 데이터 로드 실패"));
    }

}
