using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatMemories
{
    /// <summary>
    /// 전투 종료 UI를 표시하고 재시작/타이틀 씬 이동을 담당한다.
    /// 이 컴포넌트는 항상 활성 상태인 오브젝트에 두고, gameOverRoot만 비활성 상태로 시작한다.
    /// </summary>
    public class GameOverView : MonoBehaviour
    {
        private const string TitleSceneName = "Title";

        [Header("참조")]
        [SerializeField] private RoundManager round;
        [SerializeField] private GameObject gameOverRoot;

        [Header("게임오버 시 숨길 전투 UI")]
        [SerializeField] private GameObject[] hideOnGameOver;
        [Tooltip("사망 연출이 끝난 뒤 게임오버 UI를 표시하기까지의 시간.")]
        [SerializeField, Min(0f)] private float showDelay = 3.4f;
        [Tooltip("게임오버 UI가 충격감 있게 등장하는 데 걸리는 시간.")]
        [SerializeField, Min(0.05f)] private float impactDuration = 0.28f;
        [SerializeField, Range(0.1f, 1f)] private float impactStartScale = 0.68f;
        [SerializeField, Min(1f)] private float impactOvershootScale = 1.12f;

        private bool _shown;
        private Coroutine _showRoutine;
        private Vector3 _gameOverBaseScale = Vector3.one;

        private void OnEnable()
        {
            if (round != null) round.OnGameOver += Show;
        }

        private void OnDisable()
        {
            if (round != null) round.OnGameOver -= Show;
            if (_showRoutine != null) StopCoroutine(_showRoutine);
            _showRoutine = null;
            if (gameOverRoot != null) gameOverRoot.transform.DOKill();
        }

        private void Start()
        {
            _shown = false;
            if (gameOverRoot != null)
            {
                _gameOverBaseScale = gameOverRoot.transform.localScale;
                gameOverRoot.SetActive(false);
            }
        }

        private void Show()
        {
            if (_shown) return;
            _shown = true;
            _showRoutine = StartCoroutine(ShowAfterDeathPresentation());
        }

        private IEnumerator ShowAfterDeathPresentation()
        {
            if (showDelay > 0f) yield return new WaitForSecondsRealtime(showDelay);

            if (hideOnGameOver != null)
            {
                foreach (GameObject target in hideOnGameOver)
                {
                    if (target != null) target.SetActive(false);
                }
            }

            if (gameOverRoot != null)
            {
                Transform rootTransform = gameOverRoot.transform;
                rootTransform.DOKill();
                rootTransform.localScale = _gameOverBaseScale * impactStartScale;
                gameOverRoot.SetActive(true);
                rootTransform.SetAsLastSibling();

                float punchDuration = Mathf.Max(0.05f, impactDuration);
                Sequence impact = DOTween.Sequence().SetUpdate(true).SetTarget(rootTransform);
                impact.Append(rootTransform
                    .DOScale(_gameOverBaseScale * impactOvershootScale, punchDuration * 0.6f)
                    .SetEase(Ease.OutBack));
                impact.Append(rootTransform
                    .DOScale(_gameOverBaseScale, punchDuration * 0.4f)
                    .SetEase(Ease.InOutSine));
            }
            _showRoutine = null;
        }

        public void RetryCurrentBattle()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.name)) return;
            SceneManager.LoadScene(activeScene.name);
        }

        public void GoToTitle()
        {
            SceneManager.LoadScene(TitleSceneName);
        }
    }
}
