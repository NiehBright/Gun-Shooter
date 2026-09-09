using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Canvas))]
    public class OverlayUI : MonoBehaviour
    {
        [SerializeField] CurrencyUIPanelSimple coinsUI;

        private static Canvas canvas;

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }
        }

        public void Initialise()
        {
            canvas = GetComponent<Canvas>();
        }

        public static void ShowOverlay()
        {
            if (canvas == null)
            {
                OverlayUI overlay = FindAnyObjectByType<OverlayUI>(FindObjectsInactive.Include);
                if (overlay != null)
                {
                    canvas = overlay.GetComponent<Canvas>();
                }
            }

            if (canvas != null)
            {
                canvas.enabled = true;
            }
        }

        public static void HideOverlay()
        {
            if (canvas == null)
            {
                OverlayUI overlay = FindAnyObjectByType<OverlayUI>(FindObjectsInactive.Include);
                if (overlay != null)
                {
                    canvas = overlay.GetComponent<Canvas>();
                }
            }

            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
    }
}
