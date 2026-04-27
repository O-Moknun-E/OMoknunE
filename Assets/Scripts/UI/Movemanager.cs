using UnityEngine;
using TMPro;
public class Movemanager : MonoBehaviour
{
    public Transform content;
    public GameObject moveTextPrefab;

    int moveCount = 1;

    public void AddMove(int x, int y, string player)
    {
        GameObject obj = Instantiate(moveTextPrefab, content);

        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.text = moveCount + ". " + player + " (" + x + "," + y + ")";

        moveCount++;
    }

    public void TestAddMove()
    {
        AddMove(3, 3, "Èæ");
    }
}
