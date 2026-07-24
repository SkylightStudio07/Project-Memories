using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 제시/응답 마디에 따라 배너 스프라이트를 바꾼다.
    ///  - 제시 마디(적이 행동, 플레이어는 관찰) → <see cref="enemyActingSprite"/>
    ///  - 응답 마디(플레이어가 행동, 적은 관찰) → <see cref="playerActingSprite"/>
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PhaseBanner : MonoBehaviour
    {
        [SerializeField] private Conductor conductor;
        [Tooltip("응답 마디 — 플레이어 행동 (ActionLeft_WatchRight)")]
        [SerializeField] private Sprite playerActingSprite;
        [Tooltip("제시 마디 — 적 행동 (WatchLeft_ActionRight)")]
        [SerializeField] private Sprite enemyActingSprite;

        private Image _img;

        private void Awake() => _img = GetComponent<Image>();

        private void OnEnable()
        {
            if (conductor != null)
            {
                conductor.OnPresentMeasureStart += OnPresent;
                conductor.OnResponseMeasureStart += OnResponse;
            }
        }

        private void OnDisable()
        {
            if (conductor != null)
            {
                conductor.OnPresentMeasureStart -= OnPresent;
                conductor.OnResponseMeasureStart -= OnResponse;
            }
        }

        private void Start() => Set(enemyActingSprite); // 첫 마디는 제시(적 행동)

        private void OnPresent(int cycle) => Set(enemyActingSprite);
        private void OnResponse(int cycle) => Set(playerActingSprite);

        private void Set(Sprite s)
        {
            if (_img != null && s != null) _img.sprite = s;
        }
    }
}
