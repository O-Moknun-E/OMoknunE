using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using System.Collections.Generic;
using System;

public class Store : MonoBehaviour
{
    public bool IsPurchased { get; private set; }

    public Action<List<CatalogItem>> OnStoreLoaded;
    public Action OnBuyItem;

    [SerializeField]
    private Inventory inven;
    private string catalogVersion = "Main";
    private string moneyCode = "SD";

    private List<CatalogItem> storeCatalog = new List<CatalogItem>();

    public void BuyItem(string itemId, uint price)
    {
        if (inven.HasItem(itemId))
        {
            IsPurchased = false;
            OnBuyItem?.Invoke();
            return;
        }


        var request = new PurchaseItemRequest
        {
            CatalogVersion = catalogVersion,
            ItemId = itemId,
            VirtualCurrency = moneyCode,
            Price = (int)price
        };
        IsPurchased = true;

        PlayFabClientAPI.PurchaseItem(request, result =>
        {
            inven.LoadItem();
            Debug.Log($"{itemId}구매완료");

            OnBuyItem?.Invoke();
        },
    error =>
    {
        Debug.LogError(error.GenerateErrorReport());
    });
    }

    public List<CatalogItem> LoadStoreCatalog()
    {
        var request = new GetCatalogItemsRequest
        {
            CatalogVersion = catalogVersion
        };

        PlayFabClientAPI.GetCatalogItems(request,
            result => {
                storeCatalog = result.Catalog;
                Debug.Log("상점 카탈로그 로드 완료!");
                OnStoreLoaded?.Invoke(storeCatalog);
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );

        return storeCatalog;
    }
}
