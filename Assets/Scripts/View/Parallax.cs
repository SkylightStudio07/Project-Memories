using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 배경/바닥 레이어 파랄랙스. 기준 카메라의 이동에 대해 <see cref="factor"/> 비율로 따라가고,
    /// 선택적으로 <see cref="autoScroll"/>로 상시 스크롤한다(고정 카메라에서도 미세한 생동감).
    /// </summary>
    [DisallowMultipleComponent]
    public class Parallax : MonoBehaviour
    {
        [Tooltip("기준 카메라(비우면 Camera.main)")]
        [SerializeField] private Transform cam;
        [Tooltip("0=완전 고정, 1=카메라와 동일 이동. 멀수록 작게")]
        [Range(0f, 1f)][SerializeField] private float factor = 0.4f;
        [Tooltip("상시 자동 스크롤 속도(units/s). 고정 카메라 배경에 생동감")]
        [SerializeField] private Vector2 autoScroll = Vector2.zero;

        private Vector3 startPos;
        private Vector3 camStart;

        private void Start()
        {
            if (cam == null && Camera.main != null) cam = Camera.main.transform;
            startPos = transform.position;
            camStart = cam != null ? cam.position : Vector3.zero;
        }

        private void LateUpdate()
        {
            Vector3 pos = startPos;
            if (cam != null)
            {
                Vector3 d = cam.position - camStart;
                pos.x += d.x * factor;
                pos.y += d.y * factor;
            }
            pos.x += autoScroll.x * Time.time;
            pos.y += autoScroll.y * Time.time;
            transform.position = pos; // z는 startPos.z 유지(정렬용)
        }
    }
}
