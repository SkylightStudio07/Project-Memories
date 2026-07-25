using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 스테이지 시작 전(프롤로그) · 스테이지 전환 시 대사를 재생하는 렌더러.
    /// 적 대사는 우측 하단 창, 플레이어 대사는 좌측 하단 창에 표시하며
    /// 화자에 맞는 창만 보인다. 대사는 타자기처럼 한 글자씩 나타나며,
    /// 타이핑 중 클릭하면 즉시 완성하고 그다음 클릭에 다음 줄로 넘어간다.
    /// 프리팹으로 관리해 씬마다 재사용한다. StageManager가 <see cref="PlayRoutine"/>을
    /// 카운트인 시작 전에 호출한다.
    /// </summary>
    public class DialogueViewer : MonoBehaviour
    {
        [Header("전체 루트 (재생 중에만 활성)")]
        [SerializeField] private GameObject root;
        [Tooltip("화면 전체를 덮어 클릭으로 다음 줄 진행 + 뒤 UI 입력 차단")]
        [SerializeField] private Button advanceButton;

        [Header("타자기 연출 (인스펙터 조정)")]
        [Tooltip("초당 표시할 글자 수. 0이면 즉시 전체 표시")]
        [SerializeField, Min(0f)] private float charsPerSecond = 40f;

        [Header("적 대사창 (우측 하단)")]
        [SerializeField] private GameObject enemyWindow;
        [SerializeField] private Image enemyPortrait;
        [SerializeField] private TMP_Text enemyName;
        [SerializeField] private TMP_Text enemyText;

        [Header("플레이어 대사창 (좌측 하단)")]
        [SerializeField] private GameObject playerWindow;
        [SerializeField] private Image playerPortrait;
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text playerText;

        private bool _advanced;
        private bool _typing;
        private Sprite _lastEnemyPortrait;
        private Sprite _lastPlayerPortrait;
        private string _lastEnemyName;
        private string _lastPlayerName;
        private Coroutine _typingRoutine;
        private TMP_Text _typingTarget;
        private string _typingFullText;

        public bool IsPlaying { get; private set; }

        // 초기 비활성 상태는 프리팹 저장값(root/enemyWindow/playerWindow 모두 비활성)에 맡긴다.
        // StageManager(DefaultExecutionOrder -100)가 Awake에서 곧바로 다이얼로그를 재생할 수
        // 있으므로, 여기서 SetActive(false)를 실행하면 순서에 따라 방금 켠 걸 도로 꺼버릴 수 있다.
        private void Awake()
        {
            if (advanceButton != null) advanceButton.onClick.AddListener(OnAdvanceClicked);
        }

        private void OnDestroy()
        {
            if (advanceButton != null) advanceButton.onClick.RemoveListener(OnAdvanceClicked);
        }

        /// <summary>대사 SO를 순서대로 재생하고 마지막 줄이 넘어가면 반환한다. 비어 있으면 즉시 반환.</summary>
        public IEnumerator PlayRoutine(DialogueSO dialogue)
        {
            var lines = dialogue != null ? dialogue.lines : null;
            if (lines == null || lines.Count == 0) yield break;

            IsPlaying = true;
            _lastEnemyPortrait = null;
            _lastPlayerPortrait = null;
            _lastEnemyName = null;
            _lastPlayerName = null;
            if (root != null) root.SetActive(true);

            for (int i = 0; i < lines.Count; i++)
            {
                ShowLine(lines[i]);
                _advanced = false;
                yield return new WaitUntil(() => _advanced);
            }

            StopTyping();
            if (enemyWindow != null) enemyWindow.SetActive(false);
            if (playerWindow != null) playerWindow.SetActive(false);
            if (root != null) root.SetActive(false);
            IsPlaying = false;
        }

        private void ShowLine(DialogueLine line)
        {
            bool isEnemy = line.speaker == DialogueSpeaker.Enemy;
            // 한쪽 창을 켜기 전에 반대쪽을 먼저 꺼서 두 창이 겹쳐 보이는 프레임이 없게 한다.
            if (isEnemy) { if (playerWindow != null) playerWindow.SetActive(false); }
            else { if (enemyWindow != null) enemyWindow.SetActive(false); }
            if (enemyWindow != null) enemyWindow.SetActive(isEnemy);
            if (playerWindow != null) playerWindow.SetActive(!isEnemy);

            string body = string.IsNullOrEmpty(line.text) ? line.text : line.text.Replace("\\n", "\n");

            if (isEnemy)
            {
                if (line.portrait != null) _lastEnemyPortrait = line.portrait;
                if (!string.IsNullOrEmpty(line.speakerName)) _lastEnemyName = line.speakerName;
                SetPortrait(enemyPortrait, _lastEnemyPortrait);
                if (enemyName != null) enemyName.text = _lastEnemyName;
                StartTyping(enemyText, body);
            }
            else
            {
                if (line.portrait != null) _lastPlayerPortrait = line.portrait;
                if (!string.IsNullOrEmpty(line.speakerName)) _lastPlayerName = line.speakerName;
                SetPortrait(playerPortrait, _lastPlayerPortrait);
                if (playerName != null) playerName.text = _lastPlayerName;
                StartTyping(playerText, body);
            }
        }

        private void StartTyping(TMP_Text target, string fullText)
        {
            StopTyping();
            _typingTarget = target;
            _typingFullText = fullText;
            if (target == null) return;

            if (charsPerSecond <= 0f || string.IsNullOrEmpty(fullText))
            {
                target.text = fullText;
                return;
            }

            target.text = string.Empty;
            _typing = true;
            _typingRoutine = StartCoroutine(TypeText(target, fullText));
        }

        private IEnumerator TypeText(TMP_Text target, string fullText)
        {
            var sb = new StringBuilder();
            float delay = 1f / charsPerSecond;
            for (int i = 0; i < fullText.Length; i++)
            {
                sb.Append(fullText[i]);
                target.text = sb.ToString();
                yield return new WaitForSecondsRealtime(delay);
            }
            _typing = false;
            _typingRoutine = null;
        }

        /// <summary>진행 중인 타자기 연출을 즉시 끝내 전체 텍스트를 보여준다.</summary>
        private void SkipTyping()
        {
            if (_typingRoutine != null) StopCoroutine(_typingRoutine);
            _typingRoutine = null;
            _typing = false;
            if (_typingTarget != null) _typingTarget.text = _typingFullText;
        }

        private void StopTyping()
        {
            if (_typingRoutine != null) StopCoroutine(_typingRoutine);
            _typingRoutine = null;
            _typing = false;
        }

        private static void SetPortrait(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private void OnAdvanceClicked()
        {
            if (_typing) SkipTyping();
            else _advanced = true;
        }
    }
}
