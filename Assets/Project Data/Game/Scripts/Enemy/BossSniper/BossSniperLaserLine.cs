using UnityEngine;

namespace Watermelon.Enemy.BossSniper
{
    public class BossSniperLaserLine
    {
        private MeshRenderer meshRenderer;

        public void Init(MeshRenderer meshRenderer)
        {
            this.meshRenderer = meshRenderer;
        }

        public void SetColor(Color color)
        {
            meshRenderer.material.SetColor("_BaseColor", color);
        }

        public void SetActive(bool isActive)
        {
            meshRenderer.gameObject.SetActive(isActive);
        }

        public void Initialise(Vector3 startPos, Vector3 hitPos, Vector3 scale)
        {
            var middlePoint = (startPos + hitPos) / 2f;

            meshRenderer.transform.position = middlePoint;
            meshRenderer.transform.localScale = scale;
            meshRenderer.transform.rotation = Quaternion.LookRotation((hitPos - startPos).normalized);
        }
    }
}