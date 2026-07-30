#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Tinh nang 3: Ban do nhiet mat do ke dich (Enemy Heatmap) trong Scene View.
    /// Kich hoat/tat qua: Tools > Squad Shooter > Toggle Enemy Heatmap   (hoac Shift+H)
    /// </summary>
    [InitializeOnLoad]
    public static class EnemyHeatmapOverlay
    {
        private const string PREF_KEY    = "LevelEditor_HeatmapEnabled";
        private const string MENU_PATH   = "Tools/Squad Shooter/Toggle Enemy Heatmap";
        private const float  RADIUS      = 2.5f;
        private const float  ALPHA_BASE  = 0.18f;

        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool(PREF_KEY, false);
            set => EditorPrefs.SetBool(PREF_KEY, value);
        }

        static EnemyHeatmapOverlay()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        [MenuItem(MENU_PATH)]
        private static void Toggle()
        {
            IsEnabled = !IsEnabled;
            SceneView.RepaintAll();
            Debug.Log("[Heatmap] Enemy Heatmap: " + (IsEnabled ? "BAT" : "TAT"));
        }

        [MenuItem(MENU_PATH, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MENU_PATH, IsEnabled);
            return true;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.H && e.shift)
            {
                Toggle();
                e.Use();
            }

            if (!IsEnabled) return;

            LevelEditorEnemy[] allEnemies = Object.FindObjectsByType<LevelEditorEnemy>(FindObjectsInactive.Exclude);
            if (allEnemies == null || allEnemies.Length == 0)
            {
                Handles.BeginGUI();
                GUI.Label(new Rect(10, 40, 280, 24), "Heatmap: Khong co ke dich trong phong.", new GUIStyle(EditorStyles.helpBox));
                Handles.EndGUI();
                return;
            }

            Dictionary<LevelEditorEnemy, int> density = new Dictionary<LevelEditorEnemy, int>();
            foreach (var a in allEnemies) density[a] = 0;

            foreach (var a in allEnemies)
                foreach (var b in allEnemies)
                    if (a != b && Vector3.Distance(a.transform.position, b.transform.position) < RADIUS * 2f)
                        density[a]++;

            int maxDensity = 1;
            foreach (var kvp in density)
                if (kvp.Value > maxDensity) maxDensity = kvp.Value;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            foreach (var kvp in density)
            {
                float t = Mathf.Clamp01((float)kvp.Value / maxDensity);
                Color heatColor = Color.Lerp(
                    Color.Lerp(new Color(0.2f, 0.9f, 0.3f), new Color(0.95f, 0.85f, 0.1f), t * 2f),
                    new Color(0.95f, 0.1f, 0.1f),
                    Mathf.Max(0f, t * 2f - 1f)
                );
                heatColor.a = ALPHA_BASE + t * (1f - ALPHA_BASE) * 0.6f;

                Handles.color = heatColor;
                Vector3 pos   = kvp.Key.transform.position;
                Handles.DrawSolidDisc(pos, Vector3.up, RADIUS);

                Handles.color = new Color(1f, 1f, 1f, 0.4f);
                Handles.DrawWireDisc(pos, Vector3.up, RADIUS);

                if (kvp.Value > 0)
                {
                    GUIStyle lblStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontStyle = FontStyle.Bold
                    };
                    lblStyle.normal.textColor = Color.white;
                    Handles.Label(pos + Vector3.up * 0.5f, "x" + (kvp.Value + 1), lblStyle);
                }
            }

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            Handles.BeginGUI();
            var bgRect = new Rect(10, 40, 280, 52);
            EditorGUI.DrawRect(bgRect, new Color(0f, 0f, 0f, 0.55f));

            GUIStyle titleStyle = new GUIStyle(EditorStyles.label);
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(14, 44, 272, 20), "Enemy Heatmap  (Shift+H de tat)", titleStyle);

            Rect legendRect = new Rect(14, 64, 260, 14);
            float lw = legendRect.width / 3f;
            EditorGUI.DrawRect(new Rect(legendRect.x,         legendRect.y, lw, legendRect.height), new Color(0.2f, 0.9f, 0.3f, 0.9f));
            EditorGUI.DrawRect(new Rect(legendRect.x + lw,    legendRect.y, lw, legendRect.height), new Color(0.95f, 0.85f, 0.1f, 0.9f));
            EditorGUI.DrawRect(new Rect(legendRect.x + lw*2f, legendRect.y, lw, legendRect.height), new Color(0.95f, 0.1f, 0.1f, 0.9f));
            GUI.Label(new Rect(legendRect.x,         legendRect.y, lw, legendRect.height), "Thua",     EditorStyles.centeredGreyMiniLabel);
            GUI.Label(new Rect(legendRect.x + lw,    legendRect.y, lw, legendRect.height), "Vua",      EditorStyles.centeredGreyMiniLabel);
            GUI.Label(new Rect(legendRect.x + lw*2f, legendRect.y, lw, legendRect.height), "Day dac",  EditorStyles.centeredGreyMiniLabel);
            Handles.EndGUI();

            sceneView.Repaint();
        }
    }
}

