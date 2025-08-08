#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// 레시피 프리뷰 버튼 및 matchedRecipeNames 출력 에디터
/// </summary>
[CustomEditor(typeof(BaseStation), true)]
public class StationEditor : Editor
{
    private RecipePreviewResult previewResult;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("레시피 미리보기", EditorStyles.boldLabel);

        BaseStation station = (BaseStation)target;

        if (GUILayout.Button("레시피 미리보기 갱신"))
        {
            previewResult = RecipePreviewer.GetPreview(station.currentIngredients, true);
        }

        if (previewResult != null)
        {
            GUILayout.Label($"매칭된 레시피: {previewResult.BestMatchedRecipeText}");

            GUILayout.Space(5);
            GUILayout.Label("유효 재료 ID 목록", EditorStyles.boldLabel);

            if (previewResult.AvailableFoodIds == null || previewResult.AvailableFoodIds.Count == 0)
            {
                GUILayout.Label("없음");
            }
            else
            {
                foreach (var id in previewResult.AvailableFoodIds)
                {
                    GUILayout.Label($"- {id}");
                }
            }
        }
    }
}
#endif