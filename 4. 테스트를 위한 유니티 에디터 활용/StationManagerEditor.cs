#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(StationManager))]
public class StationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        StationManager manager = (StationManager)target;

        if (GUILayout.Button("스테이션 프리팹 자동 로드"))
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/02.Prefabs/Station/Station" });
            List<GameObject> loadedPrefabs = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    loadedPrefabs.Add(prefab);
                }
            }

            // 할당
            Undo.RecordObject(manager, "Load Station Prefabs");
            manager.GetType()
                   .GetField("stationPrefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                   ?.SetValue(manager, loadedPrefabs);

            EditorUtility.SetDirty(manager);
            Debug.Log($"[Editor] {loadedPrefabs.Count}개 스테이션 프리팹 로드 완료");
        }
    }
}
#endif
