using UnityEngine;
using UnityEngine.AI;
using Watermelon;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class CharacterBehaviour : MonoBehaviour, IEnemyDetector, IHealth, INavMeshAgent
    {
        private static readonly int SHADER_HIT_SHINE_COLOR_HASH = Shader.PropertyToID("_EmissionColor");

        private static CharacterBehaviour characterBehaviour;

        [SerializeField] NavMeshAgent agent;
        [SerializeField] EnemyDetector enemyDetector;

        [Header("Health")]
        [SerializeField] HealthbarBehaviour healthbarBehaviour;
        public HealthbarBehaviour HealthbarBehaviour => healthbarBehaviour;

        [SerializeField] ParticleSystem healingParticle;
        [SerializeField] ParticleSystem godModeParticle;

        [Header("Target")]
        [SerializeField] GameObject targetRingPrefab;
        [SerializeField] Color targetRingActiveColor;
        [SerializeField] Color targetRingDisabledColor;
        [SerializeField] Color targetRingSpecialColor;

        [Space(5)]
        [SerializeField] AimRingBehavior aimRingBehavior;

        // Character Graphics
        private BaseCharacterGraphics graphics;
        public BaseCharacterGraphics Graphics => graphics;

        private GameObject graphicsPrefab;
        private SkinnedMeshRenderer characterMeshRenderer;

        private MaterialPropertyBlock hitShinePropertyBlock;
        private TweenCase hitShineTweenCase;

        private CharacterStats stats;
        public CharacterStats Stats => stats;

        // Gun
        private BaseGunBehavior gunBehaviour;
        public BaseGunBehavior Weapon => gunBehaviour;

        private GameObject gunPrefabGraphics;

        // Health
        private float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.Health;
        public bool FullHealth => currentHealth == stats.Health;

        public bool IsInvulnerable { get; private set; }

        public bool IsActive => isActive;
        private bool isActive;

        public static Transform Transform => characterBehaviour.transform;

        // Movement
        private MovementSettings movementSettings;
        private MovementSettings movementAimingSettings;

        private MovementSettings activeMovementSettings;
        public MovementSettings MovementSettings => activeMovementSettings;
        public float ActualMoveSpeed => activeMovementSettings != null ? activeMovementSettings.MoveSpeed * (1f + EquipmentController.GetTotalBonusStats().bonusMoveSpeed / 100f) : 0f;

        private bool isMoving;
        private float speed = 0;



        // Dash Settings & Variables
        [Header("Dash Settings")]
        [SerializeField] float dashSpeed = 22f;
        [SerializeField] float dashDuration = 0.2f;
        [SerializeField] float dashCooldown = 1.0f;
        [SerializeField] GameObject dashVFXPrefab;
        [SerializeField] Vector3 dashVFXOffset = new Vector3(0f, 0.15f, -0.5f);
        [SerializeField] GameObject dashVFXChildObject;

        private bool isDashing;
        private float dashTimeLeft;
        private float dashCooldownTimeLeft;
        private Vector3 dashDirection;
        public bool IsDashing => isDashing;
        public float DashCooldownTimeLeft => dashCooldownTimeLeft;
        public float DashCooldown => dashCooldown;

        private Vector3 movementVelocity;
        public Vector3 MovementVelocity => movementVelocity;

        public EnemyDetector EnemyDetector => enemyDetector;

        public bool IsCloseEnemyFound => closestEnemyBehaviour != null && !IsLobbyModeActive;
        public bool IsAttackingAllowed { get; private set; } = true;

        private static bool isLobbyModeActive;
        public static bool IsLobbyModeActive
        {
            get => isLobbyModeActive;
            set
            {
                isLobbyModeActive = value;
                var behaviour = GetBehaviour();
                if (behaviour != null)
                {
                    if (behaviour.healthbarBehaviour != null)
                    {
                        if (isLobbyModeActive)
                            behaviour.healthbarBehaviour.DisableBar();
                        else
                            behaviour.healthbarBehaviour.EnableBar();
                    }
                    if (isLobbyModeActive)
                    {
                        behaviour.OnCloseEnemyChanged(null);
                    }
                }
            }
        }

        private BaseEnemyBehavior closestEnemyBehaviour;
        public BaseEnemyBehavior ClosestEnemyBehaviour => closestEnemyBehaviour;

        private Transform playerTarget;
        private GameObject targetRing;
        private Renderer targetRingRenderer;
        private TweenCase ringTweenCase;

        private VirtualCameraCase mainCameraCase;
        public VirtualCameraCase MainCameraCase => mainCameraCase;

        private bool isMovementActive = false;
        public bool IsMovementActive => isMovementActive;

        public static bool NoDamage { get; private set; } = false;

        public static bool IsDead { get; private set; } = false;

        public static SimpleCallback OnDied;

        private void Awake()
        {
            agent.enabled = false;
        }

        public void Initialise()
        {
            characterBehaviour = this;

            hitShinePropertyBlock = new MaterialPropertyBlock();

            isActive = false;
            enabled = false;

            // Create target
            GameObject tempTarget = new GameObject("[TARGET]");
            tempTarget.transform.position = transform.position;
            tempTarget.SetActive(true);

            playerTarget = tempTarget.transform;

            // Get camera case
            mainCameraCase = CameraController.GetCamera(CameraType.Main);

            // Initialise enemy detector
            enemyDetector.Initialise(this);

            // Set health
            currentHealth = MaxHealth;

            // Initialise healthbar
            healthbarBehaviour.Initialise(transform, this, true, CharactersController.SelectedCharacter.GetCurrentStage().HealthBarOffset);

            aimRingBehavior.Init(transform);

            targetRing = Instantiate(targetRingPrefab, new Vector3(0f, 0f, -999f), Quaternion.identity);
            targetRingRenderer = targetRing.GetComponent<Renderer>();

            aimRingBehavior.Hide();

            IsDead = false;

            IsAttackingAllowed = !GameController.Settings.UseAttackButton;
            if (GameController.Settings.UseAttackButton)
            {
                AttackButtonBehavior.onStatusChanged += OnAttackButtonStatusChanged;
            }
        }

        private void OnAttackButtonStatusChanged(bool isPressed)
        {
            IsAttackingAllowed = isPressed;
        }

        public void Reload(bool resetHealth = true)
        {
            isActive = false;

            // Set health
            if (resetHealth)
            {
                currentHealth = MaxHealth;
            }

            IsDead = false;

            healthbarBehaviour.EnableBar(true);
            healthbarBehaviour.RedrawHealth();

            enemyDetector.Reload();

            enemyDetector.gameObject.SetActive(false);

            graphics.DisableRagdoll();
            graphics.Reload();

            gunBehaviour.Reload();

            gameObject.SetActive(true);
        }

        public void ResetDetector()
        {
            var radius = enemyDetector.DetectorRadius;
            enemyDetector.SetRadius(0);
            Tween.NextFrame(() => enemyDetector.SetRadius(radius), framesOffset: 2, updateMethod: UpdateMethod.FixedUpdate);
        }

        public void Unload()
        {
            if (graphics != null)
                graphics.Unload();

            if (playerTarget != null)
                Destroy(playerTarget.gameObject);

            if (aimRingBehavior != null)
                Destroy(aimRingBehavior.gameObject);

            if (healthbarBehaviour != null)
                healthbarBehaviour.Destroy();
        }

        public void OnLevelLoaded()
        {
            if (gunBehaviour != null)
                gunBehaviour.OnLevelLoaded();
        }

        public void OnNavMeshUpdated()
        {
            if (agent.isOnNavMesh)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
        }

        public void ActivateAgent()
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        public static void DisableNavmeshAgent()
        {
            characterBehaviour.agent.enabled = false;
        }

        public void MakeInvulnerable(float duration)
        {
            IsInvulnerable = true;

            godModeParticle.Play();

            Tween.DelayedCall(duration, () => {
                IsInvulnerable = false;

                godModeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            });
        }

        public virtual void TakeDamage(float damage)
        {
            if (currentHealth <= 0 || IsInvulnerable || isDashing)
                return;

            // Áp dụng giảm sát thương từ giáp của trang bị
            float armorPercent = EquipmentController.GetTotalBonusStats().bonusArmor;
            armorPercent = Mathf.Clamp(armorPercent, 0f, 75f); // Giới hạn tối đa 75% giảm sát thương
            damage = damage * (1f - armorPercent / 100f);

            currentHealth = Mathf.Clamp(currentHealth - damage, 0, MaxHealth);

            healthbarBehaviour.OnHealthChanged();

            mainCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            if (currentHealth <= 0)
            {
                healthbarBehaviour.DisableBar();
                OnCloseEnemyChanged(null);

                isActive = false;
                enabled = false;

                enemyDetector.gameObject.SetActive(false);
                aimRingBehavior.Hide();

                OnDeath();

                graphics.EnableRagdoll();

                OnDied?.Invoke();

                Vibration.Vibrate(VibrationIntensity.Medium);
            }

            HitEffect();

            AudioController.PlaySound(AudioController.Sounds.characterHit.GetRandomItem());

            Vibration.Vibrate(VibrationIntensity.Light);

            FloatingTextController.SpawnFloatingText("PlayerHit", "-" + damage.ToString("F0"), transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 3.75f, Random.Range(-0.1f, 0.1f)), Quaternion.identity, 1f);
        }

        [Button]
        public void OnDeath()
        {
            graphics.OnDeath();

            IsDead = true;

            Tween.DelayedCall(0.5f, LevelController.OnPlayerDied);
        }

        public void SetPosition(Vector3 position)
        {
            playerTarget.position = position.AddToZ(10f);
            transform.position = position;
            transform.rotation = Quaternion.identity;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Warp(position);
            }
        }

        protected void HitEffect()
        {
            hitShineTweenCase.KillActive();

            characterMeshRenderer.GetPropertyBlock(hitShinePropertyBlock);
            hitShinePropertyBlock.SetColor(SHADER_HIT_SHINE_COLOR_HASH, Color.white);
            characterMeshRenderer.SetPropertyBlock(hitShinePropertyBlock);

            hitShineTweenCase = characterMeshRenderer.DOPropertyBlockColor(SHADER_HIT_SHINE_COLOR_HASH, hitShinePropertyBlock, Color.black, 0.32f);

            graphics.PlayHitAnimation();
        }

        #region Gun
        public void SetGun(WeaponData weaponData, bool playBounceAnimation = false, bool playAnimation = false, bool playParticle = false)
        {
            var gunUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);
            var currentStage = gunUpgrade.GetCurrentStage();

            // Check if graphics isn't exist already
            if (gunPrefabGraphics != currentStage.WeaponPrefab)
            {
                // Store prefab link
                gunPrefabGraphics = currentStage.WeaponPrefab;

                if (gunBehaviour != null)
                {
                    gunBehaviour.OnGunUnloaded();

                    Destroy(gunBehaviour.gameObject);
                }

                if (gunPrefabGraphics != null)
                {
                    GameObject gunObject = Instantiate(gunPrefabGraphics);
                    gunObject.SetActive(true);

                    gunBehaviour = gunObject.GetComponent<BaseGunBehavior>();

                    if (graphics != null)
                    {
                        gunBehaviour.InitialiseCharacter(graphics);
                        gunBehaviour.PlaceGun(graphics);

                        graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());

                        gunBehaviour.UpdateHandRig();
                    }
                }
            }

            if (gunBehaviour != null)
            {
                gunBehaviour.Initialise(this, weaponData);

                Vector3 defaultScale = gunBehaviour.transform.localScale;

                if (playAnimation)
                {
                    gunBehaviour.transform.localScale = defaultScale * 0.8f;
                    gunBehaviour.transform.DOScale(defaultScale, 0.15f).SetEasing(Ease.Type.BackOut);
                }

                if (playBounceAnimation)
                    gunBehaviour.PlayBounceAnimation();

                if (playParticle)
                    gunBehaviour.PlayUpgradeParticle();
            }

            enemyDetector.SetRadius(currentStage.RangeRadius);
            aimRingBehavior.SetRadius(currentStage.RangeRadius);
        }

        public void OnGunShooted()
        {
            graphics.OnShoot();
        }
        #endregion

        #region Graphics
        public void SetStats(CharacterStats stats)
        {
            this.stats = stats;

            currentHealth = stats.Health;

            if (healthbarBehaviour != null)
                healthbarBehaviour.OnHealthChanged();
        }

        public void SetGraphics(GameObject newGraphicsPrefab, bool playParticle, bool playAnimation)
        {
            // Check if graphics isn't exist already
            if (graphicsPrefab != newGraphicsPrefab)
            {
                // Store prefab link
                graphicsPrefab = newGraphicsPrefab;

                if (graphics != null)
                {
                    if (gunBehaviour != null)
                        gunBehaviour.transform.SetParent(null);

                    graphics.Unload();

                    Destroy(graphics.gameObject);
                }

                GameObject graphicObject = Instantiate(newGraphicsPrefab);
                graphicObject.transform.SetParent(transform);
                graphicObject.transform.ResetLocal();
                graphicObject.SetActive(true);

                graphics = graphicObject.GetComponent<BaseCharacterGraphics>();
                graphics.Initialise(this);

                movementSettings = graphics.MovementSettings;
                movementAimingSettings = graphics.MovementAimingSettings;

                activeMovementSettings = movementSettings;

                characterMeshRenderer = graphics.MeshRenderer;

                if (gunBehaviour != null)
                {
                    gunBehaviour.InitialiseCharacter(graphics);
                    gunBehaviour.PlaceGun(graphics);

                    graphics.SetShootingAnimation(gunBehaviour.GetShootAnimationClip());

                    gunBehaviour.UpdateHandRig();

                    Jump();
                }
                else
                {
                    Tween.NextFrame(Jump, 0, false, UpdateMethod.LateUpdate);
                }

                if (playParticle)
                    graphics.PlayUpgradeParticle();

                if (playAnimation)
                    graphics.PlayBounceAnimation();
            }
        }
        #endregion

        public void Activate(bool check = true)
        {
            if (check && isActive)
                return;

            isActive = true;
            enabled = true;

            enemyDetector.gameObject.SetActive(true);

            aimRingBehavior.Show();

            graphics.Activate();

            NavMeshController.InvokeOrSubscribe(this);
        }

        public void Disable()
        {
            if (!isActive)
                return;

            isActive = false;
            enabled = false;

            agent.enabled = false;

            aimRingBehavior.Hide();

            targetRing.SetActive(false);
            targetRing.transform.SetParent(null);

            graphics.Disable();

            closestEnemyBehaviour = null;

            if (isMoving)
            {
                isMoving = false;

                speed = 0;
            }
        }

        public void MoveForwardAndDisable(float duration)
        {
            agent.enabled = false;

            transform.DOMove(transform.position + Vector3.forward * ActualMoveSpeed * duration, duration).OnComplete(() =>
            {
                Disable();
            });
        }

        public void DisableAgent()
        {
            agent.enabled = false;
        }

        public void ActivateMovement()
        {
            isMovementActive = true;

            aimRingBehavior.Show();
        }

        private void Update()
        {
            if (gunBehaviour != null)
                gunBehaviour.UpdateHandRig();

#if UNITY_EDITOR
            // Cheat test nhanh: Nhấn phím I để tự động mở khóa và trang bị Kiếm (Sword)
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
            {
                var swordData = WeaponsController.Database.GetWeapon(WeaponType.Sword);
                if (swordData != null)
                {
                    var upgrade = Watermelon.UpgradesController.GetUpgrade<Watermelon.Upgrades.BaseUpgrade>(swordData.UpgradeType);
                    if (upgrade != null && upgrade.UpgradeLevel == 0)
                    {
                        upgrade.UpgradeStage();
                    }
                    WeaponsController.SelectWeapon(WeaponType.Sword);
                    Debug.Log("[Cheat] Da mo khoa va trang bi Kiem (Sword) thanh cong!");
                }
            }

            // Keyboard Dash Test for PC Editor
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.leftShiftKey.wasPressedThisFrame)
                {
                    PerformDash();
                }
            }
