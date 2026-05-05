using UnityEngine;
using TMPro;

public class MoveHistoryUI : MonoBehaviour
{
    public Transform content;        // Content
    public GameObject moveItemPrefab; // ÇÁ¸®ÆÕ

    int moveCount = 1;

    public void AddMove(string player, int x, int y)
    {
        GameObject item = Instantiate(moveItemPrefab, content);

        TMP_Text text = item.GetComponent<TMP_Text>();
        text.text = moveCount + ". " + player + " (" + x + "," + y + ")";

        moveCount++;

      
    }

}