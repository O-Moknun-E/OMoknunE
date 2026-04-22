using UnityEngine;

public interface IMagic
{
    string Name { get; }        // 마법 이름
    string Description { get; } // 마법 설명
    int Cost { get; }           // 마법 비용
    void Execute();             // 마법 사용 메서드
}
