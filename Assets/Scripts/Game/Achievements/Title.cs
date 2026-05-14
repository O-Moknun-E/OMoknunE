using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    private string activeTitleKey = "ActiveTitle";

    public void EquipTitle(string titleId)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { activeTitleKey, titleId } }
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log($"[{titleId}] ÄªÈ£ ÀåÂø ¼º°ø!");
        }, OnError);
    }

    private void OnError(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());


}
