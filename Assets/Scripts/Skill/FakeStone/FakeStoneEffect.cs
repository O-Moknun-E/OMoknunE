using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FakeStoneEffect", menuName = "OmokEffects/FakeStone")]
public class FakeStoneEffect : SkillEffect
{
    [Header("가짜 돌 기본 프리팹")]
    public GameObject fakeStonePrefab;

    [Header("블라인드 효과 프리팹")]
    public GameObject blindPrefab;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (fakeStonePrefab == null || blindPrefab == null) return;

        if (!context.IsReplay)
        {
            if (OmokManager.Instance.GetBoardData(context.TargetX, context.TargetY) != StoneType.Empty)
            {
                Debug.LogWarning("이미 돌이 있는 자리입니다");
                return;
            }
        }

        Transform parent = context.IsReplay ? ReplayManager.ReplayEffectsContainer : null;

        GameObject fakeStone = Instantiate(fakeStonePrefab, spawnPos, Quaternion.identity, parent);

        bool isMine = context.IsMine();

        SpriteRenderer sr = fakeStone.GetComponent<SpriteRenderer>();
        Animator anim = fakeStone.GetComponent<Animator>();
        if (sr != null)
        {
            if (isMine)
            {
                if (anim != null) anim.enabled = true;
                sr.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                if (anim != null) anim.enabled = false;
                sr.sprite = StoneSkinRegistry.Instance.GetStoneSkin(context.SkinID);
                sr.color = new Color(1f, 1f, 1f, 1f);

                GameObject blind = Instantiate(blindPrefab, new Vector3(0f, 0f, -0.2f), Quaternion.identity);
                Destroy(blind, 1.5f);
            }
        }

        TurnDuration timer = fakeStone.AddComponent<TurnDuration>();
        timer.Setup(context.GetCasterStoneType(), 3, context.IsReplay);

        if (!context.IsReplay)
        {
            FakeStoneCleanup cleanup = fakeStone.AddComponent<FakeStoneCleanup>();

            // 소멸 시 연출을 위해 가짜 돌 원본 프리팹을 전달합니다
            cleanup.LockSpot(context.TargetX, context.TargetY, fakeStonePrefab);
        }
    }
}