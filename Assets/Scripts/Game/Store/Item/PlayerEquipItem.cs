using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipItem : PersistentSingleton<PlayerEquipItem>
{

    public Sprite customPicture { get; private set; }
    public Sprite customStone { get; private set; }
    public Sprite customBord { get; private set; }


    public void PictureItem(Sprite picture) => this.customPicture = picture;

    public void StoneItem(Sprite stone = null)
    {
        this.customStone = stone;
        int skinID = StoneSkinRegistry.Instance.GetEquipStoneSkin(stone);;
    }

    public void BordItem(Sprite bord) => this.customBord = bord;



}
