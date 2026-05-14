using UnityEngine;

public class SkinRegistry : PersistentSingleton<SkinRegistry>
{
    [Header("스킨 데이터 모음")]
    [SerializeField] private Sprite[] stoneSkins;
    [SerializeField] private Sprite[] pictureSkins;
    [SerializeField] private Sprite[] bordSkins;

    public Sprite GetStoneSkin(int id) => GetSkin(stoneSkins, id);
    public int GetStoneID(Sprite sprite) => GetID(stoneSkins, sprite);

    public Sprite GetPictureSkin(int id) => GetSkin(pictureSkins, id);
    public int GetPictureID(Sprite sprite) => GetID(pictureSkins, sprite);

    public Sprite GetBordSkin(int id) => GetSkin(bordSkins, id);
    public int GetBordID(Sprite sprite) => GetID(bordSkins, sprite);

    private Sprite GetSkin(Sprite[] array, int id)
    {
        if (array == null || id < 0 || id >= array.Length) return null;
        return array[id];
    }

    private int GetID(Sprite[] array, Sprite sprite)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == sprite) return i;
        }
        return 0; 
    }
}
