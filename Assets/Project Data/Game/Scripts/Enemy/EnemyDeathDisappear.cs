using System.Collections;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyDeathDisappear : MonoBehaviour
    {
        [Header("Cài đặt hiệu ứng biến mất")]
        [Tooltip("Thời gian chờ trước khi bắt đầu biến mất (giây)")]
        [SerializeField] private float delayBeforeDisappear = 0.05f;

        [Tooltip("Thời gian hiệu ứng thu nhỏ (giây)")]
        [SerializeField] private float shrinkDuration = 0.2f;

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
            if (enemy == null || !enemy.gameObject.activeInHierarchy) return;

            // 1. Reset xương về vị trí chuẩn ban đầu ngay lập tức để xóa bỏ hoàn toàn hiện tượng kéo giãn model
            if (enemy.Ragdoll != null)
            {
                enemy.Ragdoll.Reset();
                enemy.Ragdoll.Disable();
            }

            // 2. Chặn tất cả lực vật lý trên các xương
            Rigidbody[] rigidbodies = enemy.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }

            // 3. Tắt Animator để giữ nguyên hình dạng chuẩn
            if (enemy.Animator != null)
            {
                enemy.Animator.enabled = false;
            }

            // 4. Bắt đầu coroutine thu nhỏ và biến mất nhanh chóng
            StartCoroutine(DisappearCoroutine(enemy));
        }

        private IEnumerator DisappearCoroutine(BaseEnemyBehavior enemy)
        {
            if (enemy == null) yield break;

            Transform enemyTransform = enemy.transform;
            if (enemyTransform == null) yield break;

            if (delayBeforeDisappear > 0)
            {
                yield return new WaitForSeconds(delayBeforeDisappear);
            }

            if (enemyTransform == null || !enemy.gameObject.activeInHierarchy) yield break;

            Vector3 originalScale = enemyTransform.localScale;
            if (originalScale.sqrMagnitude < 0.01f) originalScale = Vector3.one;

            float elapsed = 0f;

            // Thu nhỏ mượt mà về 0 trong 0.2 giây
            while (elapsed < shrinkDuration)
            {
                if (enemyTransform == null || !enemy.gameObject.activeInHierarchy) yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / shrinkDuration);

                float scale = 1f - t;
                enemyTransform.localScale = originalScale * scale;

                yield return null;
            }

            if (enemyTransform != null && enemy.gameObject.activeInHierarchy)
            {
                // Reset xương và khôi phục scale ban đầu cho Object Pool tái sử dụng
                if (enemy.Ragdoll != null)
                {
                    enemy.Ragdoll.Reset();
                }

                enemyTransform.localScale = originalScale;
                enemy.gameObject.SetActive(false);
            }
        }
    }
}
