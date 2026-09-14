using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Bộ xử lý thao tác chạm trên nút Kỹ năng (Skill Button)
    /// Hỗ trợ Nhấn nhanh (Tap - Auto-aim vào quái gần nhất) và Ấn giữ/Kéo ngắm (Hold & Drag - Định vị chiêu tự do trên mặt đất).
    /// </summary>
    public class SkillButtonBehavior : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Aiming Settings")]
        [SerializeField] private float dragAimSensitivity = 110f; // Khoảng cách kéo đạt tầm xa cực đại
        [SerializeField] private float maxSkillRange = 10f;      // Tầm ngắm pháo kích cực đại
        [SerializeField] private float holdThreshold = 0.15f;    // Thời gian giữ để chuyển từ Tap sang Aim

        private SkillTargetIndicator targetIndicator;
        private bool isPressed = false;
        private bool isAimingSkill = false;
        private bool hasStartedAiming = false;
        private float pressStartTime = 0f;
        private Vector2 pointerDownPos = Vector2.zero;
        private Vector3 aimedWorldPosition = Vector3.zero;

        private void Awake()
        {
            // Tự động tìm hoặc khởi tạo SkillTargetIndicator trong Scene
            targetIndicator = SkillTargetIndicator.GetOrCreate();
        }

        private void Start()
        {
            if (targetIndicator == null)
            {
                targetIndicator = SkillTargetIndicator.GetOrCreate();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var player = CharacterBehaviour.GetBehaviour();
            if (player == null || !player.IsSkillReady) return;

            var character = CharactersController.SelectedCharacter;
            if (character == null || character.SkillData == null) return;

            isPressed = true;
            hasStartedAiming = false;
            pressStartTime = Time.unscaledTime;
            pointerDownPos = eventData.position;

            // Kiểm tra xem kỹ năng có hỗ trợ ngắm kéo tự do không (Hiện tại là Nieh - OrbitalLaser)
            if (character.SkillData.SkillType == SkillType.OrbitalLaser)
            {
                isAimingSkill = true;

                if (targetIndicator == null)
                {
                    targetIndicator = SkillTargetIndicator.GetOrCreate();
                }

                // Cập nhật bán kính chỉ thị khớp với AoE của chiêu
                float actualRadius = Mathf.Min(character.SkillData.AoeRadius, 1.5f);
                targetIndicator.SetRadius(actualRadius);

                // Điểm ngắm mặc định: Vị trí quái gần nhất hoặc phía trước mặt nhân vật
                if (player.ClosestEnemyBehaviour != null && !player.ClosestEnemyBehaviour.IsDead)
                {
                    aimedWorldPosition = player.ClosestEnemyBehaviour.transform.position;
                }
                else
                {
                    aimedWorldPosition = player.transform.position + player.transform.forward * 3.5f;
                }

                aimedWorldPosition.y = player.transform.position.y;
                targetIndicator.SetPosition(aimedWorldPosition);
            }
            else
            {
                isAimingSkill = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isPressed) return;

            if (isAimingSkill)
            {
                Vector2 delta = eventData.position - pointerDownPos;

                // Khi ngón tay dịch chuyển trên 16 pixel hoặc đã giữ đủ lâu -> Bật chế độ ngắm
                if (!hasStartedAiming && (delta.magnitude > 16f || (Time.unscaledTime - pressStartTime > holdThreshold)))
                {
                    hasStartedAiming = true;
                    if (targetIndicator != null) targetIndicator.Show();
                }

                if (hasStartedAiming)
                {
                    UpdateAimPosition(delta);
                }
            }
        }

        private void Update()
        {
            if (!isPressed) return;

            // Nếu người chơi ấn giữ tại chỗ quá ngưỡng holdThreshold mà chưa kéo -> Vẫn kích hoạt vòng tròn ngắm tại quái gần nhất
            if (isAimingSkill && !hasStartedAiming)
            {
                if (Time.unscaledTime - pressStartTime > holdThreshold)
                {
                    hasStartedAiming = true;
                    if (targetIndicator != null)
                    {
                        targetIndicator.Show();
                        targetIndicator.SetPosition(aimedWorldPosition);
                    }
                }
            }
        }

        private void UpdateAimPosition(Vector2 screenDelta)
        {
            var player = CharacterBehaviour.GetBehaviour();
            if (player == null) return;

            Camera cam = CameraController.MainCamera != null ? CameraController.MainCamera : Camera.main;
            if (cam == null) return;

            // Nếu độ lệch rất nhỏ, ưu tiên khóa quái gần nhất nếu có
            if (screenDelta.magnitude < 15f && player.ClosestEnemyBehaviour != null && !player.ClosestEnemyBehaviour.IsDead)
            {
                aimedWorldPosition = player.ClosestEnemyBehaviour.transform.position;
            }
            else
            {
                // Chuyển đổi vector kéo trên màn hình sang hướng trong thế giới 3D tương đối theo Camera
                Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
                Vector3 aimDir = (camRight * screenDelta.x + camForward * screenDelta.y).normalized;

                float dpiMultiplier = Screen.dpi > 0 ? Screen.dpi / 160f : 1f;
                float effectiveMaxDrag = dragAimSensitivity * dpiMultiplier;
                float dragRatio = Mathf.Clamp01(screenDelta.magnitude / effectiveMaxDrag);

                float currentRange = Mathf.Max(dragRatio * maxSkillRange, 1.2f);
                aimedWorldPosition = player.transform.position + aimDir * currentRange;
            }

            aimedWorldPosition.y = player.transform.position.y;
            if (targetIndicator != null)
            {
                targetIndicator.SetPosition(aimedWorldPosition);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;

            var player = CharacterBehaviour.GetBehaviour();
            if (player == null || !player.IsSkillReady)
            {
                if (targetIndicator != null) targetIndicator.Hide();
                hasStartedAiming = false;
                isAimingSkill = false;
                return;
            }

            if (isAimingSkill)
            {
                if (targetIndicator != null) targetIndicator.Hide();

                if (hasStartedAiming)
                {
                    // Người chơi đã ngắm kéo -> Kích hoạt pháo kích tại vị trí ngắm đã chọn!
                    player.ActivateSkill(aimedWorldPosition);
                }
                else
                {
                    // Người chơi chỉ nhấn nhanh (Tap) -> Tự động khóa quái gần nhất
                    player.ActivateSkill();
                }
            }
            else
            {
                // Các nhân vật khác (NinNin, BlackHole...) kích hoạt bình thường
                player.ActivateSkill();
            }

            hasStartedAiming = false;
            isAimingSkill = false;
        }

        /// <summary>
        /// Gọi kích hoạt khi bấm nút thông thường (ví dụ từ phím tắt bàn phím hoặc Gamepad)
        /// </summary>
        public void TriggerSkillDirectly()
        {
            var player = CharacterBehaviour.GetBehaviour();
            if (player != null && player.IsSkillReady)
            {
                player.ActivateSkill();
            }
        }
    }
}
