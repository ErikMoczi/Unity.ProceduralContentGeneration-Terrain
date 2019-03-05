using UnityEditor;
using UnityEngine;

namespace PCG.Terrain.Editor
{
    [CustomEditor(typeof(TerrainProfiler))]
    public class TerrainProfilerEditor : UnityEditor.Editor
    {
        private TerrainProfiler _terrainProfiler;

        private void Awake()
        {
            _terrainProfiler = target as TerrainProfiler;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += RefreshCreator;
        }

        private void OnDisable()
        {
            // ReSharper disable once DelegateSubtraction
            Undo.undoRedoPerformed -= RefreshCreator;
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(_terrainProfiler.Working))
            {
                if (_terrainProfiler.AutoUpdate)
                {
                    EditorGUI.BeginChangeCheck();
                    DrawDefaultInspector();
                    if (EditorGUI.EndChangeCheck() & _terrainProfiler.AutoUpdate)
                    {
                        if (!Application.isPlaying) return;
                        RefreshCreator();
                    }
                }
                else
                {
                    DrawDefaultInspector();
                }

                if (!Application.isPlaying) return;

                if (GUILayout.Button("SetUp"))
                {
                    _terrainProfiler.SetUp();
                }

                if (GUILayout.Button("Run"))
                {
                    _terrainProfiler.Run();
                }

                if (GUILayout.Button("Clear"))
                {
                    _terrainProfiler.Clear();
                }
            }
        }

        private void RefreshCreator()
        {
            if (Application.isPlaying)
            {
                _terrainProfiler.SetUp();
                _terrainProfiler.Run();
            }
        }
    }
}