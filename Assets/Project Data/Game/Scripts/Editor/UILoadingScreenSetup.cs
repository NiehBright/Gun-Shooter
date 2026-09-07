using UnityEngine;
using UnityEditor;
using TMPro;
using Watermelon.SquadShooter;

#if UNITY_EDITOR
namespace Watermelon.SquadShooter
{
    public class UILoadingScreenSetup : Editor
    {
        [MenuItem("GunShooter/Fix Loading Screen UI")]
        public static void SetupPrefab()
        {
            // Tìm đối tượng UILoadingScreen đang nằm trong Scene Game
            UILoadingScreen loadingScreen = FindAnyObjectByType<UILoadingScreen>(FindObjectsInactive.Include);
            
            if (loadingScreen == null)
            {
                Debug.LogError("Em không tìm thấy Loading Screen! Anh hãy chắc chắn là đang mở Scene 'Game' nhé!");
                return;
            }

            SerializedObject so = new SerializedObject(loadingScreen);
            SerializedProperty hintTextProp = so.FindProperty("hintText");

            if (hintTextProp.objectReferenceValue == null)
            {
                GameObject textObj = new GameObject("HintText");
                textObj.transform.SetParent(loadingScreen.transform, false);
                
                RectTransform rect = textObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.15f);
                rect.anchorMax = new Vector2(0.5f, 0.15f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 0);
                rect.sizeDelta = new Vector2(1000, 100);

                TextMeshProUGUI hintText = textObj.AddComponent<TextMeshProUGUI>();
                hintText.alignment = TextAlignmentOptions.Center;
                hintText.fontSize = 45;
                hintText.color = Color.white;
                hintText.textWrappingMode = TextWrappingModes.NoWrap;
                hintText.text = "Đang tải dữ liệu...";

                // Gán vào biến
                hintTextProp.objectReferenceValue = hintText;
                so.ApplyModifiedProperties();

                // Focus vào object để user dễ thấy
                Selection.activeGameObject = textObj;
                EditorGUIUtility.PingObject(textObj);

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(loadingScreen.gameObject.scene);

                Debug.Log("Thành công! Em đã tạo HintText và đang focus vào nó cho anh rồi đấy!");
            }
            else
            {
                Debug.Log("HintText đã có sẵn rồi ạ!");
                Selection.activeGameObject = ((TextMeshProUGUI)hintTextProp.objectReferenceValue).gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
        }
    }
}
#endif
