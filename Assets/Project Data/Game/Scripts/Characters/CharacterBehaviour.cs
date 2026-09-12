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
        public bool FullHealth => currentHealth == MaxHealth;

        public static event System.Action<float, float> OnPlayerHealthChanged;
        public static event System.Action<CharacterBehaviour> OnPlayerSpawned;

        public void NotifyHealthChanged()
        {
            OnPlayerHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

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

        // Active Skill Variables
        private float skillCooldownTimeLeft;
        public float SkillCooldownTimeLeft => skillCooldownTimeLeft;
        public float SkillCooldown => CharactersController.SelectedCharacter != null && CharactersController.SelectedCharacter.SkillData != null ? CharactersController.SelectedCharacter.SkillData.Cooldown : 1f;
        public bool IsSkillReady => skillCooldownTimeLeft <= 0;

        private Vector3 movementVelocity;
        public Vector3 MovementVelocity => movementVelocity;

        public EnemyDetector EnemyDetector => enemyDetector;

        public bool IsCloseEnemyFound => closestEnemyBehaviour != null && !IsLobbyModeActive;
        private bool isAttackingAllowedInternal = true;
        public bool IsAttackingAllowed
        {
            get
            {
                if (IsLobbyModeActive) return false;
                return isAttackingAllowedInternal || IsAutoShootActive;
            }
            private set => isAttackingAllowedInternal = value;
        }

        public static bool IsAutoShootActive
        {
            get => PlayerPrefs.GetInt("AutoShootSetting", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("AutoShootSetting", value ? 1 : 0);
                PlayerPrefs.Save();
                
                // Cập nhật hiển thị Attack Button ở giao diện UIGame
                var uiGame = UIController.GetPage<UIGame>();
                if (uiGame != null)
                {
                    uiGame.UpdateAttackButtonVisibility();
                }
            }
        }

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
                    if (behaviour.healthbarBehaviour != null && behaviour.healthbarBehaviour.HealthBarTransform != null)
                    {
                        behaviour.healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);
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

        private DroneBehavior currentDrone;
        public DroneBehavior CurrentDrone => currentDrone;

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

        public void UpdateDrone()
        {
            if (currentDrone != null)
            {
                Destroy(currentDrone.gameObject);
                currentDrone = null;
            }

            currentDrone = DronesController.SpawnDrone(this);
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
            if (healthbarBehaviour.HealthBarTransform != null)
            {
                healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);
            }

            OnPlayerSpawned?.Invoke(this);
            NotifyHealthChanged();

            aimRingBehavior.Init(transform);

            if (targetRing == null)
            {
                targetRing = Instantiate(targetRingPrefab, new Vector3(0f, 0f, -999f), Quaternion.identity);
                targetRingRenderer = targetRing.GetComponent<Renderer>();
            }

            aimRingBehavior.Hide();

            IsDead = false;

            // Luôn đăng ký lắng nghe sự kiện từ nút bắn thủ công
            AttackButtonBehavior.onStatusChanged += OnAttackButtonStatusChanged;
            isAttackingAllowedInternal = false;
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
            if (healthbarBehaviour.HealthBarTransform != null)
            {
                healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);
            }
            NotifyHealthChanged();

            // Drone is now a child of the player, so it doesn't need to be destroyed on reload

            enemyDetector.Reload();

            enemyDetector.gameObject.SetActive(false);

            graphics.DisableRagdoll();
            graphics.Reload();

            if (gunBehaviour != null)
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
            if (agent != null)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 3.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    agent.enabled = true;
                    agent.isStopped = false;
                }
                else
                {
                    agent.enabled = true;
                    if (agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                    }
                }
            }
        }

        public void ActivateAgent()
        {
            if (agent != null)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 3.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                }
                agent.enabled = true;
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
            }
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
            if (healthbarBehaviour.HealthBarTransform != null)
            {
                healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);
            }
            NotifyHealthChanged();

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

            if (agent != null && agent.isActiveAndEnabled)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(position, out UnityEngine.AI.NavMeshHit hit, 3.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
                else
                {
                    agent.enabled = false;
                }
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

        private UnityEngine.AddressableAssets.AssetReferenceGameObject gunPrefabReference;

        #region Gun
        public void SetGun(WeaponData weaponData, bool playBounceAnimation = false, bool playAnimation = false, bool playParticle = false)
        {
            var gunUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);
            var currentStage = gunUpgrade.GetCurrentStage();

            // Check if graphics isn't exist already
            if (gunPrefabReference == null || gunPrefabReference.AssetGUID != currentStage.WeaponPrefab.AssetGUID)
            {
                // Store prefab link
                gunPrefabReference = currentStage.WeaponPrefab;

                if (gunBehaviour != null)
                {
                    gunBehaviour.OnGunUnloaded();
                    Destroy(gunBehaviour.gameObject);
                }

                if (gunPrefabReference != null && gunPrefabReference.RuntimeKeyIsValid())
                {
                    GameObject gunObject = gunPrefabReference.InstantiateAsync().WaitForCompletion();
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

            // Drone spawning has been moved to UpdateDrone()

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
            {
                healthbarBehaviour.OnHealthChanged();
                if (healthbarBehaviour.HealthBarTransform != null)
                {
                    healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);
                }
            }
            NotifyHealthChanged();
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

                // Them vien sang nhe (hoi trang) cho nhan vat va vu khi
                var outline = graphicObject.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = graphicObject.AddComponent<Outline>();
                }
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineColor = new Color(0.9f, 0.92f, 0.95f, 1f);
                outline.OutlineWidth = 1.9f;
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

            if (closestEnemyBehaviour != null)
            {
                closestEnemyBehaviour.SetTargeted(false);
            }
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
                    if (Control.IsMovementControlActive)
                    {
                        PerformDash();
                    }
                }
            }
