using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.Upgrades;

namespace Watermelon
{
    public abstract class UIUpgradeAbstractPanel : MonoBehaviour
    {
        [Header("Power")]
        [SerializeField] protected GameObject powerObject;
        [SerializeField] protected TextMeshProUGUI powerText;

        [Header("Selection")]
        [SerializeField] protected Image selectionImage;
        [SerializeField] protected Transform backgroundTransform;

        protected RectTransform panelRectTransform;
        public RectTransform RectTransform => panelRectTransform;

        public abstract bool IsUnlocked { get; }

        protected bool isUpgradeAnimationPlaying;

        protected BaseUpgrade BaseUpgrade { get; private set; }

        public void OnMoneyAmountChanged()
        {
            if (isUpgradeAnimationPlaying)
                return;

            RedrawUpgradeButton();
        }

        protected virtual void RedrawUpgradeButton()
        {

        }

        public virtual void OnPanelOpened()
        {

        }

        public abstract void Select();
    }
}