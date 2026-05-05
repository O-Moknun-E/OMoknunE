using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MagicData
{
    public string magicName;
    public Sprite icon;
    public int price;
    public string description;
    public Rarity rarity;
    public bool owned;

    public enum Rarity { Common, Rare, Epic, Legendary } //추후 삭제 가능?
}

public static class MagicDatabase
{
    public static List<MagicData> allMagics = new List<MagicData>()
    {
        new MagicData { magicName = "안개 Mist",    price = 100, description = "안개 공격",  rarity = MagicData.Rarity.Rare },
        new MagicData { magicName = "암전 Blackout", price = 100, description = "암전 공격",       rarity = MagicData.Rarity.Common },
        new MagicData { magicName = "색상교란 Blind",    price = 100, description = "색상교란 공격",  rarity = MagicData.Rarity.Epic },
        new MagicData { magicName = "파이어볼 Fireball",   price = 100, description = "파이어볼 공격",       rarity = MagicData.Rarity.Rare },
        new MagicData { magicName = "위치변경 Shift",    price = 100, description = "위치변경 공격",       rarity = MagicData.Rarity.Rare },
        new MagicData { magicName = "환영 Illusion",      price = 100, description = "환영 공격",   rarity = MagicData.Rarity.Legendary },
        new MagicData { magicName = "시간과부하 Overload",      price = 100, description = "시간과부하 공격",   rarity = MagicData.Rarity.Legendary },
        new MagicData { magicName = "마나덫 Manatrap",      price = 100, description = "마나덫 공격",   rarity = MagicData.Rarity.Legendary },
        new MagicData { magicName = "침묵 Silence",      price = 100, description = "침묵 공격",   rarity = MagicData.Rarity.Legendary },
        new MagicData { magicName = "결계 Crater",      price = 100, description = "결계 공격",   rarity = MagicData.Rarity.Legendary },
        new MagicData { magicName = "정화 Purify",      price = 100, description = "정화 공격",   rarity = MagicData.Rarity.Legendary },
    };
}