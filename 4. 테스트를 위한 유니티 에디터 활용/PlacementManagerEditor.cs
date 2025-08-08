#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem.XR;

[CustomEditor(typeof(PlacementManager))]
public class PlacementManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("=== 테스트 전용 버튼 ===", EditorStyles.boldLabel);

        PlacementManager manager = (PlacementManager)target;

        // 1. 배치 가능한 그리드 표시
        if (GUILayout.Button("1 배치 가능한 그리드 표시"))
        {
            manager.tilemapController.ShowAllCells();
            manager.tilemapController.UpdateGridCellStates();
        }

        // 2. 배치 불가능한 그리드 표시 (모든 GridCell에 자식 하나씩 임의로 넣기)
        if (GUILayout.Button("2 배치 불가능한 그리드 표시"))
        {
            foreach (var cell in manager.tilemapController.gridCells)
            {
                if (cell.transform.childCount == 0)
                {
                    GameObject blocker = new GameObject("DummyStation");
                    blocker.tag = "Station";                                // Outline 활성화를 위해 태그 맞추기
                    blocker.transform.SetParent(cell.transform);
                    blocker.transform.localPosition = Vector3.zero;
                }
            }
            manager.tilemapController.UpdateGridCellStates();
        }

        // 3. 선택된 GridCell에 설비 배치
        if (GUILayout.Button("3 선택된 GridCell에 배치 시도"))
        {
            if (manager.testTargetGridCell != null)
            {
                manager.TestTryPlaceStationAt(manager.testTargetGridCell);
            }
            else
            {
                Debug.LogWarning("[에디터] testTargetGridCell을 인스펙터에 지정하세요.");
            }
        }

        // 4. 셀 숨기기
        if (GUILayout.Button("셀 숨기기"))
        {
            manager.tilemapController.HideAllCells();
        }
    }
}
#endif
