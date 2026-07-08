using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Watermelon.SquadShooter
{
    public class CreateLoadingScreenHelper : EditorWindow
    {
        [MenuItem("Tools/Squad Shooter/Create Loading Screen")]
        public static void CreateLoadingScreen()
        {
            // Tìm đối tượng UIController trong scene
            UIController uiController = Object.FindObjectOfType<UIController>();
            if (uiController == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy UIController trong Scene hiện tại. Hãy mở scene Game.unity trước nhé!", "OK");
                return;
            }

            // Kiểm tra xem đã có Loading Screen chưa
            Transform existing = uiController.transform.Find("UI Page - Loading Screen");
            if (existing != null)
            {
                if (EditorUtility.DisplayDialog("Cảnh báo", "Đã tồn tại UI Page - Loading Screen trong UIController. Bạn có muốn xóa đi tạo mới không?", "Có", "Không"))
                {
                    Undo.DestroyObjectImmediate(existing.gameObject);
                }
                else
                {
                    return;
                }
            }

            // Tạo đối tượng chính
            GameObject pageObj = new GameObject("UI Page - Loading Screen");
            pageObj.transform.SetParent(uiController.transform);
            pageObj.transform.ResetLocal();

            RectTransform pageRect = pageObj.AddComponent<RectTransform>();
            pageRect.anchorMin = Vector2.zero;
            pageRect.anchorMax = Vector2.one;
            pageRect.offsetMin = Vector2.zero;
            pageRect.offsetMax = Vector2.zero;

            // Thêm CanvasGroup
            CanvasGroup canvasGroup = pageObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Thêm Background Image (Đen mờ phủ toàn màn hình)
            Image bgImg = pageObj.AddComponent<Image>();
            bgImg.color = new Color(0.07f, 0.07f, 0.08f, 1f); // Màu tối cao cấp
            bgImg.raycastTarget = true;

            // Tạo Panel chứa nội dung ở giữa
            GameObject contentPanel = new GameObject("Content Panel");
            contentPanel.transform.SetParent(pageObj.transform);
            contentPanel.transform.ResetLocal();

            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(500, 300);

            // Tạo Text Loading
            GameObject textObj = new GameObject("Text - Loading");
            textObj.transform.SetParent(contentPanel.transform);
            textObj.transform.ResetLocal();

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.6f);
            textRect.anchorMax = new Vector2(0.5f, 0.6f);
            textRect.sizeDelta = new Vector2(500, 50);
            textRect.anchoredPosition = new Vector2(0, 40);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "LOADING... 0%";
            text.fontSize = 32;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;

            // Tạo Slider
            GameObject sliderObj = new GameObject("Loading Slider");
            sliderObj.transform.SetParent(contentPanel.transform);
            sliderObj.transform.ResetLocal();

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.4f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.4f);
            sliderRect.sizeDelta = new Vector2(400, 20);
            sliderRect.anchoredPosition = new Vector2(0, -20);

            Slider slider = sliderObj.AddComponent<Slider>();

            // Tạo Slider Background
            GameObject sliderBg = new GameObject("Background");
            sliderBg.transform.SetParent(sliderObj.transform);
            sliderBg.transform.ResetLocal();

            RectTransform sliderBgRect = sliderBg.AddComponent<RectTransform>();
            sliderBgRect.anchorMin = Vector2.zero;
            sliderBgRect.anchorMax = Vector2.one;
            sliderBgRect.offsetMin = Vector2.zero;
            sliderBgRect.offsetMax = Vector2.zero;

            Image sliderBgImg = sliderBg.AddComponent<Image>();
            sliderBgImg.color = new Color(0.2f, 0.2f, 0.22f, 1f);

            // Tạo Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform);
            fillAreaObj.transform.ResetLocal();

            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2, 2);
            fillAreaRect.offsetMax = new Vector2(-2, -2);

            // Tạo Fill Image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform);
            fillObj.transform.ResetLocal();

            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0f, 0.82f, 1f, 1f); // Vibrant Cyan

            slider.targetGraphic = sliderBgImg;
            slider.fillRect = fillRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // Gán script UILoadingScreen
            UILoadingScreen loadingPage = pageObj.AddComponent<UILoadingScreen>();
            
            // Sử dụng Reflection để gán các trường private SerializeField của UIPage
            var fields = typeof(UILoadingScreen).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var f in fields)
            {
                if (f.Name == "loadingSlider") f.SetValue(loadingPage, slider);
                if (f.Name == "progressText") f.SetValue(loadingPage, text);
                if (f.Name == "canvasGroup") f.SetValue(loadingPage, canvasGroup);
            }

            // Ghi nhận Undo
            Undo.RegisterCreatedObjectUndo(pageObj, "Create Loading Screen");

            // Đánh dấu Scene đã thay đổi để lưu
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[Helper] Da tao moi thanh cong UI Page - Loading Screen trong Scene!");
            EditorUtility.DisplayDialog("Thành công", "Đã tạo mới và thiết lập giao diện Loading Screen vào Canvas thành công!", "OK");
        }
    }
}
