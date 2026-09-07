using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterStageData
    {
        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [SerializeField] Sprite lockedSprite;
        public Sprite LockedSprite => lockedSprite;

        [SerializeField] UnityEngine.AddressableAssets.AssetReferenceGameObject prefabRef;
        public UnityEngine.AddressableAssets.AssetReferenceGameObject PrefabRef => prefabRef;

        private GameObject loadedPrefab;
        public GameObject Prefab
        {
            get
            {
                if (loadedPrefab == null && prefabRef != null && prefabRef.RuntimeKeyIsValid())
                {
                    loadedPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(prefabRef).WaitForCompletion();
                }
                return loadedPrefab;
            }
        }

        [SerializeField] Vector3 healthBarOffset;
        public Vector3 HealthBarOffset => healthBarOffset;
    }
}
