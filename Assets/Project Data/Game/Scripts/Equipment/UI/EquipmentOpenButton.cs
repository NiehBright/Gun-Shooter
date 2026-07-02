using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Script gắn vào nút mở UI trang bị.
    /// </summary>
    public class EquipmentOpenButton : MonoBehaviour
    {
        [SerializeField] GameObject equipmentPanel;

        private void Start()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            EquipmentPanelUI.Show();
        }
    }
}
