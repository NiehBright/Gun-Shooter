using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Vòng tròn định vị phạm vi ngắm chiêu 3D trên mặt đất cho Nieh (Sát Thương Quỹ Đạo - Orbital Laser)
    /// Hiển thị viền đỏ rực rỡ + vùng phủ mờ màu đỏ + tâm ngắm chính xác.
    /// </summary>
    public class SkillTargetIndicator : MonoBehaviour
    {
        private static SkillTargetIndicator instance;
        public static SkillTargetIndicator Instance => instance;

        [Header("Targeting Settings")]
        [SerializeField] private float radius = 1.5f;
        [SerializeField] private float ringThickness = 0.12f;

        private GameObject outerRingObj;
        private GameObject innerFillObj;
        private GameObject centerDotObj;

        private MeshFilter outerRingFilter;
        private MeshRenderer outerRingRenderer;
        private MeshFilter innerFillFilter;
        private MeshRenderer innerFillRenderer;

        private Material ringMaterial;
        private Material fillMaterial;
        private Material dotMaterial;

        private bool isVisible = false;
        public bool IsVisible => isVisible;

        public static SkillTargetIndicator GetOrCreate()
        {
            if (instance != null) return instance;

            GameObject root = new GameObject("[SkillTargetIndicator]");
            instance = root.AddComponent<SkillTargetIndicator>();
            instance.Initialise();
            return instance;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Initialise();
            }
        }

        private void Initialise()
        {
            // Vật liệu màu đỏ Neon
            Color ringColor = new Color(1.0f, 0.18f, 0.18f, 0.95f);
            Color fillColor = new Color(1.0f, 0.15f, 0.15f, 0.22f);
            Color dotColor = new Color(1.0f, 0.30f, 0.30f, 0.95f);

            ringMaterial = AoECircleMeshUtility.CreateIndicatorMaterial(ringColor);
            fillMaterial = AoECircleMeshUtility.CreateIndicatorMaterial(fillColor);
            dotMaterial = AoECircleMeshUtility.CreateIndicatorMaterial(dotColor);

            // 1. Viền tròn đỏ bên ngoài (Outer Ring)
            outerRingObj = new GameObject("OuterRing");
            outerRingObj.transform.SetParent(transform, false);
            outerRingObj.transform.localPosition = new Vector3(0, 0.01f, 0);

            outerRingFilter = outerRingObj.AddComponent<MeshFilter>();
            outerRingRenderer = outerRingObj.AddComponent<MeshRenderer>();
            outerRingRenderer.material = ringMaterial;
            outerRingRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outerRingRenderer.receiveShadows = false;

            // 2. Vùng đỏ mờ bên trong (Inner Disc Fill)
            innerFillObj = new GameObject("InnerFill");
            innerFillObj.transform.SetParent(transform, false);
            innerFillObj.transform.localPosition = Vector3.zero;

            innerFillFilter = innerFillObj.AddComponent<MeshFilter>();
            innerFillRenderer = innerFillObj.AddComponent<MeshRenderer>();
            innerFillRenderer.material = fillMaterial;
            innerFillRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            innerFillRenderer.receiveShadows = false;

            // 3. Tâm định vị chính giữa (Center Dot)
            centerDotObj = new GameObject("CenterDot");
            centerDotObj.transform.SetParent(transform, false);
            centerDotObj.transform.localPosition = new Vector3(0, 0.02f, 0);

            var dotFilter = centerDotObj.AddComponent<MeshFilter>();
            var dotRenderer = centerDotObj.AddComponent<MeshRenderer>();
            dotRenderer.material = dotMaterial;
            dotRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dotRenderer.receiveShadows = false;
            dotFilter.mesh = AoECircleMeshUtility.CreateDiscMesh(0.18f, 24);

            BuildMeshes();
            Hide();
        }

        private void BuildMeshes()
        {
            if (outerRingFilter != null)
            {
                outerRingFilter.mesh = AoECircleMeshUtility.CreateRingMesh(radius, ringThickness, 64);
            }

            if (innerFillFilter != null)
            {
                innerFillFilter.mesh = AoECircleMeshUtility.CreateDiscMesh(radius, 64);
            }
        }

        public void SetRadius(float newRadius)
        {
            if (Mathf.Approximately(radius, newRadius)) return;
            radius = Mathf.Max(0.5f, newRadius);
            BuildMeshes();
        }

        public void SetPosition(Vector3 worldPos)
        {
            // Đặt trên mặt đất nhẹ nhàng (+0.04m) để tránh z-fighting
            transform.position = new Vector3(worldPos.x, worldPos.y + 0.04f, worldPos.z);
        }

        public void Show()
        {
            isVisible = true;
            gameObject.SetActive(true);
            if (outerRingRenderer != null) outerRingRenderer.enabled = true;
            if (innerFillRenderer != null) innerFillRenderer.enabled = true;
            if (centerDotObj != null) centerDotObj.SetActive(true);
        }

        public void Hide()
        {
            isVisible = false;
            if (outerRingRenderer != null) outerRingRenderer.enabled = false;
            if (innerFillRenderer != null) innerFillRenderer.enabled = false;
            if (centerDotObj != null) centerDotObj.SetActive(false);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!isVisible) return;

            // Xoay nhẹ viền ngoài để tạo cảm giác radar/aim công nghệ cao sống động
            if (outerRingObj != null)
            {
                outerRingObj.transform.Rotate(0f, 40f * Time.deltaTime, 0f);
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
