using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Watermelon.SquadShooter
{
    public class RoomPresetSaveWindow : EditorWindow
    {
        private static RoomPresetSaveWindow window;
        private Action<string> calback;
        private string presetName;

        public static void CreateRoomPresetSaveWindow(Action<string> calback)
        {
            window = (RoomPresetSaveWindow)GetWindow(typeof(RoomPresetSaveWindow));
            window.minSize = new Vector2(300, 56);
            window.maxSize = new Vector2(700, 56);
            window.calback = calback;
            window.ShowPopup();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            presetName = EditorGUILayout.TextField("Preset name:", presetName);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", WatermelonEditor.Styles.button_03))
            {
                Close();
            }

            if (GUILayout.Button("Save", WatermelonEditor.Styles.button_02))
            {
                calback?.Invoke(presetName);
                Close();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

    }
}
