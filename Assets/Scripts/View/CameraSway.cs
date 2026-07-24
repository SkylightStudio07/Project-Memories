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

        private Vector3 start;

        private void Start() => start = transform.position;

        private void LateUpdate()
        {
            float x = Mathf.Sin(Time.time * speed.x * Mathf.PI * 2f) * amplitude.x;
            float y = Mathf.Sin(Time.time * speed.y * Mathf.PI * 2f) * amplitude.y;
            transform.position = start + new Vector3(x, y, 0f);
        }
    }
}
