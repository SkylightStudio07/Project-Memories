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

        private bool _shown;

        private void OnEnable()
        {
            if (round != null) round.OnGameOver += Show;
        }

        private void OnDisable()
        {
            if (round != null) round.OnGameOver -= Show;
        }

        private void Start()
        {
            _shown = false;
            if (gameOverRoot != null) gameOverRoot.SetActive(false);
        }

        private void Show()
        {
            if (_shown) return;
            _shown = true;

            if (hideOnGameOver != null)
            {
                foreach (GameObject target in hideOnGameOver)
                {
                    if (target != null) target.SetActive(false);
                }
            }

            if (gameOverRoot != null)
            {
                gameOverRoot.SetActive(true);
                gameOverRoot.transform.SetAsLastSibling();
            }
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
