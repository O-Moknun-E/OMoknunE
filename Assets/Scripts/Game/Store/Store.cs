using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;

public class Store : MonoBehaviour
{
   
    private string catalogVersion = "Main";
    private string moneyCode = "SD";

    [SerializeField]
    private Inventory inven;


    public void BuyItem(string itemId, int price)
    {
        if (inven.HasItem(itemId))
        {
            Debug.Log($"{itemId}는 이미 소유 중임!");
            return;
        }

        var request = new PurchaseItemRequest
        {
            CatalogVersion = catalogVersion,
            ItemId = itemId,
            VirtualCurrency = moneyCode,
            Price = price
        };

        PlayFabClientAPI.PurchaseItem(request, result =>{ inven.LoadItem(); }, error => Debug.LogError(error.GenerateErrorReport()));
    }
}
