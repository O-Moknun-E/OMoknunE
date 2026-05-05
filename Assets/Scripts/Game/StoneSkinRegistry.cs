using UnityEngine;

public class StoneSkinRegistry : Singleton<StoneSkinRegistry>
{
    [Header("돌 스킨 배열")]
    [SerializeField] private Sprite[] _stoneSkins;

    /// <summary>
    /// 스킨 ID로 스프라이트 가져오기
    /// </summary>
    /// <param name="skinID">스킨 ID</param>
    /// <returns>해당하는 스킨 스프라이트 반환</returns>
    public Sprite GetStoneSkin(int skinID)
    {
        // 유효성 검사
        if(_stoneSkins == null || skinID < 0 || skinID >= _stoneSkins.Length)
        {
            Debug.LogWarning($"[SkinRegistry] 유호하지 않은 스킨 ID: {skinID}");
            return null;
        }

        return _stoneSkins[skinID];
    }

    /// <summary>
    /// 돌 스킨의 총 개수 반환
    /// </summary>
    /// <returns>돌 스킨의 총 개수</returns>
    public int GetStoneSkinCount() => _stoneSkins != null ? _stoneSkins.Length : 0;
}