#endif

            if (dashCooldownTimeLeft > 0)
            {
                dashCooldownTimeLeft -= Time.deltaTime;
            }



            if (!isActive)
                return;

            var joystick = Control.CurrentControl;

            if (isDashing)
            {
                dashTimeLeft -= Time.deltaTime;
                if (dashTimeLeft <= 0)
                {
                    isDashing = false;
                    isMoving = false;
                    graphics.OnMovingStoped();
                }
                else
                {
                    transform.position += dashDirection * dashSpeed * Time.deltaTime;

                    if (!isMoving)
                    {
                        isMoving = true;
                        graphics.OnMovingStarted();
                    }

                    Vector2 animDir = new Vector2(dashDirection.x, dashDirection.z).normalized;
                    graphics.OnMoving(1.0f, animDir, IsCloseEnemyFound);

                    transform.rotation = Quaternion.LookRotation(dashDirection);
                }
            }
            else if (joystick.IsMovementInputNonZero && joystick.MovementInput.sqrMagnitude > 0.1f)
            {
                if (!isMoving)
                {
                    isMoving = true;

                    speed = 0;

                    graphics.OnMovingStarted();
                }

                float maxAlowedSpeed = Mathf.Clamp01(joystick.MovementInput.magnitude) * ActualMoveSpeed;

                if (speed > maxAlowedSpeed)
                {
                    speed -= activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed < maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }
                else
                {
                    speed += activeMovementSettings.Acceleration * Time.deltaTime;
                    if (speed > maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }

                movementVelocity = transform.forward * speed;

                transform.position += joystick.MovementInput * Time.deltaTime * speed;

                graphics.OnMoving(Mathf.InverseLerp(0, ActualMoveSpeed, speed), joystick.MovementInput, IsCloseEnemyFound);

                if (!IsCloseEnemyFound)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(joystick.MovementInput.normalized), Time.deltaTime * activeMovementSettings.RotationSpeed);
                }
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;

                    movementVelocity = Vector3.zero;

                    graphics.OnMovingStoped();

                    speed = 0;
                }
            }

            if (IsCloseEnemyFound)
            {
                playerTarget.position = Vector3.Lerp(playerTarget.position, new Vector3(closestEnemyBehaviour.transform.position.x, transform.position.y, closestEnemyBehaviour.transform.position.z), Time.deltaTime * activeMovementSettings.RotationSpeed);

                transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
            }

            targetRing.transform.rotation = Quaternion.identity;

            if (healthbarBehaviour != null)
                healthbarBehaviour.FollowUpdate();

            aimRingBehavior.UpdatePosition();
        }

        private void FixedUpdate()
        {
            graphics.CustomFixedUpdate();

            if (gunBehaviour != null && !IsLobbyModeActive)
                gunBehaviour.GunUpdate();
        }

        public void OnCloseEnemyChanged(BaseEnemyBehavior enemyBehavior)
        {
            if (!isActive) return;
            if (IsLobbyModeActive) return;

            if (enemyBehavior != null)
            {
                if (closestEnemyBehaviour == null)
                {
                    playerTarget.position = transform.position + transform.forward * 5;
                }

                activeMovementSettings = movementAimingSettings;

                closestEnemyBehaviour = enemyBehavior;

                targetRing.SetActive(true);
                targetRing.transform.rotation = Quaternion.identity;

                ringTweenCase.KillActive();

                targetRing.transform.SetParent(enemyBehavior.transform);
                targetRing.transform.localScale = Vector3.one * enemyBehavior.Stats.TargetRingSize * 1.4f;
                targetRing.transform.localPosition = Vector3.zero;

                ringTweenCase = targetRing.transform.DOScale(Vector3.one * enemyBehavior.Stats.TargetRingSize, 0.2f).SetEasing(Ease.Type.BackIn);

                CameraController.SetEnemyTarget(enemyBehavior);

                SetTargetActive();

                return;
            }

            activeMovementSettings = movementSettings;

            closestEnemyBehaviour = null;
            targetRing.SetActive(false);
            targetRing.transform.SetParent(null);

            CameraController.SetEnemyTarget(null);
        }

        public static BaseEnemyBehavior GetClosestEnemy()
        {
            return characterBehaviour.enemyDetector.ClosestEnemy;
        }

        public static CharacterBehaviour GetBehaviour()
        {
            return characterBehaviour;
        }

        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            EnemyDetector.TryAddClosestEnemy(enemy);
        }

        public void SetTargetActive()
        {
            if (closestEnemyBehaviour != null && closestEnemyBehaviour.Tier == EnemyTier.Elite)
            {
                targetRingRenderer.material.color = targetRingSpecialColor;
            }
            else
            {
                targetRingRenderer.material.color = targetRingActiveColor;
            }
        }

        public void SetTargetUnreachable()
        {
            targetRingRenderer.material.color = targetRingDisabledColor;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                IDropableItem item = other.GetComponent<IDropableItem>();
                if (item.IsPickable(this) && !item.IsPicked)
                {
                    OnItemPicked(item);
                    item.Pick();
                }
            }
            else if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                other.GetComponent<AbstractChestBehavior>().ChestApproached();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_ITEM))
            {
                IDropableItem item = other.GetComponent<IDropableItem>();
                if (item.IsPickable(this) && !item.IsPicked)
                {
                    OnItemPicked(item);
                    item.Pick();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_CHEST))
            {
                other.GetComponent<AbstractChestBehavior>().ChestLeft();
            }
        }

        public void OnItemPicked(IDropableItem item)
        {
            if (item.DropType == DropableItemType.Currency)
            {
                if (item.DropData.currencyType == CurrencyType.Coins)
                {
                    if (item.IsRewarded)
                    {
                        LevelController.OnRewardedCoinPicked(item.DropAmount);
                    }
                    else
                    {
                        LevelController.OnCoinPicked(item.DropAmount);
                    }
                }
                else
                {
                    CurrenciesController.Add(item.DropData.currencyType, item.DropAmount);
                }
            }
            else if (item.DropType == DropableItemType.Heal)
            {
                currentHealth = Mathf.Clamp(currentHealth + item.DropAmount, 0, MaxHealth);
                healthbarBehaviour.OnHealthChanged();
                healingParticle.Play();
            }
        }

        [Button]
        public void Jump()
        {
            graphics.Jump();
            gunBehaviour.transform.localScale = Vector3.zero;
            gunBehaviour.gameObject.SetActive(false);
        }

        public void SpawnWeapon()
        {
            if (gunBehaviour.NeedsRig)
                graphics.EnableRig();
            else
                graphics.DisableRig();

            gunBehaviour.gameObject.SetActive(true);
            gunBehaviour.DOScale(1, 0.2f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
        }



        public void PerformDash()
        {
            if (isDashing || dashCooldownTimeLeft > 0 || !isActive) return;

            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownTimeLeft = dashCooldown;

            var joystick = Control.CurrentControl;
            if (joystick != null && joystick.IsMovementInputNonZero)
            {
                dashDirection = new Vector3(joystick.MovementInput.x, 0, joystick.MovementInput.y).normalized;
            }
            else
            {
                dashDirection = transform.forward;
            }

            // 1. Kích hoạt VFX con gắn trực tiếp trên nhân vật nếu được cấu hình
            if (dashVFXChildObject != null)
            {
                try
                {
                    dashVFXChildObject.SetActive(true);
                    var particles = dashVFXChildObject.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var ps in particles)
                    {
                        ps.Play(true);
                    }
                    
                    // Tắt đi sau khi lướt xong (theo thời gian dashDuration)
                    Tween.DelayedCall(dashDuration, () =>
                    {
                        if (dashVFXChildObject != null)
                            dashVFXChildObject.SetActive(false);
                    });
                    Debug.Log("[CharacterBehaviour] Da kich hoat VFX con: " + dashVFXChildObject.name);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[CharacterBehaviour] Loi kich hoat VFX con: " + e.Message);
                }
            }
            // 2. Kích hoạt VFX prefab (Instantiate) nếu không gán VFX con
            else if (dashVFXPrefab != null)
            {
                try
                {
                    Vector3 spawnPos = transform.position + transform.rotation * dashVFXOffset;
                    Quaternion spawnRot = Quaternion.LookRotation(-transform.forward); // Quay mặt về phía sau
                    GameObject vfxInstance = Instantiate(dashVFXPrefab, spawnPos, spawnRot);
                    
                    if (vfxInstance != null)
                    {
                        vfxInstance.SetActive(true);
                        // Bắt buộc chạy tất cả hệ thống hạt bên trong VFX
                        var particles = vfxInstance.GetComponentsInChildren<ParticleSystem>(true);
                        foreach (var ps in particles)
                        {
                            ps.Play(true);
                        }
                        
                        Destroy(vfxInstance, 2.0f); // Tự động giải phóng bộ nhớ sau 2 giây
                    }
                    Debug.Log("[CharacterBehaviour] Da khoi tao va chay VFX luyen tai vi tri: " + spawnPos);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[CharacterBehaviour] Loi khoi tao VFX lướt: " + e.Message);
                }
            }
            else
            {
                // Fallback nổ năng lượng tại vị trí nhân vật
                try
                {
                    int upgradeParticleHash = ParticlesController.GetHash("Upgrade");
                    ParticlesController.PlayParticle(upgradeParticleHash).SetPosition(transform.position + new Vector3(0, 0.5f, 0));
                }
                catch
                {
                }
            }

            var trail = GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.emitting = true;
                Tween.DelayedCall(dashDuration, () => trail.emitting = false);
            }

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        private void OnDestroy()
        {
            if (healthbarBehaviour.HealthBarTransform != null)
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);

            if (aimRingBehavior != null)
                aimRingBehavior.OnPlayerDestroyed();

            AttackButtonBehavior.onStatusChanged -= OnAttackButtonStatusChanged;
        }
    }
}