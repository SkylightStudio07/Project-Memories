using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 스프라이트 발밑에 소프트 타원 드롭섀도를 깐다. 자식 "Shadow" SpriteRenderer를 만들어
    /// 스프라이트 하단(발) 위치에 어둡게 배치하고, 캐릭터 정렬 바로 아래로 그린다.
    /// 크기/오프셋은 월드 단위(부모 스케일과 무관). 에디터 미리보기(ExecuteAlways).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DropShadow2D : MonoBehaviour
    {
        [Tooltip("그림자 스프라이트(소프트 원형). 비우면 자동 로드")]
        [SerializeField] private Sprite blob;
        [SerializeField] private Color color = new Color(0f, 0f, 0f, 0.5f);
        [Tooltip("그림자 월드 크기(가로, 세로)")]
        [SerializeField] private Vector2 worldSize = new Vector2(1.4f, 0.45f);
        [Tooltip("발밑 기준 추가 오프셋(월드)")]
        [SerializeField] private Vector2 worldOffset = new Vector2(0f, 0.05f);
        [Tooltip("캐릭터 정렬보다 이만큼 아래에 그림")]
        [SerializeField] private int orderBelow = 1;
        [Tooltip("바닥 그림자 각도(도). 0=수평. 부모(스프라이트) 회전과 무관하게 고정")]
        [SerializeField] private float groundAngle = 0f;

        private SpriteRenderer _own;
        private Transform _shadow;
        private SpriteRenderer _shadowSr;

        private void OnEnable() { Ensure(); Apply(); }
        private void OnValidate() { if (isActiveAndEnabled) { Ensure(); Apply(); } }
        private void LateUpdate() { Apply(); } // 캐릭터가 움직여도 따라오도록

        private void Ensure()
        {
            if (_own == null) _own = GetComponent<SpriteRenderer>();
            if (_shadow == null)
            {
                Transform t = transform.Find("Shadow");
                if (t == null)
                {
                    var go = new GameObject("Shadow");
                    go.transform.SetParent(transform, false);
                    t = go.transform;
                }
                _shadow = t;
                _shadowSr = t.GetComponent<SpriteRenderer>();
                if (_shadowSr == null) _shadowSr = t.gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private void Apply()
        {
            if (_shadowSr == null || _own == null) return;
            if (blob != null) _shadowSr.sprite = blob;
            _shadowSr.color = color;
            _shadowSr.sortingLayerID = _own.sortingLayerID;
            _shadowSr.sortingOrder = _own.sortingOrder - orderBelow;

            // 발밑 = 스프라이트 바운드 하단. 월드 위치로 직접 배치.
            Vector3 wp = transform.position;
            wp.x += worldOffset.x;
            wp.y = _own.bounds.min.y + worldOffset.y;
            wp.z = transform.position.z + 0.01f;
            _shadow.position = wp;

            // 월드 크기 → 부모 스케일 보정한 로컬 스케일
            Vector3 ls = transform.lossyScale;
            float bw = blob != null ? Mathf.Max(1e-4f, blob.bounds.size.x) : 1f;
            float bh = blob != null ? Mathf.Max(1e-4f, blob.bounds.size.y) : 1f;
            _shadow.localScale = new Vector3(
                worldSize.x / Mathf.Max(1e-4f, ls.x) / bw,
                worldSize.y / Mathf.Max(1e-4f, ls.y) / bh,
                1f);
            // 부모(스프라이트) 회전을 상속하지 않고 바닥에 평평하게 — 월드 회전 고정
            _shadow.rotation = Quaternion.Euler(0f, 0f, groundAngle);
        }
    }
}
