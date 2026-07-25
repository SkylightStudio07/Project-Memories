using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 카메라를 아주 미세하게 사인 흔들림 → 파랄랙스 레이어에 깊이감을 준다.
    /// 고정 카메라 게임에서 배경/전경이 다른 속도로 움직이는 효과. 값은 인스펙터 조정(끄려면 0).
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraSway : MonoBehaviour
    {
        [Tooltip("흔들림 진폭(월드 단위)")]
        [SerializeField] private Vector2 amplitude = new Vector2(0.22f, 0.08f);
        [Tooltip("흔들림 주기 속도(Hz 유사)")]
        [SerializeField] private Vector2 speed = new Vector2(0.10f, 0.16f);

        [Header("타격 Shake")]
        [SerializeField, Min(0f)] private float hitDuration = 0.18f;
        [SerializeField, Min(0f)] private float hitAmplitude = 0.16f;
        [SerializeField, Min(1f)] private float strongAttackMultiplier = 1.8f;

        private Vector3 start;
        private float hitTimer;
        private float hitStrength;
        private bool initialized;

        private void Start()
        {
            start = transform.position;
            initialized = true;
        }

        /// <summary>기존 배경 Sway 위에 감쇠하는 타격 Shake를 더한다.</summary>
        public void Shake(bool strongAttack = false)
        {
            hitTimer = hitDuration;
            hitStrength = strongAttack ? strongAttackMultiplier : 1f;
        }

        private void LateUpdate()
        {
            float x = Mathf.Sin(Time.time * speed.x * Mathf.PI * 2f) * amplitude.x;
            float y = Mathf.Sin(Time.time * speed.y * Mathf.PI * 2f) * amplitude.y;
            Vector3 impact = Vector3.zero;
            if (hitTimer > 0f && hitDuration > 0f)
            {
                hitTimer = Mathf.Max(0f, hitTimer - Time.deltaTime);
                float strength = hitAmplitude * hitStrength * (hitTimer / hitDuration);
                Vector2 randomOffset = Random.insideUnitCircle * strength;
                impact = new Vector3(randomOffset.x, randomOffset.y, 0f);
            }
            transform.position = start + new Vector3(x, y, 0f) + impact;
        }

        private void OnDisable()
        {
            if (initialized) transform.position = start;
        }
    }
}
