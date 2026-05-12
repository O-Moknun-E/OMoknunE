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

    public void StoneItem( Sprite stone = null) => this.customStone = stone;

    public void BordItem( Sprite bord) => this.customBord = bord;

/*    public Sprite CheckCustomStone(StoneType stoneType)
    {
        if(stoneType == StoneType.Black)

        return customStone;
    }*/

}
