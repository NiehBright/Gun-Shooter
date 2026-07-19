#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CustomEditor(typeof(BlackHoleBehaviour))]
    public class BlackHoleBehaviourEditor : Editor
    {
        private void OnSceneGUI()
        {
            BlackHoleBehaviour blackHole = (BlackHoleBehaviour)target;
            if (blackHole == null) return;

            // Draw the interactive radius handle in the Scene View
            EditorGUI.BeginChangeCheck();
            
            // Purple color for the handle to match the black hole theme
            Handles.color = new Color(0.5f, 0f, 1f, 0.8f);
            
            float newRadius = Handles.RadiusHandle(Quaternion.identity, blackHole.transform.position, blackHole.Radius);
            
            if (EditorGUI.EndChangeCheck())
            {
                // Record undo state so the changes can be undone with Ctrl+Z
                Undo.RecordObject(blackHole, "Change Black Hole Radius");
                
                // Update radius
                blackHole.Radius = newRadius;
                
                // Dynamically update the visual scale of the VFX in the editor
                blackHole.transform.localScale = new Vector3(blackHole.Radius * 2f, 1f, blackHole.Radius * 2f);
                
                // Mark object as dirty to save change
                EditorUtility.SetDirty(blackHole);
            }
        }
    }
}
#endif
