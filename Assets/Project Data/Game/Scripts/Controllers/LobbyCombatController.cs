using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Watermelon.SquadShooter
{
    public class LobbyCombatController : MonoBehaviour
    {
        private static LobbyCombatController instance;

        private Button enterCombatBtn;
        private Button exitCombatBtn;

        public static bool IsCombatModeActive { get; private set; } = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            GameObject go = new GameObject("[LOBBY COMBAT CONTROLLER]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<LobbyCombatController>();
        }

        private void Update()
        {
            // 1. Tự động kiểm tra và kết nối với nút "Thử Sát Thương" trong UIMainMenu (UI Game Menu sảnh chờ) nếu có
            var mainMenu = UIController.GetPage<UIMainMenu>();
            if (mainMenu != null && enterCombatBtn == null)
            {
                Transform enterBtnTrans = mainMenu.transform.Find("EnterCombatLobbyButton");
                if (enterBtnTrans != null)
                {
                    enterCombatBtn = enterBtnTrans.GetComponent<Button>();
                    if (enterCombatBtn != null)
                    {
                        enterCombatBtn.onClick.RemoveAllListeners();
                        enterCombatBtn.onClick.AddListener(EnterCombatMode);
                    }
                }
            }

            // 2. Tự động kiểm tra và kết nối với nút "Thoát Thử Sát Thương" trong UIGame nếu có
            var uiGame = UIController.GetPage<UIGame>();
            if (uiGame != null && exitCombatBtn == null)
            {
                Transform exitBtnTrans = uiGame.transform.Find("ExitCombatLobbyButton");
                if (exitBtnTrans != null)
                {
                    exitCombatBtn = exitBtnTrans.GetComponent<Button>();
                    if (exitCombatBtn != null)
                    {
                        exitCombatBtn.onClick.RemoveAllListeners();
                        exitCombatBtn.onClick.AddListener(ExitCombatMode);
                    }
                }
            }

            // 3. Đảm bảo hiển thị nút "Thử Sát Thương" khi không trong chế độ test
            if (enterCombatBtn != null)
            {
                enterCombatBtn.gameObject.SetActive(!IsCombatModeActive);
            }

            // 4. Quản lý trạng thái hiển thị của nút Exit Combat:
            if (exitCombatBtn != null)
            {
                exitCombatBtn.gameObject.SetActive(IsCombatModeActive);
            }
        }

        private void EnterCombatMode()
        {
            IsCombatModeActive = true;

            // 1. Tắt LobbyMode trên nhân vật để cho phép bắn súng và nhắm bắn
            CharacterBehaviour.IsLobbyModeActive = false;

            // 2. Mở giao diện UI chiến đấu (UIGame) và tắt giao diện sảnh (UIMainMenu)
            UIController.ShowPage<UIGame>();
            UIController.HidePage<UIMainMenu>();

            // 3. Bật hiển thị Notch Panel và Attack Button trong UIGame
            var uiGame = UIController.GetPage<UIGame>();
            if (uiGame != null)
            {
                Transform notchPanel = uiGame.transform.Find("Notch Panel");
                if (notchPanel != null)
                {
                    notchPanel.gameObject.SetActive(true);
                }

                Transform attackButton = uiGame.transform.Find("Attack Button");
                if (attackButton != null)
                {
                    attackButton.gameObject.SetActive(true);
                }

                uiGame.UpdateAttackButtonVisibility();
                uiGame.UpdateAutoShootButtonUI();
            }

            // 4. Quét tìm tất cả các quái bia tập trong sảnh chờ để nhắm bắn ngay lập tức
            var player = CharacterBehaviour.GetBehaviour();
            if (player != null && player.EnemyDetector != null)
            {
                player.EnemyDetector.Reload();
                BaseEnemyBehavior[] allEnemies = Object.FindObjectsByType<BaseEnemyBehavior>(FindObjectsInactive.Exclude);
                foreach (var enemy in allEnemies)
                {
                    if (enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead)
                    {
                        player.EnemyDetector.TryAddClosestEnemy(enemy);
                    }
                }
            }
        }

        private void ExitCombatMode()
        {
            IsCombatModeActive = false;

            // 1. Kích hoạt lại LobbyMode trên nhân vật
            CharacterBehaviour.IsLobbyModeActive = true;

            // 2. Mở lại giao diện sảnh chính (UIMainMenu) (UIGame luôn mở để hiện Joystick di chuyển)
            UIController.ShowPage<UIMainMenu>();

            // 3. Tắt hiển thị Notch Panel và Attack Button trong UIGame
            var uiGame = UIController.GetPage<UIGame>();
            if (uiGame != null)
            {
                Transform notchPanel = uiGame.transform.Find("Notch Panel");
                if (notchPanel != null)
                {
                    notchPanel.gameObject.SetActive(false);
                }

                Transform attackButton = uiGame.transform.Find("Attack Button");
                if (attackButton != null)
                {
                    attackButton.gameObject.SetActive(false);
                }

                uiGame.UpdateAttackButtonVisibility();
                uiGame.UpdateAutoShootButtonUI();
            }
        }
    }
}
