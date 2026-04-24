using UnityEngine;

public interface IMagic
{
    int id { get; }             // 마법 번호(-1: 사용안함(없음))
    string Name { get; }        // 마법 이름
    string Description { get; } // 마법 설명
    int Cost { get; }           // 마법 비용
    void Execute();             // 마법 사용 메서드
}
