using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BeatMemories
{
    /// <summary>
    /// Charge Ready 상태의 글로우와 에너지 입자를 관리한다.
    /// 파티클 디자인은 씬의 ParticleSystem 설정을 그대로 사용한다.
    /// </summary>
    public sealed class ChargeAuraEffect : MonoBehaviour
    {
        [Header("Effect References")]
        [SerializeField] private Light2D rimGlow;
        [SerializeField] private ParticleSystem energyWisps;
        [SerializeField] private LineRenderer readyRing;

        [Header("Rim Glow")]
        [SerializeField, Min(0f)] private float flashIntensity = 1.6f;
        [SerializeField, Min(0f)] private float sustainIntensity = 0.3f;
        [SerializeField, Min(0.1f)] private float pulsePeriod = 0.5f;

        [Header("Ready Ring")]
        [SerializeField, Min(0f)] private float ringDuration = 0.28f;
        [SerializeField, Min(0.1f)] private float ringRadius = 1.5f;

        private SpriteRenderer followTarget;
        private Color auraColor = Color.cyan;
        private bool isReady;

        public bool IsReady => isReady;

        private void Awake()
        {
            CacheReferences();
            StopImmediate();
        }

        private void LateUpdate()
        {
            FollowPlayer();
        }

        private void OnDisable()
        {
            StopImmediate();
        }

        public void Initialize(SpriteRenderer target, Color color)
        {
            followTarget = target;
            auraColor = color;
            CacheReferences();
            ApplyColor();
            InitializeRing();
            FollowPlayer();
            StopImmediate();
        }

        /// <summary>PlayerData.OnChargedChanged 이벤트에서 직접 사용할 Charge Ready 전환점.</summary>
        public void SetReady(bool ready)
        {
            if (isReady == ready) return;
            isReady = ready;

            if (ready)
            {
                FollowPlayer();
                PlayParticles();
                PlayGlow();
                PlayReadyRing();
            }
            else
            {
                StopParticles();
                StopGlow();
                StopReadyRing();
            }
        }

        public void StopImmediate()
        {
            isReady = false;

            if (rimGlow != null)
            {
                rimGlow.DOKill();
                rimGlow.intensity = 0f;
                rimGlow.enabled = false;
            }

            if (energyWisps != null)
                energyWisps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (readyRing != null)
            {
                readyRing.DOKill();
                readyRing.transform.DOKill();
                readyRing.enabled = false;
                readyRing.transform.localScale = Vector3.one;
            }
        }

        private void CacheReferences()
        {
            if (rimGlow == null) rimGlow = GetComponent<Light2D>();
            if (energyWisps == null)
            {
                energyWisps = GetComponent<ParticleSystem>();
                if (energyWisps == null)
                {
                    energyWisps = gameObject.AddComponent<ParticleSystem>();
                    ConfigureRuntimeParticles();
                }
            }
            if (readyRing == null) readyRing = GetComponent<LineRenderer>();
        }

        private void ConfigureRuntimeParticles()
        {
            ParticleSystem.MainModule main = energyWisps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = 0.7f;
            main.startSpeed = 0.45f;
            main.startSize = 0.08f;
            main.maxParticles = 32;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = energyWisps.emission;
            emission.rateOverTime = 18f;

            ParticleSystem.ShapeModule shape = energyWisps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.65f;
            shape.radiusThickness = 0.15f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                energyWisps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.85f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;
        }

        private void ApplyColor()
        {
            if (rimGlow != null) rimGlow.color = auraColor;

            if (energyWisps != null)
            {
                ParticleSystem.MainModule main = energyWisps.main;
                main.startColor = auraColor;
            }

            SetRingColor(0f);
        }

        private void FollowPlayer()
        {
            if (followTarget != null) transform.position = followTarget.bounds.center;
        }

        private void PlayParticles()
        {
            if (energyWisps == null) return;
            ParticleSystem.MainModule main = energyWisps.main;
            main.startColor = auraColor;
            energyWisps.Play(true);
        }

        private void StopParticles()
        {
            if (energyWisps != null)
                energyWisps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void PlayGlow()
        {
            if (rimGlow == null) return;

            rimGlow.DOKill();
            rimGlow.color = auraColor;
            rimGlow.intensity = 0f;
            rimGlow.enabled = true;

            Sequence intro = DOTween.Sequence().SetTarget(rimGlow);
            intro.Append(DOTween.To(
                () => rimGlow.intensity,
                value => rimGlow.intensity = value,
                flashIntensity,
                0.1f).SetEase(Ease.OutQuad));
            intro.Append(DOTween.To(
                () => rimGlow.intensity,
                value => rimGlow.intensity = value,
                sustainIntensity,
                0.12f).SetEase(Ease.InQuad));
            intro.OnComplete(StartGlowPulse);
        }

        private void StartGlowPulse()
        {
            if (!isReady || rimGlow == null) return;

            rimGlow.DOKill();
            float low = sustainIntensity * 0.7f;
            float high = sustainIntensity * 1.25f;
            rimGlow.intensity = low;
            DOTween.To(
                    () => rimGlow.intensity,
                    value => rimGlow.intensity = value,
                    high,
                    pulsePeriod * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(rimGlow);
        }

        private void StopGlow()
        {
            if (rimGlow == null) return;

            rimGlow.DOKill();
            rimGlow.enabled = true;
            Sequence release = DOTween.Sequence().SetTarget(rimGlow);
            release.Append(DOTween.To(
                () => rimGlow.intensity,
                value => rimGlow.intensity = value,
                flashIntensity * 1.35f,
                0.05f).SetEase(Ease.OutQuad));
            release.Append(DOTween.To(
                () => rimGlow.intensity,
                value => rimGlow.intensity = value,
                0f,
                0.12f).SetEase(Ease.InQuad));
            release.OnComplete(() => rimGlow.enabled = false);
        }

        private void InitializeRing()
        {
            if (readyRing == null) return;

            readyRing.useWorldSpace = false;
            readyRing.loop = true;
            readyRing.positionCount = 32;
            readyRing.widthMultiplier = 0.05f;
            for (int i = 0; i < readyRing.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / readyRing.positionCount;
                readyRing.SetPosition(
                    i,
                    new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ringRadius);
            }

            SetRingColor(0f);
            readyRing.enabled = false;
        }

        private void PlayReadyRing()
        {
            if (readyRing == null) return;

            float alpha = 1f;
            readyRing.DOKill();
            readyRing.transform.DOKill();
            readyRing.enabled = true;
            readyRing.transform.localScale = Vector3.one * 0.3f;
            SetRingColor(alpha);

            Sequence ring = DOTween.Sequence().SetTarget(readyRing);
            ring.Append(readyRing.transform.DOScale(1.25f, ringDuration).SetEase(Ease.OutCubic));
            ring.Join(DOTween.To(
                () => alpha,
                value =>
                {
                    alpha = value;
                    SetRingColor(value);
                },
                0f,
                ringDuration).SetEase(Ease.InQuad));
            ring.OnComplete(() => readyRing.enabled = false);
        }

        private void StopReadyRing()
        {
            if (readyRing == null) return;
            readyRing.DOKill();
            readyRing.transform.DOKill();
            readyRing.enabled = false;
        }

        private void SetRingColor(float alpha)
        {
            if (readyRing == null) return;
            Color color = auraColor;
            color.a = alpha;
            readyRing.startColor = color;
            readyRing.endColor = color;
        }
    }
}
