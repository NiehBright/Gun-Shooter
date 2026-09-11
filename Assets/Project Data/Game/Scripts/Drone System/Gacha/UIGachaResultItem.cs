using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Watermelon.SquadShooter
{
    public class UIGachaResultItem : MonoBehaviour
    {
        [SerializeField] Image droneIcon;
        [SerializeField] TextMeshProUGUI droneNameText;
        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] GameObject newBadge;
        [SerializeField] GameObject duplicateBadge;

        public void Init(GachaResult result)
        {
            if (result == null || result.Drone == null) return;

            if (droneIcon != null)
            {
                droneIcon.sprite = result.Drone.Icon;
            }

            if (droneNameText != null)
            {
                droneNameText.text = result.Drone.Name;
            }

            if (result.IsNewDrone)
            {
                if (statusText != null) statusText.text = "MỞ KHOÁ!";
                if (newBadge != null) newBadge.SetActive(true);
                if (duplicateBadge != null) duplicateBadge.SetActive(false);
            }
            else
            {
                if (statusText != null) statusText.text = $"+{result.CardsReceived} Thẻ";
                if (newBadge != null) newBadge.SetActive(false);
                if (duplicateBadge != null) duplicateBadge.SetActive(true);
            }
        }
    }
}
