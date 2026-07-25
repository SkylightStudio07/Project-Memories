using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 비트 타임라인 커서. 자기 담당 박 구간(<see cref="beatOffset"/>부터 dots 개수만큼)에서만
    /// 해당 박의 점(dot) 트랜스폼 위치로 이동하고, 이동할 때마다 펄스를 준다.
    /// 예) 제시 커서: offset 0 + PresentDot0~3 / 응답 커서: offset 4 + ResponseDot0~3.
    /// 담당 밖의 박에서는 움직이지 않는다(마지막 위치 유지).
    /// </summary>
    public class BeatCursor : MonoBehaviour
    {
        [SerializeField] private Conductor conductor;
        [SerializeField] private RoundManager round;
        [SerializeField] private RectTransform cursor;
        [Tooltip("이 커서가 담당하는 첫 박(사이클 내). 제시=0, 응답=4")]
        [SerializeField, Min(0)] private int beatOffset = 0;
        [Tooltip("담당 구간의 점들(박 순서)")]
        [SerializeField] private RectTransform[] dots = new RectTransform[4];

        [Header("펄스 (인스펙터 조정)")]
        [SerializeField, Min(1f)] private float pulseScale = 1.4f;
        [SerializeField, Min(0.03f)] private float pulseDuration = 0.12f;

        [Header("이동")]
        [Tooltip("켜면 점 사이를 부드럽게 이동, 끄면 즉시 스냅")]
        [SerializeField] private bool smooth = false;
        [SerializeField, Min(1f)] private float smoothSpeed = 22f;
        [Tooltip("담당 박이 아닐 때 Cursor 이미지를 숨김")]
        [SerializeField] private bool showOnlyDuringRange = true;
        [Tooltip("응답 입력 창 동안 시계 방향으로 판정 시간을 채움")]
        [SerializeField] private bool showInputWindowProgress;

        private Vector3 _baseScale = Vector3.one;
        private float _t;
        private RectTransform _target;
        private Image _cursorImage;

        private void Awake()
        {
            if (cursor == null) return;
            _baseScale = cursor.localScale;
            _cursorImage = cursor.GetComponent<Image>();
            if (showOnlyDuringRange && _cursorImage != null) _cursorImage.enabled = false;
        }

        private void OnEnable() { if (conductor != null) conductor.OnBeat += OnBeat; }
        private void OnDisable() { if (conductor != null) conductor.OnBeat -= OnBeat; }

        private void OnBeat(int beatInCycle)
        {
            int idx = beatInCycle - beatOffset;
            if (cursor == null || dots == null || idx < 0 || idx >= dots.Length)
            {
                if (showOnlyDuringRange && _cursorImage != null) _cursorImage.enabled = false;
                return;
            }
            _target = dots[idx];
            if (_target == null) return;
            if (!smooth) cursor.position = _target.position; // 즉시 스냅
            if (!showInputWindowProgress && _cursorImage != null) _cursorImage.enabled = true;
            _t = 1f; // 펄스 시작(가장 큼 → 원래대로)
        }

        private void Update()
        {
            if (cursor == null) return;

            // 카운트인/미시작(전환 직후 포함) 중엔 이전 스테이지 잔상이 남지 않게 커서를 숨긴다.
            if (conductor != null && conductor.TotalBeats < 0)
            {
                if (_cursorImage != null) _cursorImage.enabled = false;
                _target = null;
                return;
            }

            if (showInputWindowProgress && _cursorImage != null)
            {
                int slot = conductor != null ? conductor.BeatInCycle - beatOffset : -1;
                float progress = 0f;
                bool visible = round != null
                    && slot >= 0
                    && dots != null
                    && slot < dots.Length
                    && round.TryGetInputWindowProgress(slot, out progress);
                _cursorImage.enabled = visible;
                if (visible) _cursorImage.fillAmount = progress;
            }

            if (smooth && _target != null)
                cursor.position = Vector3.Lerp(cursor.position, _target.position, Time.deltaTime * smoothSpeed);
            if (_t > 0f)
            {
                _t = Mathf.MoveTowards(_t, 0f, Time.deltaTime / pulseDuration);
                cursor.localScale = Vector3.Lerp(_baseScale, _baseScale * pulseScale, _t);
            }
        }
    }
}
