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
        [Tooltip("시작(카운트인) 시 EnemyActor에 미리 표시할 스프라이트. 비우면 풀의 첫 적 스프라이트")]
        public Sprite enemySprite;

        [Header("리듬 / 페이즈")]
        public RhythmPatternSO pattern;
        public List<PhaseSO> phases = new List<PhaseSO>();
        [Min(1)] public int cyclesPerPhase = 2;

        [Header("수치")]
        [Min(1f)] public float bpm = 90f;
        [Min(1)] public int playerMaxHp = 8;

        [Header("배경 (옵션 — 비우면 씬 기존 것 유지)")]
        [Tooltip("씬의 '백그라운드' SpriteRenderer에 바꿔끼울 스프라이트")]
        public Sprite backgroundSprite;
        [Tooltip("씬의 '바닥' SpriteRenderer에 바꿔끼울 스프라이트")]
        public Sprite floorSprite;
        [Tooltip("(레거시) 지정 시 이 프리팹을 배경으로 인스턴스화")]
        public GameObject backgroundPrefab;
    }
}
