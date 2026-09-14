using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShadowCloneBehaviour : MonoBehaviour
    {
        [Header("Clone Settings")]
        [SerializeField] private float tauntRadius = 12f;
        [SerializeField] private float explosionRadius = 3.2f;
        [SerializeField] private float explosionDamage = 200f;
        [SerializeField] private float duration = 3.5f;
        [SerializeField] private float stunDuration = 1.5f;
        [SerializeField] private GameObject explosionVfxPrefab;

        [Header("Lightning Shock Aura")]
        [SerializeField] private float shockRadius = 2.0f;
        [SerializeField] private float shockDamagePerTick = 25f;
        [SerializeField] private float shockTickInterval = 0.35f;

        public float TauntRadius { get => tauntRadius; set => tauntRadius = value; }
        public float ExplosionRadius { get => explosionRadius; set => explosionRadius = value; }
        public float ExplosionDamage { get => explosionDamage; set => explosionDamage = value; }
        public float Duration { get => duration; set => duration = value; }
        public float StunDuration { get => stunDuration; set => stunDuration = value; }
        public GameObject ExplosionVfxPrefab { get => explosionVfxPrefab; set => explosionVfxPrefab = value; }
        public float ShockRadius { get => shockRadius; set => shockRadius = value; }
        public float ShockDamagePerTick { get => shockDamagePerTick; set => shockDamagePerTick = value; }

        private float lifetime;
        private float nextTauntScanTime;
        private float nextShockTime;
        private bool isInitialised = false;
        private bool hasDetonated = false;
        private readonly HashSet<BaseEnemyBehavior> tauntedEnemies = new HashSet<BaseEnemyBehavior>();

        // Chỉ thị phạm vi dưới mặt đất (Ground Indicators)
        private Transform shockAuraTransform;
        private Transform explosionTelegraphTransform;
        private Transform expandingFillTransform;
        private MeshRenderer outerExplosionRenderer;

        public void Initialise(float tauntRadius, float explosionRadius, float explosionDamage, float duration, float stunDuration, GameObject explosionVfxPrefab, float shockDamagePerTick = 25f)
        {
            this.tauntRadius = tauntRadius;
            this.explosionRadius = explosionRadius;
            this.explosionDamage = explosionDamage;
            this.duration = duration;
            this.stunDuration = stunDuration;
            if (explosionVfxPrefab != null) this.explosionVfxPrefab = explosionVfxPrefab;
            this.shockDamagePerTick = shockDamagePerTick;

            this.lifetime = 0f;
            this.nextTauntScanTime = 0f;
            this.nextShockTime = 0.1f; // Bắt đầu giật sét ngay sau 0.1s
            this.isInitialised = true;
            this.hasDetonated = false;

            // Khởi tạo các vòng tròn chỉ thị trực quan dưới mặt đất (Vòng tròn sét + Vòng tròn nổ đếm ngược)
            CreateGroundIndicators();

            // Quét và khiêu khích quái vật xung quanh ngay lập tức
            ScanAndTauntEnemies();
        }

        private void CreateGroundIndicators()
        {
            try
            {
                // 1. Vòng tròn phạm vi sấm sét dưới chân phân thân (Shock Aura Ground Ring)
                GameObject shockObj = new GameObject("ShockAuraRing");
                shockObj.transform.SetParent(transform, false);
                shockObj.transform.localPosition = new Vector3(0, 0.02f, 0);
                shockAuraTransform = shockObj.transform;

                // Vành đai ngoài màu xanh Lôi điện (Cyan Neon)
                var shockRingObj = new GameObject("Ring");
                shockRingObj.transform.SetParent(shockObj.transform, false);
                var shockRingFilter = shockRingObj.AddComponent<MeshFilter>();
                var shockRingRenderer = shockRingObj.AddComponent<MeshRenderer>();
                Color shockRingColor = new Color(0.1f, 0.9f, 1f, 0.85f);
                shockRingRenderer.material = AoECircleMeshUtility.CreateIndicatorMaterial(shockRingColor);
                shockRingRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                shockRingRenderer.receiveShadows = false;
                shockRingFilter.mesh = AoECircleMeshUtility.CreateRingMesh(shockRadius, 0.12f, 48);

                // Vùng phủ mờ bên trong màu xanh Cyan
                var shockFillObj = new GameObject("Fill");
                shockFillObj.transform.SetParent(shockObj.transform, false);
                var shockFillFilter = shockFillObj.AddComponent<MeshFilter>();
                var shockFillRenderer = shockFillObj.AddComponent<MeshRenderer>();
                Color shockFillColor = new Color(0.1f, 0.9f, 1f, 0.08f);
                shockFillRenderer.material = AoECircleMeshUtility.CreateIndicatorMaterial(shockFillColor);
                shockFillRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                shockFillRenderer.receiveShadows = false;
                shockFillFilter.mesh = AoECircleMeshUtility.CreateDiscMesh(shockRadius, 48);

                // 2. Vòng tròn cảnh báo phát nổ đếm ngược (Explosion Telegraph Countdown)
                GameObject explosionObj = new GameObject("ExplosionTelegraph");
                explosionObj.transform.SetParent(transform, false);
                explosionObj.transform.localPosition = new Vector3(0, 0.03f, 0);
                explosionTelegraphTransform = explosionObj.transform;

                // Vòng tròn lớn viền đỏ bên ngoài thể hiện phạm vi nổ cực đại
                var outerRingObj = new GameObject("OuterExplosionRing");
                outerRingObj.transform.SetParent(explosionObj.transform, false);
                var outerRingFilter = outerRingObj.AddComponent<MeshFilter>();
                outerExplosionRenderer = outerRingObj.AddComponent<MeshRenderer>();
                Color outerRedColor = new Color(1f, 0.15f, 0.15f, 0.95f);
                outerExplosionRenderer.material = AoECircleMeshUtility.CreateIndicatorMaterial(outerRedColor);
                outerExplosionRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                outerExplosionRenderer.receiveShadows = false;
                outerRingFilter.mesh = AoECircleMeshUtility.CreateRingMesh(explosionRadius, 0.15f, 64);

                // Vòng tròn nhỏ màu đỏ bên trong tràn dần từ tâm ra ngoài
                var expandingObj = new GameObject("ExpandingFill");
                expandingObj.transform.SetParent(explosionObj.transform, false);
                expandingObj.transform.localPosition = Vector3.zero;
                expandingFillTransform = expandingObj.transform;
                expandingFillTransform.localScale = new Vector3(0f, 1f, 0f); // Bắt đầu từ tâm (scale = 0)

                var expandingFilter = expandingObj.AddComponent<MeshFilter>();
                var expandingRenderer = expandingObj.AddComponent<MeshRenderer>();
                Color expandingRedColor = new Color(1f, 0.15f, 0.15f, 0.35f);
                expandingRenderer.material = AoECircleMeshUtility.CreateIndicatorMaterial(expandingRedColor);
                expandingRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                expandingRenderer.receiveShadows = false;
                expandingFilter.mesh = AoECircleMeshUtility.CreateDiscMesh(explosionRadius, 64);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[ShadowCloneBehaviour] Không thể tạo ground indicators: " + ex.Message);
            }
        }

        /// <summary>
        /// Sao chép chính xác dáng đứng (pose) và các khớp xương của nhân vật NinNin tại thời điểm kích hoạt chiêu
        /// </summary>
        public void CopyPoseFrom(GameObject sourceGraphics)
        {
            if (sourceGraphics == null) return;

            // Xóa graphic mặc định (nếu có trong prefab)
            Transform existingGraphic = transform.Find("Graphic");
            if (existingGraphic != null)
            {
                Destroy(existingGraphic.gameObject);
            }

            // Tạo bản sao trực tiếp từ graphics của người chơi
            GameObject cloneGraphic = Instantiate(sourceGraphics, transform);
            cloneGraphic.name = "Graphic";
            cloneGraphic.transform.localPosition = Vector3.zero;
            cloneGraphic.transform.localRotation = Quaternion.identity;
            cloneGraphic.transform.localScale = sourceGraphics.transform.localScale;

            // Gỡ bỏ các component gameplay logic của người chơi khỏi phân thân
            var characterGraphics = cloneGraphic.GetComponent<BaseCharacterGraphics>();
            if (characterGraphics != null) Destroy(characterGraphics);

            var rigBuilder = cloneGraphic.GetComponent<UnityEngine.Animations.Rigging.RigBuilder>();
            if (rigBuilder != null) Destroy(rigBuilder);

            // Sao chép chính xác vị trí và góc xoay của từng xương (Bones)
            CopyBonesRecursively(sourceGraphics.transform, cloneGraphic.transform);

            // Tắt Animator để không bao giờ bị reset về Bind Pose (dáng chữ T)
            var animators = cloneGraphic.GetComponentsInChildren<Animator>();
            foreach (var anim in animators)
            {
                anim.enabled = false;
            }

            // Thêm hiệu ứng viền Neon Hologram màu xanh Cyber
            var outline = cloneGraphic.GetComponent<Outline>() ?? cloneGraphic.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = new Color(0.1f, 0.9f, 1f, 0.95f);
            outline.OutlineWidth = 2.2f;
        }

        private void CopyBonesRecursively(Transform source, Transform destination)
        {
            destination.localPosition = source.localPosition;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;

            for (int i = 0; i < source.childCount; i++)
            {
                Transform sourceChild = source.GetChild(i);
                Transform destChild = destination.Find(sourceChild.name);
                if (destChild != null)
                {
                    CopyBonesRecursively(sourceChild, destChild);
                }
            }
        }

        private void Update()
        {
            if (!isInitialised || hasDetonated) return;

            lifetime += Time.deltaTime;

            // 1. Quét tìm quái vật mới mỗi 0.5s để khiêu khích chúng về phía phân thân
            if (Time.time >= nextTauntScanTime)
            {
                nextTauntScanTime = Time.time + 0.5f;
                ScanAndTauntEnemies();
            }

            // 2. Vùng sấm sét xung quanh phân thân giật điện liên tục gây sát thương lên quái đứng ngay đó đến khi tắt sét
            if (Time.time >= nextShockTime)
            {
                nextShockTime = Time.time + shockTickInterval;
                ShockNearbyEnemies();
            }

            // 3. Cập nhật vòng tròn đếm ngược phát nổ: Tràn từ tâm ra chạm đúng mép vòng tròn ngoài (0 -> 100%)
            if (expandingFillTransform != null && duration > 0f)
            {
                float progress = Mathf.Clamp01(lifetime / duration);
                expandingFillTransform.localScale = new Vector3(progress, 1f, progress);
            }

            // Xoay nhẹ vòng tròn sét để tạo hiệu ứng điện quang năng động
            if (shockAuraTransform != null)
            {
                shockAuraTransform.Rotate(0f, 45f * Time.deltaTime, 0f);
            }

            // 4. Tự phát nổ khi hết thời gian tồn tại (vòng tròn bên trong vừa chạm khít vòng tròn lớn ngoài)
            if (lifetime >= duration)
            {
                Detonate();
            }
        }

        private void ShockNearbyEnemies()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, shockRadius);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BaseEnemyBehavior>();
                if (enemy != null && !enemy.IsDead)
                {
                    // Chỉ gây sát thương khi quái thực sự đứng ngay trong quầng điện VFX
                    float horizontalDist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(enemy.transform.position.x, enemy.transform.position.z));
                    if (horizontalDist <= shockRadius)
                    {
                        Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
                        if (pushDir == Vector3.zero) pushDir = Vector3.forward;

                        enemy.TakeDamage(shockDamagePerTick, transform.position, pushDir * 0.1f);
                    }
                }
            }
        }

        private void ScanAndTauntEnemies()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, tauntRadius);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BaseEnemyBehavior>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.SetTarget(transform);
                    tauntedEnemies.Add(enemy);
                }
            }
        }

        public void Detonate()
        {
            if (hasDetonated) return;
            hasDetonated = true;

            // 1. Kích hoạt hiệu ứng nổ Anime VFX - Nâng cao hơn mặt đất 0.9m để không bị chìm + Phóng to vụ nổ
            if (explosionVfxPrefab != null)
            {
                Vector3 explosionPos = transform.position + Vector3.up * 0.9f;
                GameObject explosionObj = Instantiate(explosionVfxPrefab, explosionPos, Quaternion.identity);
                explosionObj.transform.localScale = Vector3.one * 1.6f; // Vụ nổ to hơn và hoành tráng hơn
            }

            // 2. Rung màn hình
            var player = CharacterBehaviour.GetBehaviour();
            if (player != null && player.MainCameraCase != null)
            {
                player.MainCameraCase.Shake(0.08f, 0.08f, 0.35f, 1.2f);
            }

            // 3. Âm thanh nổ
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            // 4. Gây sát thương diện rộng và làm choáng quái vật - CHỈ quái vật đứng ngay trong phạm vi VFX (<= explosionRadius) mới dính sát thương
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<BaseEnemyBehavior>();
                if (enemy != null && !enemy.IsDead)
                {
                    // Tính khoảng cách 2D mặt đất chính xác, loại trừ việc collider rìa bị va chạm oan
                    float horizontalDist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(enemy.transform.position.x, enemy.transform.position.z));
                    if (horizontalDist <= explosionRadius)
                    {
                        Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
                        if (pushDir == Vector3.zero) pushDir = Vector3.forward;

                        enemy.TakeDamage(explosionDamage, transform.position, pushDir);

                        // Làm choáng (Stun)
                        ApplyStun(enemy);
                    }
                }
            }

            // 5. Trả lại mục tiêu là người chơi cho các quái vật còn sống
            RestorePlayerTarget();

            Destroy(gameObject);
        }

        private void ApplyStun(BaseEnemyBehavior enemy)
        {
            if (enemy == null || enemy.IsDead) return;

            if (enemy.NavMeshAgent != null && enemy.NavMeshAgent.enabled)
            {
                float originalSpeed = enemy.RunningSpeed;
                enemy.NavMeshAgent.isStopped = true;
                enemy.NavMeshAgent.speed = 0f;

                Tween.DelayedCall(stunDuration, () =>
                {
                    if (enemy != null && !enemy.IsDead && enemy.NavMeshAgent != null && enemy.NavMeshAgent.enabled)
                    {
                        enemy.NavMeshAgent.speed = originalSpeed;
                        enemy.NavMeshAgent.isStopped = false;
                    }
                });
            }
        }

        private void RestorePlayerTarget()
        {
            var player = CharacterBehaviour.GetBehaviour();
            if (player != null)
            {
                foreach (var enemy in tauntedEnemies)
                {
                    if (enemy != null && !enemy.IsDead)
                    {
                        enemy.SetTarget(player.transform);
                    }
                }
            }
            tauntedEnemies.Clear();
        }

        private void OnDestroy()
        {
            RestorePlayerTarget();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Bán kính khiêu khích màu vàng cam
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, tauntRadius);

            // Bán kính giật sét màu xanh Cyan
            Gizmos.color = new Color(0f, 0.9f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, shockRadius);

            // Bán kính vụ nổ màu đỏ
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);

            UnityEditor.Handles.color = new Color(1f, 0.3f, 0f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, explosionRadius);
        }
#endif
    }
}
