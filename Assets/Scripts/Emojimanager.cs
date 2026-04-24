using UnityEngine;
using UnityEngine.UI;

public class Emojimanager : MonoBehaviour
{
    public GameObject emojiPrefab;
    public Transform spawnPoint;
    public Sprite[] emojiSprites;

    public void ShowEmoji(int index)
    {
        Debug.Log("이모지 버튼 눌림! index: " + index);

        GameObject obj = Instantiate(emojiPrefab, spawnPoint);

        Image img = obj.GetComponent<Image>();
        img.sprite = emojiSprites[index];

        Destroy(obj, 2f);
        obj.transform.localPosition = Vector3.zero;
    }
}
