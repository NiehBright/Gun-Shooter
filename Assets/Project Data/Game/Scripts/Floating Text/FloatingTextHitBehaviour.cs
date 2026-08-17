using TMPro;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class FloatingTextHitBehaviour : FloatingTextBaseBehaviour
    {
        [SerializeField] TextMeshProUGUI floatingText;

        [Space]
        [SerializeField] float delay;
        [SerializeField] float disableDelay;
        [SerializeField] float scale;
        [SerializeField] float time;
        [SerializeField] Ease.Type easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] Ease.Type scaleEasing;

        private Vector3 defaultScale;
        private SimpleCallback onDelayCompleteCallback;
        private SimpleCallback onRotateCompleteCallback;
        private SimpleCallback onDisableDelayCompleteCallback;

        private void Awake()
        {
            defaultScale = transform.localScale;
            onDelayCompleteCallback = OnDelayComplete;
            onRotateCompleteCallback = OnRotateComplete;
            onDisableDelayCompleteCallback = OnDisableDelayComplete;
        }

        public override void Activate(string text, float scale = 1.0f)
        {
            floatingText.text = text;

            int sign = Random.value >= 0.5f ? 1 : -1;

            transform.localScale = defaultScale * scale * this.scale;
            transform.localRotation = Quaternion.Euler(70, 0, 18 * sign);

            Tween.DelayedCall(delay, onDelayCompleteCallback);
        }

        private void OnDelayComplete()
        {
            transform.DOLocalRotate(Quaternion.Euler(70, 0, 0), time).SetEasing(easing).OnComplete(onRotateCompleteCallback);
            transform.DOScale(defaultScale, scaleTime).SetEasing(scaleEasing);
        }

        private void OnRotateComplete()
        {
            Tween.DelayedCall(disableDelay, onDisableDelayCompleteCallback);
        }

        private void OnDisableDelayComplete()
        {
            gameObject.SetActive(false);
        }
    }
}