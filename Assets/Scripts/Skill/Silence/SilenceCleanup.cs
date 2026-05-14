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
            GameObject unlockObj = Instantiate(_silencePrefab, transform.position, transform.rotation, transform.parent);

            float destroyDelay = 1.0f;

            Animator animator = unlockObj.GetComponent<Animator>();
            if (animator != null)
            {
                // 애니메이션 상태 안전 호출
                animator.Play("Silence_Open");

                RuntimeAnimatorController ac = animator.runtimeAnimatorController;
                if (ac != null)
                {
                    foreach (AnimationClip clip in ac.animationClips)
                    {
                        // 클립 이름 대소문자 무관 포함 확인
                        if (clip.name.ToLower().Contains("open"))
                        {
                            destroyDelay = clip.length;
                            break;
                        }
                    }
                }
            }

            Destroy(unlockObj, destroyDelay);
        }
    }
}