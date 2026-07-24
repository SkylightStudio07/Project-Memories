using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BeatMemories
{
    /// <summary>
    /// 색 프리셋 기반 2D 포인트 라이트. 아무 트랜스폼에 붙여 그 자리를 비춘다.
    /// 프리셋: 초록/청색/흰색/하늘색/적색 (+커스텀). 모든 값 인스펙터 조정.
    /// <see cref="aimTarget"/> 지정 시 콘 각도로 그 방향을 조준(스팟처럼).
    /// 에디터에서도 미리보기(ExecuteAlways).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Light2D))]
    public class NeonLight2D : MonoBehaviour
    {
        public enum Preset { Green, Blue, White, Sky, Red, Custom }

        [Header("색")]
        [SerializeField] private Preset preset = Preset.Sky;
        [SerializeField] private Color customColor = Color.white;

        [Header("세기·범위 (인스펙터 조정)")]
        [SerializeField, Min(0f)] private float intensity = 1.2f;
        [SerializeField, Min(0f)] private float outerRadius = 4f;
        [SerializeField, Min(0f)] private float innerRadius = 0.5f;

        [Header("스팟(옵션)")]
        [Tooltip("지정 시 이 트랜스폼 방향으로 콘을 조준")]
        [SerializeField] private Transform aimTarget;
        [Tooltip("콘 각도(360=원형 포인트, 작을수록 좁은 스팟)")]
        [SerializeField, Range(0f, 360f)] private float coneOuterAngle = 360f;
        [SerializeField, Range(0f, 360f)] private float coneInnerAngle = 360f;

        private Light2D _light;

        private void OnEnable() => Apply();
        private void OnValidate() => Apply();

        private void Update()
        {
            if (aimTarget != null) AimAt(aimTarget);
        }

        /// <summary>현재 설정을 Light2D에 반영.</summary>
        public void Apply()
        {
            if (_light == null) _light = GetComponent<Light2D>();
            if (_light == null) return;
            _light.lightType = Light2D.LightType.Point;
            _light.color = ResolveColor();
            _light.intensity = intensity;
            _light.pointLightOuterRadius = outerRadius;
            _light.pointLightInnerRadius = Mathf.Min(innerRadius, outerRadius);
            _light.pointLightOuterAngle = coneOuterAngle;
            _light.pointLightInnerAngle = Mathf.Min(coneInnerAngle, coneOuterAngle);
        }

        private void AimAt(Transform t)
        {
            Vector3 d = t.position - transform.position;
            if (d.sqrMagnitude < 1e-6f) return;
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);
        }

        private Color ResolveColor()
        {
            switch (preset)
            {
                case Preset.Green: return new Color(0.35f, 1.00f, 0.45f); // 초록
                case Preset.Blue:  return new Color(0.30f, 0.45f, 1.00f); // 청색
                case Preset.White: return Color.white;                    // 흰색
                case Preset.Sky:   return new Color(0.45f, 0.85f, 1.00f); // 하늘색
                case Preset.Red:   return new Color(1.00f, 0.30f, 0.30f); // 적색
                default:           return customColor;
            }
        }
    }
}
