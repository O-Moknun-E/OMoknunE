using PlayFab.ClientModels;
using UnityEngine;

public class StoneItem : Item, IUsable
{
    public StoneItem(ItemInstance instance, ItemData meta) : base(instance, meta) { }

    public void Use()
    {
        throw new System.NotImplementedException(); //게임시  플레이어 스톤이 바뀌어야함
    }
}
