using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class FPSCounter : MonoBehaviour
    {
        private float deltaTime = 0.0f;
        private GUIStyle style;

        private void Start()
        {
            style = new GUIStyle();
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = Screen.height * 2 / 50;
            style.normal.textColor = Color.green;
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

            if (fps < 30)
                style.normal.textColor = Color.red;
            else if (fps < 50)
                style.normal.textColor = Color.yellow;
            else
                style.normal.textColor = Color.green;

            // Draw shadow
            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = Color.black;
            Rect shadowRect = new Rect(Screen.width - 250, 10, 250, 50);
            shadowRect.x += 2; shadowRect.y += 2;
            GUI.Label(shadowRect, text, shadowStyle);

            // Draw text
            Rect rect = new Rect(Screen.width - 250, 10, 250, 50);
            GUI.Label(rect, text, style);
        }
    }
}