#endif

            if (dashCooldownTimeLeft > 0)
            {
                dashCooldownTimeLeft -= Time.deltaTime;
            }

            if (skillCooldownTimeLeft > 0)
            {
                skillCooldownTimeLeft -= Time.deltaTime;
            }



            if (!isActive)
                return;

            var joystick = Control.CurrentControl;

            Vector3 movementInput = Vector3.zero;
            if (joystick != null && joystick.IsMovementInputNonZero && joystick.MovementInput.sqrMagnitude > 0.05f)
            {
                movementInput = joystick.MovementInput;
            }
#if UNITY_EDITOR
            else if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                Vector3 keyInput = Vector3.zero;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) keyInput.z += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) keyInput.z -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) keyInput.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) keyInput.x += 1f;

                if (keyInput.sqrMagnitude > 0.01f)
                {
                    movementInput = keyInput.normalized;
                }
            }
#endif

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
            else if (movementInput.sqrMagnitude > 0.05f)
            {
                if (!isMoving)
                {
                    isMoving = true;

                    speed = 0;

                    graphics.OnMovingStarted();
                }

                float maxAlowedSpeed = Mathf.Clamp01(movementInput.magnitude) * ActualMoveSpeed;

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

                transform.position += movementInput * Time.deltaTime * speed;

                graphics.OnMoving(Mathf.InverseLerp(0, ActualMoveSpeed, speed), movementInput, IsCloseEnemyFound);

                if (!IsCloseEnemyFound)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movementInput.normalized), Time.deltaTime * activeMovementSettings.RotationSpeed);
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

            if (targetRing != null)
            {
                if (IsCloseEnemyFound)
                {
                    targetRing.transform.position = closestEnemyBehaviour.transform.position;
                }
                targetRing.transform.rotation = Quaternion.identity;
            }

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
            if (IsLobbyModeActive && enemyBehavior != null) return;

            if (enemyBehavior != null)
            {
                if (closestEnemyBehaviour == null)
                {
                    playerTarget.position = transform.position + transform.forward * 5;
                }

                if (closestEnemyBehaviour != null && closestEnemyBehaviour != enemyBehavior)
                {
                    closestEnemyBehaviour.SetTargeted(false);
                }

                activeMovementSettings = movementAimingSettings;

                closestEnemyBehaviour = enemyBehavior;
                closestEnemyBehaviour.SetTargeted(true);

                targetRing.SetActive(true);
                targetRing.transform.rotation = Quaternion.identity;

                ringTweenCase.KillActive();

                targetRing.transform.localScale = Vector3.one * enemyBehavior.Stats.TargetRingSize * 1.4f;

                ringTweenCase = targetRing.transform.DOScale(Vector3.one * enemyBehavior.Stats.TargetRingSize, 0.2f).SetEasing(Ease.Type.BackIn);

                CameraController.SetEnemyTarget(enemyBehavior);

                SetTargetActive();

                return;
            }

            if (closestEnemyBehaviour != null)
            {
                closestEnemyBehaviour.SetTargeted(false);
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
            if (closestEnemyBehaviour != null)
            {
                closestEnemyBehaviour.SetTargeted(true);
            }

            if (targetRingRenderer != null && targetRingRenderer.material != null)
            {
                Color ringColor = (closestEnemyBehaviour != null && closestEnemyBehaviour.Tier == EnemyTier.Elite)
                    ? targetRingSpecialColor
                    : targetRingActiveColor;

                targetRingRenderer.material.color = ringColor;
                if (targetRingRenderer.material.HasProperty("_Color"))
                    targetRingRenderer.material.SetColor("_Color", ringColor);
                if (targetRingRenderer.material.HasProperty("_BaseColor"))
                    targetRingRenderer.material.SetColor("_BaseColor", ringColor);
            }
        }

        public void SetTargetUnreachable()
        {
            if (targetRingRenderer != null && targetRingRenderer.material != null)
            {
                targetRingRenderer.material.color = targetRingDisabledColor;
                if (targetRingRenderer.material.HasProperty("_Color"))
                    targetRingRenderer.material.SetColor("_Color", targetRingDisabledColor);
                if (targetRingRenderer.material.HasProperty("_BaseColor"))
                    targetRingRenderer.material.SetColor("_BaseColor", targetRingDisabledColor);
            }
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
                if (healthbarBehaviour.HealthBarTransform != null)
                {
                    healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);
                }
                NotifyHealthChanged();
                healingParticle.Play();
            }
        }

        [Button]
        public void Jump()
        {
            graphics.Jump();
            if (gunBehaviour != null)
            {
                gunBehaviour.transform.localScale = Vector3.zero;
                gunBehaviour.gameObject.SetActive(false);
            }
        }

        public void SpawnWeapon()
        {
            if (gunBehaviour != null)
            {
                if (gunBehaviour.NeedsRig)
                    graphics.EnableRig();
                else
                    graphics.DisableRig();

                gunBehaviour.gameObject.SetActive(true);
                gunBehaviour.DOScale(1, 0.2f).SetCustomEasing(Ease.GetCustomEasingFunction("BackOutLight"));
            }
        }



        public void PerformDash()
        {
            if (isDashing || dashCooldownTimeLeft > 0 || !isActive || !Control.IsMovementControlActive) return;

            isDashing = true;
            dashTimeLeft = dashDuration;
            dashCooldownTimeLeft = dashCooldown;

            var joystick = Control.CurrentControl;
            if (joystick != null && joystick.IsMovementInputNonZero)
            {
                dashDirection = new Vector3(joystick.MovementInput.x, 0, joystick.MovementInput.y).normalized;
            }
#if UNITY_EDITOR
            else if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                Vector3 keyInput = Vector3.zero;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) keyInput.z += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) keyInput.z -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) keyInput.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) keyInput.x += 1f;
                if (keyInput.sqrMagnitude > 0.01f) dashDirection = keyInput.normalized;
                else dashDirection = transform.forward;
            }
