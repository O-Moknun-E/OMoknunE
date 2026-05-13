using UnityEngine;

public class SilenceCleanup : MonoBehaviour
{
    private GameObject _silencePrefab;
    private bool _isQuitting = false;

    public void Setup(GameObject prefab)
    {
        _silencePrefab = prefab;
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void OnDestroy()
    {
        if (_isQuitting || !gameObject.scene.isLoaded) return;

        if (_silencePrefab != null)
        {
            // 투명한 추적기가 파괴되는 시점에 자물쇠 프리팹을 다시 생성합니다
            GameObject unlockObj = Instantiate(_silencePrefab, transform.position, transform.rotation, transform.parent);

            float destroyDelay = 1.0f;

            Animator animator = unlockObj.GetComponent<Animator>();
            if (animator != null)
            {
                // 생성되자마자 자동으로 재생되는 lock을 무시하고, Silence_Open을 즉시 덮어씌워 재생시킵니다
                animator.Play("Silence_Open", -1, 0f);
                animator.Update(0f);

                RuntimeAnimatorController ac = animator.runtimeAnimatorController;
                if (ac != null)
                {
                    foreach (AnimationClip clip in ac.animationClips)
                    {
                        if (clip.name == "Silence_Open")
                        {
                            destroyDelay = clip.length;
                            break;
                        }
                    }
                }
            }

            // Silence_Open 애니메이션이 끝난 후 완전히 파괴하여 연출을 종료합니다
            Destroy(unlockObj, destroyDelay);
        }
    }
}