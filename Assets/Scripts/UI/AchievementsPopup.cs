using System.Collections;
using TMPro;
using UnityEngine;

public class AchievementsPopup : MonoBehaviour
{
    [SerializeField] private Transform originPos;
    [SerializeField] private Transform targetPos;
    [SerializeField] private TextMeshProUGUI title;

    private Coroutine moveCoroutine;
    private WaitForSeconds waitUpdateDelay = new WaitForSeconds(3f);

    private float moveSpeed = 2.5f;

    public void Move(string text)
    {
        if (moveCoroutine != null) return;

        this.title.text = text;
        moveCoroutine = StartCoroutine(MoveCo());
    }

    private IEnumerator MoveCo()
    {
        while (Vector2.Distance(transform.position, targetPos.position) > 0.001f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPos.position,
                moveSpeed 
            );
            yield return null;
        }

        yield return waitUpdateDelay;

        while (Vector2.Distance(transform.position, originPos.position) > 0.001f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                originPos.position,
                moveSpeed
            );
            yield return null;
        }

        moveCoroutine = null;
        yield return null;
    }
}
