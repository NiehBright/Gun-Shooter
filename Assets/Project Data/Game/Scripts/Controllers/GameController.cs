using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        [Header("Refferences")]
        [SerializeField] UIController uiController;

        [Space]
        [DrawReference]
        [SerializeField] GameSettings settings;

        private CurrenciesController currenciesController;
        private UpgradesController upgradesController;
        private ParticlesController particlesController;
        private FloatingTextController floatingTextController;
        private ExperienceController experienceController;
        private WeaponsController weaponsController;
        private CharactersController charactersController;
        private BalanceController balanceController;
        private EnemyController enemyController;
        private TutorialController tutorialController;
        private DronesController dronesController;
        private GachaController gachaController;

        public static GameSettings Settings => instance.settings;
        public GameSettings GameSettings => settings;

        private static bool isGameActive;
        public static bool IsGameActive => isGameActive;

        private void Awake()
        {
            instance = this;

            SaveController.Initialise(false);

            // Cache components
            CacheComponent(out currenciesController);
            CacheComponent(out upgradesController);
            CacheComponent(out particlesController);
            CacheComponent(out floatingTextController);
            CacheComponent(out experienceController);
            CacheComponent(out weaponsController);
            CacheComponent(out charactersController);
            CacheComponent(out balanceController);
            CacheComponent(out enemyController);
            CacheComponent(out tutorialController);
            CacheComponent(out dronesController);
            CacheComponent(out gachaController);
        }

        private void Start()
        {
            InitialiseGame();
        }

        public void InitialiseGame()
        {
            CustomMusicController.Initialise(AudioController.Music.menuMusic);

            uiController.Initialise();

            // Fix Invalid AABB inAABB on all Canvas objects
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                if (canvas.GetComponent<AABBFixer>() == null)
                    canvas.gameObject.AddComponent<AABBFixer>();
            }

            currenciesController.Initialise();
            tutorialController.Initialise();
            upgradesController.Initialise();
            particlesController.Initialise();
            floatingTextController.Inititalise();

            settings.Initialise();

            LevelController.Initialise();

            experienceController.Initialise();
            weaponsController.Initialise();
            charactersController.Initialise();
            balanceController.Initialise();
            enemyController.Initialise();
            if (dronesController != null) dronesController.Initialise();
            if (gachaController != null) gachaController.Initialise();

            LevelController.SpawnPlayer();

            uiController.InitialisePages();

            if (LevelController.IsFirstTimePlayer)
            {
                // Lần đầu chơi game: Bỏ qua Main Menu và vào thẳng Level 1 World 1 để chạy hướng dẫn (Tutorial)
                UIController.ShowPage<UIGame>();
                CameraController.SetCameraShiftState(true);
                CameraController.EnableCamera(CameraType.Main);
                
                LevelController.LoadLevel(0, 0);
                LevelController.StartGameplay();
                Control.EnableMovementControl();
                
                var character = CharacterBehaviour.GetBehaviour();
                if (character != null)
                {
                    character.Activate();
                    character.ActivateMovement();
                    character.ActivateAgent();
                }
            }
            else
            {
                // Đã hoàn thành màn 1: Hiện Menu sảnh chờ Lobby bình thường
                UIController.ShowPage<UIMainMenu>();
                CameraController.SetCameraShiftState(false);
                LevelController.LoadLobby();
            }
        }

        public static void OnGameStarted()
        {
            isGameActive = true;
        }

        public static void LevelComplete()
        {
            if (!isGameActive)
                return;

            LevelData currentLevel = LevelController.CurrentLevelData;

            UIComplete completePage = UIController.GetPage<UIComplete>();
            completePage.SetData(ActiveRoom.CurrentWorldIndex + 1, ActiveRoom.CurrentLevelIndex + 1, currentLevel.GetCoinsReward(), currentLevel.XPAmount, currentLevel.GetGemsReward(), currentLevel.GetCardsReward());

            UIController.OnPageOpenedEvent += OnCompletePageOpened;
            instance.weaponsController.CheckWeaponUpdateState();

            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();

            isGameActive = false;
        }

        private static void OnCompletePageOpened(UIPage page, System.Type pageType)
        {
            if(pageType == typeof(UIComplete))
            {
                LevelController.UnloadLevel();

                UIController.OnPageOpenedEvent -= OnCompletePageOpened;
            }
        }

        public static void OnLevelCompleteClosed()
        {
            UIController.HidePage<UIComplete>(() =>
             {
                 if(LevelController.NeedCharacterSugession)
                 {
                     UIController.ShowPage<UICharacterSuggestion>();
                 }
                 else
                 {
                     ShowMainMenuAfterLevelComplete();
                 }
             });
        }

        public static void OnCharacterSugessionClosed()
        {
            ShowMainMenuAfterLevelComplete();
        }

        private static void ShowMainMenuAfterLevelComplete()
        {
            // AdsManager.ShowInterstitial(null);

            CustomMusicController.ToggleMusic(AudioController.Music.menuMusic, 0.3f, 0.3f);

            CameraController.SetCameraShiftState(false);
            CameraController.EnableCamera(CameraType.Main); // Camera follow cho Lobby

            // Đảm bảo Canvas của UIMainMenu được bật đúng cách thông qua ShowPage (tránh set cứng gây lỗi skip animation)

            UIController.ShowPage<UIMainMenu>();
            ExperienceController.GainXPPoints(LevelController.CurrentLevelData.XPAmount);

            SaveController.Save(true);

            LevelController.LoadLobby(); // Nạp sảnh chờ Lobby
        }

        public static void OnLevelExit()
        {
            isGameActive = false;
        }

        public static void OnLevelFailded()
        {
            if (!isGameActive)
                return;

            UIController.HidePage<UIGame>(() =>
            {
                UIController.ShowPage<UIGameOver>();
                UIController.OnPageOpenedEvent += OnFailedPageOpened;
            });

            LevelController.OnLevelFailed();

            isGameActive = false;
        }

        private static void OnFailedPageOpened(UIPage page, System.Type pageType)
        {
            if (pageType == typeof(UIGameOver))
            {
                // AdsManager.ShowInterstitial(null);

                UIController.OnPageOpenedEvent -= OnFailedPageOpened;
            }
        }

        public static void OnReplayLevel()
        {
            isGameActive = true;

            CustomMusicController.ToggleMusic(AudioController.Music.menuMusic, 0.3f, 0.3f);

            CameraController.SetCameraShiftState(false);
            CameraController.EnableCamera(CameraType.Main); // Camera follow cho Lobby
            LevelController.UnloadLevel();

            UIController.HidePage<UIGameOver>(() =>
            {
                LevelController.LoadLobby(); // Nạp sảnh chờ Lobby
                // Đảm bảo Canvas của UIMainMenu được bật đúng cách thông qua ShowPage
                UIController.ShowPage<UIMainMenu>();
            });
        }

        public static void OnRevive()
        {
            isGameActive = true;

            UIController.HidePage<UIGameOver>(() =>
            {
                LevelController.ReviveCharacter();

                UIController.ShowPage<UIGame>();
            });
        }

        #region Extensions
        public bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));

            component = null;

            return false;
        }
        #endregion
    }
}