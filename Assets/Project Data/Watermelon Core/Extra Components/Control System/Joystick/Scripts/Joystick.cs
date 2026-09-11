using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Animator))]
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IControlBehavior
    {
        public static Joystick Instance { get; private set; }

        [Header("Joystick")]
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Image handleImage;

        [Space]
        [SerializeField] Color backgroundActiveColor = Color.white;
        [SerializeField] Color backgroundDisableColor = Color.white;

        [SerializeField] Color handleActiveColor = Color.white;
        [SerializeField] Color handleDisableColor = Color.white;

        [Space]
        [SerializeField] float handleRange = 1;
        [SerializeField] float deadZone = 0;

        [Header("Tutorial")]
        [SerializeField] bool useTutorial;
        [SerializeField] GameObject pointerGameObject;

        private RectTransform baseRectTransform;
        private RectTransform backgroundRectTransform;
        private RectTransform handleRectTransform;

        private CanvasGroup visualsCanvasGroup;
        private GameObject pendingTargetUI;

        private bool isActive;
        public bool IsMovementInputNonZero => isActive;

        private bool canDrag;

        private Canvas canvas;
        private Camera canvasCamera;

        protected Vector2 input = Vector2.zero;

        public Vector3 Input => input;
        public Vector3 MovementInput => new Vector3(input.x, 0, input.y);

        private Vector2 defaultAnchoredPosition;

        public bool IsLookInputNonZero => false;
        public Vector3 LookInput => Vector3.zero;

        private Animator joystickAnimator;
        private bool isTutorialDisplayed;
        private bool hideVisualsActive;

        // Events
        public event SimpleCallback OnMovementInputActivated;

        private void Awake()
        {
            if (backgroundImage != null)
            {
                visualsCanvasGroup = backgroundImage.rectTransform.GetComponent<CanvasGroup>();
                if (visualsCanvasGroup == null)
                    visualsCanvasGroup = backgroundImage.rectTransform.gameObject.AddComponent<CanvasGroup>();

                visualsCanvasGroup.alpha = 0f;
                visualsCanvasGroup.blocksRaycasts = false;
                visualsCanvasGroup.interactable = false;
            }

            if (Control.InputType == InputType.UIJoystick)
            {
                Control.SetControl(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Initialise(Canvas canvas)
        {
            this.canvas = canvas;

            Instance = this;

            joystickAnimator = GetComponent<Animator>();

            baseRectTransform = GetComponent<RectTransform>();
            backgroundRectTransform = backgroundImage.rectTransform;
            handleRectTransform = handleImage.rectTransform;

            if (visualsCanvasGroup == null && backgroundRectTransform != null)
            {
                visualsCanvasGroup = backgroundRectTransform.GetComponent<CanvasGroup>();
                if (visualsCanvasGroup == null)
                    visualsCanvasGroup = backgroundRectTransform.gameObject.AddComponent<CanvasGroup>();
            }

            if (visualsCanvasGroup != null)
            {
                visualsCanvasGroup.alpha = 0f;
                visualsCanvasGroup.blocksRaycasts = false;
                visualsCanvasGroup.interactable = false;
            }

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                canvasCamera = canvas.worldCamera;

            Vector2 center = new Vector2(0.5f, 0.5f);
            backgroundRectTransform.pivot = center;
            handleRectTransform.anchorMin = center;
            handleRectTransform.anchorMax = center;
            handleRectTransform.pivot = center;
            handleRectTransform.anchoredPosition = Vector2.zero;

            isActive = false;

            if(useTutorial)
            {
                joystickAnimator.enabled = true;
                isTutorialDisplayed = true;

                pointerGameObject.SetActive(true);
            }
            else
            {
                joystickAnimator.enabled = false;
                isTutorialDisplayed = false;

                pointerGameObject.SetActive(false);
            }

            backgroundImage.color = backgroundDisableColor.SetAlpha(0f);
            handleImage.color = handleDisableColor.SetAlpha(0f);

            defaultAnchoredPosition = backgroundRectTransform.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 1. Kiểm tra nếu chạm vào nút UI nào khác (Button, Toggle, Scroll, ...) trên màn hình
            if (IsPointerOverOtherUI(eventData, out GameObject targetUI))
            {
                canDrag = false;
                isActive = false;
                pendingTargetUI = targetUI;
                eventData.pointerPress = targetUI;
                eventData.rawPointerPress = targetUI;
                ExecuteEvents.Execute(targetUI, eventData, ExecuteEvents.pointerDownHandler);
                return;
            }

            // 2. Kiểm tra nút 3D WorldSpace trong game
            canDrag = !WorldSpaceRaycaster.Raycast(eventData);

            if (!canDrag) return;

            if (!isTutorialDisplayed)
            {
                isTutorialDisplayed = true;

                joystickAnimator.enabled = false;
                pointerGameObject.SetActive(false);
            }

            backgroundRectTransform.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);

            if (visualsCanvasGroup != null)
            {
                visualsCanvasGroup.alpha = hideVisualsActive ? 0f : 1f;
            }

            backgroundImage.color = backgroundActiveColor.SetAlpha(hideVisualsActive ? 0f : backgroundActiveColor.a);
            handleImage.color = handleActiveColor.SetAlpha(hideVisualsActive ? 0f : handleActiveColor.a);

            isActive = true;

            OnMovementInputActivated?.Invoke();

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActive || !canDrag)
                return;

            Vector2 position = RectTransformUtility.WorldToScreenPoint(canvasCamera, backgroundRectTransform.position);
            Vector2 radius = backgroundRectTransform.sizeDelta / 2;
            input = (eventData.position - position) / (radius * canvas.scaleFactor);
            HandleInput(input.magnitude, input.normalized, radius, canvasCamera);
            handleRectTransform.anchoredPosition = input * radius * handleRange;
        }

        protected void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (magnitude > deadZone)
            {
                if (magnitude > 1)
                    input = normalised;
            }
            else
                input = Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pendingTargetUI != null)
            {
                ExecuteEvents.Execute(pendingTargetUI, eventData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(pendingTargetUI, eventData, ExecuteEvents.pointerClickHandler);
                pendingTargetUI = null;
                return;
            }

            WorldSpaceRaycaster.OnPointerUp(eventData);

            if (!isActive)
                return;

            isActive = false;

            ResetControl();
        }

        public void ResetControl()
        {
            isActive = false;

            if (visualsCanvasGroup != null)
            {
                visualsCanvasGroup.alpha = 0f;
            }

            backgroundImage.color = backgroundDisableColor.SetAlpha(0f);
            handleImage.color = handleDisableColor.SetAlpha(0f);

            backgroundRectTransform.anchoredPosition = defaultAnchoredPosition;

            input = Vector2.zero;
            handleRectTransform.anchoredPosition = Vector2.zero;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRectTransform, screenPosition, canvasCamera, out localPoint))
            {
                Vector2 pivotOffset = baseRectTransform.pivot * baseRectTransform.sizeDelta;
                return localPoint - (backgroundRectTransform.anchorMax * baseRectTransform.sizeDelta) + pivotOffset;
            }
            return Vector2.zero;
        }

        private bool IsPointerOverOtherUI(PointerEventData eventData, out GameObject targetUI)
        {
            targetUI = null;
            if (EventSystem.current == null) return false;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (go == null || go == gameObject || go.transform.IsChildOf(transform))
                    continue;

                // Kiểm tra xem GameObject này (hoặc cha của nó) có component nhận sự kiện click / nhấn không
                var handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);
                if (handler == null)
                    handler = ExecuteEvents.GetEventHandler<IPointerDownHandler>(go);
                if (handler == null)
                    handler = ExecuteEvents.GetEventHandler<ISubmitHandler>(go);

                if (handler != null && handler != gameObject && !handler.transform.IsChildOf(transform))
                {
                    targetUI = handler;
                    return true;
                }
            }

            return false;
        }

        public void EnableMovementControl()
        {
            gameObject.SetActive(true);
        }

        public void DisableMovementControl()
        {
            gameObject.SetActive(false);
            isActive = false;

            ResetControl();
        }

        public void HideVisuals()
        {
            hideVisualsActive = true;

            if (visualsCanvasGroup != null)
                visualsCanvasGroup.alpha = 0f;

            backgroundImage.color = backgroundImage.color.SetAlpha(0f);
            handleImage.color = backgroundImage.color.SetAlpha(0f);
        }

        public void ShowVisuals()
        {
            hideVisualsActive = false;

            if (visualsCanvasGroup != null)
                visualsCanvasGroup.alpha = isActive ? 1f : 0f;

            backgroundImage.color = backgroundImage.color.SetAlpha(1f);
            handleImage.color = backgroundImage.color.SetAlpha(1f);
        }

        public delegate void OnJoystickTouchedCallback();
    }
}