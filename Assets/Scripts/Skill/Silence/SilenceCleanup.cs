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
            // 파괴되는 시점에 동일한 자물쇠 프리팹을 그 자리에 다시 생성합니다
            GameObject unlockObj = Instantiate(_silencePrefab, transform.position, transform.rotation, transform.parent);

            float destroyDelay = 1.0f;

            Animator animator = unlockObj.GetComponent<Animator>();
            if (animator != null)
            {
                // 애니메이터 상태(State) 이름을 정확히 Silence_Open으로 호출합니다
                animator.Play("Silence_Open");

                RuntimeAnimatorController ac = animator.runtimeAnimatorController;
                if (ac != null)
                {
                    foreach (AnimationClip clip in ac.animationClips)
                    {
                        // 클립 이름도 Silence_Open과 일치하는지 확인하여 길이를 가져옵니다
                        if (clip.name == "Silence_Open")
                        {
                            destroyDelay = clip.length;
                            break;
                        }
                    }
                }
            }

            // Silence_Open 애니메이션 길이만큼 재생한 뒤 오브젝트를 완전히 파괴합니다
            Destroy(unlockObj, destroyDelay);
        }
    }
}