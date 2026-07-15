using System.Collections;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>

    /// </summary>
    public class EnemyDeathDisappear : MonoBehaviour
    {
        [Header("Cài đặt hiệu ứng biến mất")]
        [Tooltip("Thời gian chờ trước khi bắt đầu biến mất (giây)")]
        [SerializeField] private float delayBeforeDisappear = 0.3f;

        [Tooltip("Thời gian hiệu ứng thu nhỏ (giây)")]
        [SerializeField] private float shrinkDuration = 0.5f;

        private void OnEnable()
        {
            BaseEnemyBehavior.OnDiedEvent += OnEnemyDied;
        }

        private void OnDisable()
        {
            BaseEnemyBehavior.OnDiedEvent -= OnEnemyDied;
        }

        private void OnEnemyDied(BaseEnemyBehavior enemy)
        {
            if (enemy == null) return;

            // Ngay lập tức tắt ragdoll để không bị kéo giãn
            if (enemy.Ragdoll != null)
            {
                enemy.Ragdoll.Disable();
            }

            // Tắt tất cả Rigidbody trên xương để chặn vật lý
            Rigidbody[] rigidbodies = enemy.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Tắt Animator để không bị xung đột
            if (enemy.Animator != null)
            {
                enemy.Animator.enabled = false;
            }

            // Bắt đầu hiệu ứng biến mất
            StartCoroutine(DisappearCoroutine(enemy));
        }

        private IEnumerator DisappearCoroutine(BaseEnemyBehavior enemy)
        {
            Transform enemyTransform = enemy.transform;

            // Chờ một chút trước khi biến mất (để người chơi thấy quái đã chết)
            yield return new WaitForSeconds(delayBeforeDisappear);

            if (enemyTransform == null) yield break;

            // Lưu scale ban đầu
            Vector3 originalScale = enemyTransform.localScale;
            float elapsed = 0f;

            // Thu nhỏ dần về 0
            while (elapsed < shrinkDuration)
            {
                if (enemyTransform == null) yield break;

                elapsed += Time.deltaTime;
                float t = elapsed / shrinkDuration;

                // Easing: bắt đầu chậm, cuối nhanh (ease-in)
                float scale = 1f - (t * t);
                enemyTransform.localScale = originalScale * scale;

                yield return null;
            }

            // Ẩn hoàn toàn
            if (enemyTransform != null)
            {
                enemyTransform.localScale = Vector3.zero;
                enemyTransform.gameObject.SetActive(false);
            }
        }
    }
}
