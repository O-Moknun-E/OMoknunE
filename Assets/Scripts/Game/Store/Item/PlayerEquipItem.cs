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
    public void PictureItem(Sprite picture)
    {
        this.customPicture = picture;
        int skinID = SkinRegistry.Instance.GetPictureID(picture);
        SaveEquippedSkinToServer(skinID, PictureKey);
    }

    public void StoneItem(Sprite stone = null)
    {
        this.customStone = stone;
        int skinID = SkinRegistry.Instance.GetStoneID(stone);

        SaveEquippedSkinToServer(skinID, StoneKey);
    }

    public void BordItem(Sprite bord)
    {
        this.customBord = bord; // [중요] customStone이 아님봄!

        // [중요] GetStoneID가 아니라 GetBordID를 호출해야 함봄!
        int skinID = SkinRegistry.Instance.GetBordID(bord);

        SaveEquippedSkinToServer(skinID, BordKey);
    }

    private void SaveEquippedSkinToServer(int skinID, string key)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { key, skinID.ToString() } }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log($"{key} 저장 성공"),
            error => Debug.LogError($"{key} 저장 실패"));
    }
    

    public void LoadEquippedSkinsFromServer()
    {
        if(!PlayFabManager.Instance.SuccessLogin)
            return;

        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data == null) return;

            customStone = LoadSkinFromData(result.Data, StoneKey, SkinRegistry.Instance.GetStoneSkin);
            customPicture = LoadSkinFromData(result.Data, PictureKey, SkinRegistry.Instance.GetPictureSkin);
            customBord = LoadSkinFromData(result.Data, BordKey, SkinRegistry.Instance.GetBordSkin);

            Debug.Log("서버 데이터 로드 프로세스 완료");
        }, error => Debug.LogError("서버 데이터 로드 실패"));
    }


    private Sprite LoadSkinFromData(Dictionary<string, UserDataRecord> data, string key, System.Func<int, Sprite> getSkinFunc)
    {
        // TryGetValue로 키 존재 확인과 값 추출을 동시에 수행 (가독성 UP)
        if (data.TryGetValue(key, out var record) && int.TryParse(record.Value, out int id))
        {
            return getSkinFunc(id);
        }
        return null;
    }

}
