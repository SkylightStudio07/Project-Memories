using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 키프레임 스텝 애니메이터 (림버스식). 스프라이트 배열을 트위닝 없이
    /// 뚝뚝 끊어 넘긴다. 시간 기반(fps) 또는 비트 동기(박마다 다음 프레임).
    /// 행동 스프라이트 표시 중엔 <see cref="Pause"/>/<see cref="Resume"/>으로 양보한다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class KeyframeAnimator : MonoBehaviour
    {
        [Tooltip("키프레임 스프라이트들(순서대로 순환)")]
        [SerializeField] private Sprite[] frames;

        [Header("스텝 방식 (인스펙터 조정)")]
        [Tooltip("켜면 비트마다 다음 프레임(리듬 동기). 끄면 fps 기반")]
        [SerializeField] private bool syncToBeat = true;
        [SerializeField] private Conductor conductor;
        [Tooltip("비트 동기 시: 몇 박마다 한 프레임 넘길지")]
        [SerializeField, Min(1)] private int beatsPerFrame = 1;
        [Tooltip("시간 기반 시: 초당 프레임 수")]
        [SerializeField, Min(0.1f)] private float fps = 4f;

        private SpriteRenderer _sr;
        private int _frame;
        private int _beatCount;
        private float _timer;
        private bool _paused;

        public bool HasFrames => frames != null && frames.Length > 0;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        private void OnEnable()
        {
            if (conductor != null) conductor.OnClockBeat += OnBeat;
            Show();
        }

        private void OnDisable()
        {
            if (conductor != null) conductor.OnClockBeat -= OnBeat;
        }

        private void Update()
        {
            if (_paused || syncToBeat || !HasFrames) return;
            _timer += Time.deltaTime;
            float step = 1f / fps;
            while (_timer >= step)
            {
                _timer -= step;
                Advance();
            }
        }

        private void OnBeat(int totalBeat)
        {
            if (_paused || !syncToBeat || !HasFrames) return;
            _beatCount++;
            if (_beatCount % Mathf.Max(1, beatsPerFrame) == 0) Advance();
        }

        /// <summary>행동 스프라이트가 슬롯을 쓸 동안 정지.</summary>
        public void Pause() => _paused = true;

        /// <summary>정지 해제 + 현재 키프레임 즉시 표시.</summary>
        public void Resume()
        {
            _paused = false;
            Show();
        }

        private void Advance()
        {
            _frame = (_frame + 1) % frames.Length;
            Show();
        }

        private void Show()
        {
            if (_sr != null && HasFrames) _sr.sprite = frames[_frame % frames.Length];
        }
    }
}
