using UnityEngine;

public class PlayerEquipItem : PersistentSingleton<PlayerEquipItem>
{
    [SerializeField] Sprite basicPicture;
    [SerializeField] Sprite basicStone;
    [SerializeField] Sprite basicBord;


    public Sprite customPicture { get; private set; }
    public Sprite customStone { get; private set; }
    public Sprite customBord { get; private set; }

    private void Start()
    {
        customPicture = basicPicture;
        customStone = basicStone;
        customBord = basicBord;
    }


    public void PictureItem(Sprite picture) => this.customPicture = picture; 

    public void StoneItem( Sprite stone = null) => this.customStone = stone;

    public void BordItem( Sprite bord) => this.customBord = bord;

}
