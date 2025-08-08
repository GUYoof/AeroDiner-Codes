using UnityEditor;
using UnityEngine;

/// <summary>
/// TilemapController의 커스텀 인스펙터
/// 셀을 보이거나 숨기는 버튼을 제공
/// </summary>
[CustomEditor(typeof(TilemapController))]
public class TilemapControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TilemapController controller = (TilemapController)target;

        GUILayout.Space(10);
        GUILayout.Label("그리드 셀 표시 제어", EditorStyles.boldLabel);

        if (GUILayout.Button("셀 보이기"))
        {
            controller.ShowAllCells();
        }

        if (GUILayout.Button("셀 숨기기"))
        {
            controller.HideAllCells();
        }
    }
}