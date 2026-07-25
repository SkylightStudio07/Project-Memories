using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 제시/응답 구간에 따라 배너 스프라이트를 바꾼다.
    /// 적 예고 네 박이 끝나면 다음 박에 플레이어 행동 배너로 바뀐다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PhaseBanner : MonoBehaviour
    {
        [SerializeField] private Conductor conductor;
        [Tooltip("응답 구간 — 플레이어 행동 (ActionLeft_WatchRight)")]
        [SerializeField] private Sprite playerActingSprite;
        [Tooltip("제시 구간 — 적 행동 (WatchLeft_ActionRight)")]
        [SerializeField] private Sprite enemyActingSprite;
        [Tooltip("준비 구간 배너. 비우면 제시 구간 배너를 유지한다")]
        [SerializeField] private Sprite preparationSprite;

        private Image _img;

        private void Awake() => _img = GetComponent<Image>();

        private void OnEnable()
        {
            if (conductor != null)
            {
                conductor.OnPreparationMeasureStart += OnPreparation;
                conductor.OnPresentMeasureStart += OnPresent;
                conductor.OnResponseMeasureStart += OnResponse;
            }
        }

        private void OnDisable()
        {
            if (conductor != null)
            {
                conductor.OnPreparationMeasureStart -= OnPreparation;
                conductor.OnPresentMeasureStart -= OnPresent;
                conductor.OnResponseMeasureStart -= OnResponse;
            }
        }

        private void Start() => OnPreparation(0); // 첫 구간은 준비

        private void OnPreparation(int phase)
            => Set(preparationSprite != null ? preparationSprite : enemyActingSprite);
        private void OnPresent(int cycle) => Set(enemyActingSprite);
        private void OnResponse(int cycle) => Set(playerActingSprite);

        private void Set(Sprite s)
        {
            if (_img == null) return;
            _img.enabled = s != null;
            if (s != null) _img.sprite = s;
        }
    }
}
