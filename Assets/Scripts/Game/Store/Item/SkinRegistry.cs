using UnityEditor.Animations;
using UnityEngine;

public class SkinRegistry : PersistentSingleton<SkinRegistry>
{
    [Header("스킨 데이터 모음")]
    [SerializeField] private Sprite[] stoneSkins;
    [SerializeField] private Sprite[] pictureSkins;
    [SerializeField] private Sprite[] bordSkins;
    [SerializeField] private RuntimeAnimatorController[] animatorControllers;


    public Sprite GetPictureSkin(int id) => GetSkinFromArray(pictureSkins, id);
    public int GetPictureID(Sprite sprite) => GetIDFromArray(pictureSkins, sprite);

    public Sprite GetBordSkin(int id) => GetSkinFromArray(bordSkins, id);
    public int GetBordID(Sprite sprite) => GetIDFromArray(bordSkins, sprite);

    public Sprite GetStoneSkin(int id) => GetSkinFromArray(stoneSkins, id);
    public int GetStoneID(Sprite sprite) => GetIDFromArray(stoneSkins, sprite);

    private Sprite GetSkinFromArray(Sprite[] array, int id)
    {
        if (array == null || id < 0 || id >= array.Length)
        {
            Debug.LogWarning($"[SkinRegistry] ID {id}가 범위를 벗어남.");
            return null;
        }
        return array[id];
    }

    private int GetIDFromArray(Sprite[] array, Sprite sprite)
    {
        if (array == null || sprite == null) return -1;

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == sprite) return i;
        }
        return -1; // 찾지 못했을 때는 -1 반환이 정석임
    }

    public RuntimeAnimatorController GetAnimatorController(int id)
    {
        if (animatorControllers == null || id < 0 || id >= animatorControllers.Length)
        {
            Debug.LogWarning($"[SkinRegistry] AnimatorController ID {id}가 범위를 벗어남.");
            return null;
        }
        return animatorControllers[id];
    }
}
