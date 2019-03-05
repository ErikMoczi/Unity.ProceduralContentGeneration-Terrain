using PCG.Terrain.Settings;
using UnityEditor;
using UnityEngine;

namespace PCG.Terrain.Editor
{
    [CustomPropertyDrawer(typeof(TerrainSettings))]
    public class TerrainSettingsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null)
            {
                var serializedObject = new SerializedObject(property.objectReferenceValue as TerrainSettings);
                var resolution = serializedObject.FindProperty("resolution");
                var chunkCount = serializedObject.FindProperty("chunkCount");
                var chunksPerFrame = serializedObject.FindProperty("chunksPerFrame");
                var changeThreshold = serializedObject.FindProperty("changeThreshold");
                var noiseSettings = serializedObject.FindProperty("noiseSettings");
                var meshSettings = serializedObject.FindProperty("meshSettings");
                var gradientResolution = serializedObject.FindProperty("gradientResolution");
                var gradient = serializedObject.FindProperty("gradient");

                if (resolution != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(resolution);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (chunkCount != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(chunkCount);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (chunksPerFrame != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(chunksPerFrame);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (changeThreshold != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(changeThreshold, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (noiseSettings != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(noiseSettings, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (meshSettings != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(meshSettings, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (gradientResolution != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(gradientResolution, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (gradient != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(gradient, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                EditorGUI.ObjectField(position, property);
            }
        }
    }
}