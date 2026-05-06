using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 마법을 ID로 관리하는 레지스트리 클래스
/// </summary>
public class MagicRegistry : PersistentSingleton<MagicRegistry>
{
    [Header("마법 목록")]
    [SerializeField] private List<SkillBase> _magicList = new();

    private Dictionary<int, IMagic> _magicDictionary;   // ID로 마법을 빠르게 조회하기 위한 딕셔너리

    protected override void Awake()
    {
        base.Awake();

        InitDictionary();
    }

    /// <summary>
    /// 마법 딕셔너리 초기화
    /// </summary>
    private void InitDictionary()
    {
        _magicDictionary = new();

        // 인스펙터에서 등록된 마법들을 딕셔너리에 추가
        foreach(var magic in _magicList)
        {
            if(magic != null)
            {
                if(_magicDictionary.ContainsKey(magic.ID)) {
                    Debug.LogWarning($"중복된 마법 ID 발견: {magic.ID} ({magic.Name})");
                    continue;
                }

                _magicDictionary.Add(magic.ID, magic);
            }
        }

        // Resources 폴더에서 자동으로 모든 SkillBase 로드
        LoadMagicsFromResources();
    }

    /// <summary>
    /// Resources 폴더에서 모든 마법 자동으로 로드
    /// </summary>
    private void LoadMagicsFromResources()
    {
        SkillBase[] magics = Resources.LoadAll<SkillBase>("Skills");

        foreach(var magic in magics)
        {
            // 등록되지 않은 마법만 딕셔너리에 추가
            if (!_magicDictionary.ContainsKey(magic.ID))
            {
                _magicDictionary.Add(magic.ID, magic);
            }
        }

        Debug.Log($"총 {_magicDictionary.Count}개의 마법이 등록되었습니다.");
    }

    /// <summary>
    /// ID로 마법 가져오기
    /// </summary>
    /// <param name="magicID">마법 ID</param>
    /// <returns>해당 ID의 마법 객체</returns>
    public IMagic GetMagicByID(int magicID)
    {
        if(_magicDictionary.TryGetValue(magicID, out var magic))
        {
            return magic;
        }

        Debug.LogWarning($"마법 ID {magicID}에 해당하는 마법이 없습니다.");
        return null;
    }
}
