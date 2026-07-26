using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 스테이지 한 개의 데이터 정의 (SO). 스테이지마다 적 풀·사용 능력(키)·리듬·수치·배경이 다르다.
    /// <see cref="RoundManager"/>가 이 값으로 자기 설정을 구성한다.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage_", menuName = "Beat Memories/Stage", order = 3)]
    public class StageSO : ScriptableObject
    {
        [Header("식별")]
        public int stageNumber = 1;
        public string displayName;

        [Header("능력 / 입력")]
        [Tooltip("이 스테이지에서 쓰는 키 (공/방=2키, +차징=3키)")]
        public KeyMode keyMode = KeyMode.TwoKey;

        [Header("적 풀 (이 스테이지에 나오는 적)")]
        public List<Enemy> enemyPool = new List<Enemy>();

        [Header("Character Prefabs")]
        [Tooltip("플레이어 객체/Anchor 프리팹. 비어 있으면 씬의 기존 SpriteRenderer를 사용한다.")]
        public CharacterView playerPrefab;
        [Tooltip("이 스테이지 적 객체/Anchor 프리팹. 비어 있으면 씬의 기존 SpriteRenderer를 사용한다.")]
        public CharacterView enemyPrefab;

        [Tooltip("시작(카운트인) 시 EnemyActor에 미리 표시할 스프라이트. 비우면 풀의 첫 적 스프라이트")]
        public Sprite enemySprite;
        [Tooltip("EnemyActor idle 스프라이트만 좌우 반전")]
        public bool flipEnemyIdleX;

        [Header("리듬 / 페이즈")]
        public RhythmPatternSO pattern;
        public List<PhaseSO> phases = new List<PhaseSO>();
        [Min(1)] public int cyclesPerPhase = 2;
        [Tooltip("Preparation beats inserted between phases. Zero preserves the legacy timeline.")]
        [Min(0)] public int phasePreparationBeats;
        [Tooltip("켜면 마지막 페이즈 뒤 처음으로 순환. 끄면 마지막 페이즈를 적 HP가 0이 될 때까지 유지")]
        public bool repeatPhasePlan = true;

        [Header("수치")]
        [Min(1f)] public float bpm = 96f;
        [Min(1)] public int playerMaxHp = 8;
        [Tooltip("이 스테이지 적의 최대 HP. 처리(Cleared) 판정마다 1 감소")]
        [Min(1)] public int enemyMaxHp = 8;
        [Tooltip("첫 준비 4박 전에 둘 별도 무박자 카운트인. Stage 1은 준비가 카운트인을 겸해 0")]
        [Min(0f)] public float startDelay = 3f;

        [Header("보스 페이지")]
        [Tooltip("적 HP를 모두 소진해야 하는 페이지 수. 1이면 기존 단일 HP 동작")]
        [Min(1)] public int enemyPageCount = 1;
        [Tooltip("다음 페이지로 넘어갈 때 입력과 전투 진행을 멈출 비트 수")]
        [Min(0)] public int enemyPageTransitionBeats;
        [Tooltip("2페이지부터 예측 슬롯의 아래쪽 절반만 표시")]
        public bool cutPreviewBottomHalfOnSecondPage;
        [Tooltip("페이지 전환 중 EnemyActor에 표시할 공격 자세")]
        public Sprite enemyPageTransitionSprite;

        [Header("다이얼로그 (스테이지 시작 전, 카운트인 전에 재생)")]
        [Tooltip("비우면 다이얼로그 없이 바로 카운트인 시작")]
        public DialogueSO introDialogue;

        [Tooltip("이 스테이지의 적을 처치한 직후 재생. 마지막 스테이지면 게임 클리어 화면 직전에 나온다")]
        public DialogueSO outroDialogue;

        [Tooltip("보스 2페이지부터의 돌입 대사. 인덱스 0 = 2페이지 진입 시. 비우면 대사 없이 전환")]
        public List<DialogueSO> pageTransitionDialogues = new List<DialogueSO>();

        [Header("배경 (옵션 — 비우면 씬 기존 것 유지)")]
        [Tooltip("씬의 '백그라운드' SpriteRenderer에 바꿔끼울 스프라이트")]
        public Sprite backgroundSprite;
        [Tooltip("씬의 '바닥' SpriteRenderer에 바꿔끼울 스프라이트")]
        public Sprite floorSprite;
        [Tooltip("(레거시) 지정 시 이 프리팹을 배경으로 인스턴스화")]
        public GameObject backgroundPrefab;
    }
}
