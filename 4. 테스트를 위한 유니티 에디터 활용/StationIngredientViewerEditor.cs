using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrintCurrentIngredients))]
public class StationIngredientViewerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();

        // 대상 가져오기
        PrintCurrentIngredients viewer = (PrintCurrentIngredients)target;

        EditorGUILayout.Space();

        // 버튼: 현재 재료 정보 갱신
        if (GUILayout.Button("현재 재료 정보 갱신"))
        {
            viewer.UpdateCurrentIngredients();
        }

        // 버튼: 지정 재료 존재 여부 검사
        if (GUILayout.Button("재료 존재 여부 검사"))
        {
            viewer.TestIngredientPresence();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("현재 재료 목록", EditorStyles.boldLabel);

        if (viewer.currentIngredients != null && viewer.currentIngredients.Count > 0)
        {
            foreach (var ingredient in viewer.currentIngredients)
            {
                EditorGUILayout.LabelField($"• {ingredient}");
            }
        }
        else
        {
            EditorGUILayout.LabelField("재료 없음", EditorStyles.miniLabel);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("검사 결과 요약", EditorStyles.boldLabel);

        // Inspector에 결과 메시지 표시
        if (!string.IsNullOrEmpty(viewer.resultLog))
        {
            EditorGUILayout.HelpBox(viewer.resultLog, MessageType.Info);
        }
        else
        {
            EditorGUILayout.LabelField("아직 검사하지 않았습니다", EditorStyles.miniLabel);
        }
    }
}