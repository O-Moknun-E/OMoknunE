using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipItem : PersistentSingleton<PlayerEquipItem>
{
    [SerializeField] Sprite basicPicture;
    [SerializeField] Sprite basicStone;
    [SerializeField] Sprite basicBord;


    public Sprite customPicture { get; private set; }
    public Sprite customStone { get; private set; }
    public Sprite customBord { get; private set; }


    public void PictureItem(Sprite picture) => this.customPicture = picture;

    public void StoneItem(Sprite stone = null)
    {
        this.customStone = stone;
        int skinID = StoneSkinRegistry.Instance.GetEquipStoneSkin(stone);
        SaveEquippedSkinToServer(skinID);
    }

    public void BordItem(Sprite bord) => this.customBord = bord;

    private void SaveEquippedSkinToServer(int skinID)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
            { "EquippedStoneID", skinID.ToString() }
        }
        };
        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("서버에 스킨 저장 완료!"),
            error => Debug.LogError("서버 저장 실패: " + error.GenerateErrorReport()));
    }

}
