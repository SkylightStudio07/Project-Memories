using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BeatMemories
{
    /// <summary>
    /// 준비/활성 페이즈의 배경·조명·카메라·스네어를 한곳에서 연출한다.
    /// 오디오 에셋이 아직 없으면 짧은 절차적 스네어를 프로토타입 폴백으로 사용한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class PhasePresentationController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RoundManager round;
        [SerializeField] private Conductor conductor;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private AudioClip snareClip;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField, Range(0f, 1f)] private float backgroundMusicVolume = 0.45f;

        [Header("전환")]
        [SerializeField, Min(0.1f)] private float transitionSpeed = 3f;
        [Tooltip("공격 페이즈 준비 중 직교 카메라 크기 배율. 0.85면 약 18% 확대")]
        [SerializeField, Range(0.75f, 1f)] private float attackPreparationZoomRatio = 0.85f;

        private AudioSource _audioSource;
        private AudioSource _backgroundMusicSource;
        private AudioClip _runtimeSnare;
        private PhaseSO _phase;
        private bool _preparing;
        private Color _baseBackgroundColor = Color.white;
        private Color _baseLightColor = Color.white;
        private Color _targetBackgroundColor = Color.white;
        private Color _targetLightColor = Color.white;
        private float _baseOrthographicSize;
        private float _targetOrthographicSize;

        private void Awake()
        {
            if (round == null) round = GetComponent<RoundManager>();
            if (conductor == null) conductor = FindFirstObjectByType<Conductor>();
            if (targetCamera == null) targetCamera = Camera.main;

            if (background != null)
            {
                _baseBackgroundColor = background.color;
                _targetBackgroundColor = _baseBackgroundColor;
            }
            if (globalLight != null)
            {
                _baseLightColor = globalLight.color;
                _targetLightColor = _baseLightColor;
            }
            if (targetCamera != null && targetCamera.orthographic)
            {
                _baseOrthographicSize = targetCamera.orthographicSize;
                _targetOrthographicSize = _baseOrthographicSize;
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;

            if (backgroundMusic != null)
            {
                _backgroundMusicSource = gameObject.AddComponent<AudioSource>();
                _backgroundMusicSource.clip = backgroundMusic;
                _backgroundMusicSource.playOnAwake = false;
                _backgroundMusicSource.loop = true;
                _backgroundMusicSource.spatialBlend = 0f;
                _backgroundMusicSource.volume = backgroundMusicVolume;
            }

            if (FindFirstObjectByType<AudioListener>() == null && Camera.main != null)
                Camera.main.gameObject.AddComponent<AudioListener>();

            if (snareClip == null)
            {
                _runtimeSnare = CreateFallbackSnare();
                snareClip = _runtimeSnare;
            }
        }

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnPhasePreparing += OnPhasePreparing;
                round.OnPhaseChanged += OnPhaseActive;
                round.OnGameOver += OnBattleEnded;
                round.OnStageCleared += OnBattleEnded;
                round.OnStageApplied += OnStageApplied;
            }
            if (conductor != null) conductor.OnClockBeat += OnClockBeat;
            if (conductor != null) conductor.OnPreparationBeat += OnPreparationBeat;
        }

        private void Start()
        {
            if (_backgroundMusicSource == null) return;

            if (conductor != null && conductor.IsRunning)
                _backgroundMusicSource.PlayScheduled(conductor.ScheduledStartDspTime);
            else
                _backgroundMusicSource.Play();
        }

        private void OnDisable()
        {
            if (round != null)
            {
                round.OnPhasePreparing -= OnPhasePreparing;
                round.OnPhaseChanged -= OnPhaseActive;
                round.OnGameOver -= OnBattleEnded;
                round.OnStageCleared -= OnBattleEnded;
                round.OnStageApplied -= OnStageApplied;
            }
            if (conductor != null) conductor.OnClockBeat -= OnClockBeat;
            if (conductor != null) conductor.OnPreparationBeat -= OnPreparationBeat;
            RestoreImmediately();
        }

        private void OnDestroy()
        {
            if (_runtimeSnare != null) Destroy(_runtimeSnare);
        }

        private void Update()
        {
            float t = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);
            if (background != null)
                background.color = Color.Lerp(background.color, _targetBackgroundColor, t);
            if (globalLight != null)
                globalLight.color = Color.Lerp(globalLight.color, _targetLightColor, t);
            if (targetCamera != null && targetCamera.orthographic)
                targetCamera.orthographicSize =
                    Mathf.Lerp(targetCamera.orthographicSize, _targetOrthographicSize, t);
        }

        private void OnPhasePreparing(int phaseIndex, PhaseSO phase)
        {
            _phase = phase;
            _preparing = true;
            ApplyTargets();
        }

        private void OnPhaseActive(int phaseIndex, PhaseSO phase)
        {
            _phase = phase;
            _preparing = false;
            ApplyTargets();
        }

        private void OnClockBeat(int totalBeat)
        {
            if (!_preparing || _phase == null || snareClip == null || _audioSource == null)
                return;
            float volume = _phase.PreparationSnareVolume;
            if (volume > 0.001f) _audioSource.PlayOneShot(snareClip, volume);
        }

        private void OnPreparationBeat(int beat)
        {
            if (_phase == null || snareClip == null || _audioSource == null) return;
            float volume = _phase.PreparationSnareVolume;
            if (volume > 0.001f) _audioSource.PlayOneShot(snareClip, volume);
        }

        private void OnStageApplied(StageSO appliedStage)
        {
            RestoreImmediately();
            if (background != null) _baseBackgroundColor = background.color;
            if (globalLight != null) _baseLightColor = globalLight.color;
            if (targetCamera != null && targetCamera.orthographic)
                _baseOrthographicSize = targetCamera.orthographicSize;
            _phase = null;
            _preparing = false;
            ApplyTargets();
        }

        private void ApplyTargets()
        {
            if (_phase == null)
            {
                _targetBackgroundColor = _baseBackgroundColor;
                _targetLightColor = _baseLightColor;
                _targetOrthographicSize = _baseOrthographicSize;
                return;
            }

            float strength = _preparing
                ? _phase.PreparationTintStrength
                : _phase.ActiveTintStrength;
            _targetBackgroundColor = Color.Lerp(_baseBackgroundColor, _phase.CueColor, strength);
            _targetLightColor = Color.Lerp(_baseLightColor, _phase.CueColor, strength * 0.55f);
            _targetOrthographicSize = _preparing && _phase.IsAttackPhase
                ? _baseOrthographicSize * attackPreparationZoomRatio
                : _baseOrthographicSize;
        }

        private void OnBattleEnded()
        {
            _phase = null;
            _targetBackgroundColor = _baseBackgroundColor;
            _targetLightColor = _baseLightColor;
            _targetOrthographicSize = _baseOrthographicSize;
        }

        private void RestoreImmediately()
        {
            if (background != null) background.color = _baseBackgroundColor;
            if (globalLight != null) globalLight.color = _baseLightColor;
            if (targetCamera != null && targetCamera.orthographic)
                targetCamera.orthographicSize = _baseOrthographicSize;
        }

        private static AudioClip CreateFallbackSnare()
        {
            const int sampleRate = 44100;
            const float duration = 0.085f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            var rng = new System.Random(1947);
            float previousNoise = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float crack = (noise - previousNoise * 0.72f) * Mathf.Exp(-52f * time);
                float body = (
                    Mathf.Sin(2f * Mathf.PI * 190f * time) +
                    Mathf.Sin(2f * Mathf.PI * 330f * time) * 0.35f
                ) * Mathf.Exp(-40f * time);
                float snap = noise * Mathf.Exp(-180f * time);
                samples[i] = Mathf.Clamp(crack * 0.72f + body * 0.38f + snap * 0.32f, -1f, 1f);
                previousNoise = noise;
            }

            AudioClip clip = AudioClip.Create("Fast Intense Snare", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
