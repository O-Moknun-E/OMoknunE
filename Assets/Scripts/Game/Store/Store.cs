using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using System.Collections.Generic;

public class Store : MonoBehaviour
{
   

    [SerializeField]
    private Inventory inven;

    private string catalogVersion = "Main";
    private string moneyCode = "SD";

    private List<CatalogItem> storeCatalog = new List<CatalogItem>();

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

    public void LoadStoreCatalog()
    {
        var request = new GetCatalogItemsRequest
        {
            CatalogVersion = catalogVersion
        };

        PlayFabClientAPI.GetCatalogItems(request,
            result => {
                storeCatalog = result.Catalog;
                Debug.Log("상점 카탈로그 로드 완료!");
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
}