#endif
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

        public void ActivateSkill()
        {
            var character = CharactersController.SelectedCharacter;
            if (character == null || character.SkillData == null || character.SkillData.VFXPrefab == null || !IsSkillReady) return;

            var skill = character.SkillData;
            skillCooldownTimeLeft = skill.Cooldown;

            // Vị trí kích hoạt mặc định tại chân nhân vật
            Vector3 spawnPos = transform.position;

            // Tính toán sát thương theo cấp độ và vũ khí của nhân vật
            int upgradeLevel = character.Save != null ? character.Save.UpgradeLevel : 0;
            float charLevel = upgradeLevel + 1;

            float baseDmg = 100f;
            var activeWeapon = WeaponsController.Database.Weapons[WeaponsController.SelectedWeaponIndex];
            if (activeWeapon != null)
            {
                var stage = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(activeWeapon.UpgradeType).GetCurrentStage();
                if (stage != null)
                {
                    baseDmg = (stage.Damage.firstValue + stage.Damage.secondValue) / 2f;
                }
            }

            float charDmgMult = Stats.BaseBulletDamageMultiplier;
            float finalBaseDmg = baseDmg * charDmgMult;
            float scaledDmg = finalBaseDmg * skill.DamageMultiplier * (1f + (charLevel - 1) * 0.15f);

            switch (skill.SkillType)
            {
                case SkillType.BlackHole:
                    {
                        GameObject vfxObj = Instantiate(skill.VFXPrefab, spawnPos, Quaternion.identity);
                        var blackHole = vfxObj.GetComponent<BlackHoleBehaviour>() ?? vfxObj.AddComponent<BlackHoleBehaviour>();
                        blackHole.Initialise(skill.AoeRadius, skill.PullSpeed, scaledDmg, skill.TickInterval, skill.Duration);
                    }
                    break;

                case SkillType.OrbitalLaser:
                    {
                        // Định vị tại quái vật gần nhất nếu có, hoặc đặt tại chân người chơi
                        Vector3 targetPos = spawnPos;
                        if (closestEnemyBehaviour != null && !closestEnemyBehaviour.IsDead)
                        {
                            targetPos = closestEnemyBehaviour.transform.position;
                        }

                        GameObject vfxObj = Instantiate(skill.VFXPrefab, targetPos, Quaternion.identity);
                        var orbitalLaser = vfxObj.GetComponent<OrbitalLaserBehaviour>() ?? vfxObj.AddComponent<OrbitalLaserBehaviour>();
                        float burstDmg = scaledDmg * 1.5f;
                        float burnTickDmg = scaledDmg * 0.35f;
                        float actualRadius = Mathf.Min(skill.AoeRadius, 1.5f);
                        orbitalLaser.Initialise(actualRadius, burstDmg, burnTickDmg, 0.35f, skill.Duration, 0.5f);
                    }
                    break;

                case SkillType.ShadowClone:
                    {
                        // 1. Khởi tạo Phân thân Hologram tại vị trí NinNin đang đứng
                        GameObject vfxObj = Instantiate(skill.VFXPrefab, spawnPos, transform.rotation);
                        var shadowClone = vfxObj.GetComponent<ShadowCloneBehaviour>() ?? vfxObj.AddComponent<ShadowCloneBehaviour>();
                        float explosionDmg = scaledDmg * 2.5f;
                        float shockTickDmg = scaledDmg * 0.25f; // Sát thương giật sét liên tục
                        float explosionRadius = 3.2f; // Khớp chuẩn 100% với bán kính hình ảnh VFX
                        shadowClone.Initialise(12f, explosionRadius, explosionDmg, skill.Duration, 1.5f, shadowClone.ExplosionVfxPrefab, shockTickDmg);

                        // Sao chép chính xác dáng đứng (pose) của nhân vật NinNin tại khoảnh khắc sài skill
                        if (graphics != null)
                        {
                            shadowClone.CopyPoseFrom(graphics.gameObject);
                        }

                        // 2. NinNin lướt lùi về phía sau theo hướng ngược lại với hướng mặt nhìn (Backstep Dash)
                        Vector3 dashDir = -transform.forward;
                        dashDirection = dashDir;
                        isDashing = true;
                        dashTimeLeft = 0.22f;

                        // Kích hoạt vệt mờ Dash Trail
                        var trail = GetComponentInChildren<TrailRenderer>();
                        if (trail != null)
                        {
                            trail.emitting = true;
                            Tween.DelayedCall(0.22f, () => trail.emitting = false);
                        }

                        // 3. Tàng hình / Miễn nhiễm sát thương trong 1.2s
                        IsInvulnerable = true;
                        Tween.DelayedCall(1.2f, () => IsInvulnerable = false);
                    }
                    break;

                default:
                    {
                        GameObject vfxObj = Instantiate(skill.VFXPrefab, spawnPos, Quaternion.identity);
                        var defaultHole = vfxObj.GetComponent<BlackHoleBehaviour>() ?? vfxObj.AddComponent<BlackHoleBehaviour>();
                        defaultHole.Initialise(skill.AoeRadius, skill.PullSpeed, scaledDmg, skill.TickInterval, skill.Duration);
                    }
                    break;
            }

            // Play sound
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
