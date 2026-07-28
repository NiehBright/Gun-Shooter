using UnityEngine;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(Collider))]
    public class UIInteractionZone : MonoBehaviour
    {
        public enum InteractionType
        {
            WeaponsTab,
            CharactersTab,
            EquipmentUI,
            SettingsPanel
        }

        [Header("Settings")]
        [SerializeField] InteractionType interactionType = InteractionType.WeaponsTab;
        [SerializeField] float triggerCooldown = 1.5f;

        [Header("Visual Effects (Optional)")]
        [SerializeField] GameObject activateVFXPrefab;
        [SerializeField] Transform vfxSpawnPoint;

        private float lastTriggerTime;
        private bool isPlayerInside; // Quản lý trạng thái nhân vật đang đứng trong hay ngoài vòng

        private void Awake()
        {
            // Đảm bảo Collider được đặt là Trigger để đi xuyên qua và nhận diện được
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
                Debug.Log($"[UIInteractionZone] Da tim thay Collider tren '{gameObject.name}' va thiet lap isTrigger = true.");
            }
            else
            {
                Debug.LogError($"[UIInteractionZone] CANH BAO: Khong tim thay bat ky Collider nao tren '{gameObject.name}'!");
            }

            // Tự động thêm Rigidbody nếu thiếu để đảm bảo kích hoạt va chạm vật lý trong mọi tình huống (kể cả khi Prefab thiếu Rigidbody)
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                Debug.Log($"[UIInteractionZone] Tu dong them Rigidbody (Kinematic) vao '{gameObject.name}' luc runtime!");
            }
            else
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                Debug.Log($"[UIInteractionZone] Da co Rigidbody tren '{gameObject.name}', cau hinh lai ve Kinematic.");
            }

            // Ép đối tượng về Layer 'Default' (Layer 0) để chắc chắn không bị loại trừ khỏi ma trận va chạm vật lý của Player (tránh kẹt ở Layer Obstacle)
            gameObject.layer = 0; 
            Debug.Log($"[UIInteractionZone] Da ep Layer cua '{gameObject.name}' ve 'Default' (0) de nhan va cham.");
        }

        private void Start()
        {
            Collider col = GetComponent<Collider>();
            string boundsInfo = "No Collider";
            if (col != null)
            {
                boundsInfo = $"Bounds Center: {col.bounds.center}, Size: {col.bounds.size}";
            }
            Debug.Log($"[UIInteractionZone] KHOI TAO THANH CONG tren '{gameObject.name}'! (Loai tuong tac: {interactionType}, Layer: {LayerMask.LayerToName(gameObject.layer)}, {boundsInfo})");
        }

        private void OnTriggerEnter(Collider other)
        {
            // Chỉ ghi nhận va chạm vật lý để chẩn đoán (không kích hoạt logic trùng với Update)
            Debug.Log($"[UIInteractionZone] [PHYSICS TRIGGER ENTER] Va cham vat ly phat hien voi: '{other.gameObject.name}'");
        }

        private void Update()
        {
            // Chỉ chạy kiểm tra khoảng cách khi đang ở ngoài sảnh chờ (Lobby / Main Menu)
            var mainMenu = UIController.GetPage<UIMainMenu>();
            if (mainMenu == null || !mainMenu.IsPageDisplayed)
            {
                // KHÔNG reset isPlayerInside tại đây. 
                // Khi bảng nâng cấp mở ra, MainMenu sẽ bị ẩn. Nếu ta reset về false tại đây,
                // lúc đóng bảng nâng cấp (MainMenu hiển thị lại), code sẽ tưởng Player mới đi vào vòng và mở lại UI lập tức.
                return;
            }

            var player = CharacterBehaviour.GetBehaviour();
            if (player == null)
            {
                isPlayerInside = false;
                return;
            }

            // Lấy vị trí của Player và vòng VFX
            Vector3 playerPos = player.transform.position;
            Vector3 zonePos = transform.position;

            // Tính khoảng cách 2D trên mặt phẳng ngang (X, Z) để tránh lệch độ cao mặt đất (Y)
            float distanceXZ = Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(zonePos.x, zonePos.z));

            // Bán kính kích hoạt thực tế (lấy từ Collider hoặc mặc định là 1.5 mét nếu không có collider)
            float triggerRadius = 1.5f;
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                triggerRadius = col.bounds.extents.x;
            }

            // Xác định xem người chơi đang đứng trong vòng hay không
            bool isInsideNow = distanceXZ <= triggerRadius;

            if (isInsideNow)
            {
                // Chỉ kích hoạt khi đây là lần ĐẦU TIÊN người chơi bước vào vòng
                if (!isPlayerInside)
                {
                    isPlayerInside = true;

                    // Kiểm tra thời gian giãn cách (cooldown) để tránh kích hoạt liên tục
                    if (Time.time - lastTriggerTime < triggerCooldown)
                        return;

                    Debug.Log($"[UIInteractionZone] Player buoc vao vong (XZ Distance: {distanceXZ:F2}m <= Radius: {triggerRadius:F2}m). Kich hoat mo UI.");
                    
                    lastTriggerTime = Time.time;
                    TriggerInteraction(mainMenu);
                }
            }
            else
            {
                // Chỉ reset trạng thái đi vào khi người chơi đã thực sự đi ra ngoài vòng hẳn (thêm khoảng đệm 0.3m để tránh chập chờn ở rìa)
                if (isPlayerInside && distanceXZ > triggerRadius + 0.3f)
                {
                    isPlayerInside = false;
                    Debug.Log($"[UIInteractionZone] Player da di ra ngoai vong (XZ Distance: {distanceXZ:F2}m > Radius: {triggerRadius + 0.3f:F2}m). San sang cho lan vao tiep theo.");
                }
            }
        }

        private void TriggerInteraction(UIMainMenu mainMenu)
        {
            Debug.Log($"[UIInteractionZone] Player entered zone. Triggering: {interactionType}");

            // Tạo hiệu ứng VFX nếu được cấu hình
            if (activateVFXPrefab != null)
            {
                Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
                GameObject vfx = Instantiate(activateVFXPrefab, spawnPos, Quaternion.identity);
                Destroy(vfx, 3.0f); // Tự hủy VFX sau 3 giây
            }

            switch (interactionType)
            {
                case InteractionType.WeaponsTab:
                    if (mainMenu.WeaponTab != null)
                    {
                        mainMenu.WeaponTab.OnButtonClicked();
                    }
                    break;

                case InteractionType.CharactersTab:
                    if (mainMenu.CharacterTab != null)
                    {
                        mainMenu.CharacterTab.OnButtonClicked();
                    }
                    break;

                case InteractionType.EquipmentUI:
                    EquipmentPanelUI.Show();
                    break;

                case InteractionType.SettingsPanel:
                    SettingsPanel.ShowPanel();
                    break;
            }
        }
    }
}
