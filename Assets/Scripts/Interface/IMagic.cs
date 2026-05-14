using UnityEngine;

public interface IMagic
{
    int ID { get; }                     // 마법 번호
    string Name { get; }                // 시스템용 이름
    string DisplayName { get; }         // 상점용 이름 (추가)
    Sprite Icon { get; }                // 상점용 아이콘 (추가)
    string Description { get; }         // 상점용 설명 (추가)
    int Cost { get; }                   // 마법 비용

    void Execute(bool isReplay = false);
    void SetTarget(int x, int y, PlayerType caster, int skinID);

    // 기존 ReplayManager 등과의 호환성을 위한 함수들
    void SetContext(int x, int y, PlayerType caster, int skinID);
}