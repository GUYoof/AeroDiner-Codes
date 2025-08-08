# 테스트를 위한 유니티 에디터 활용

##  📂 폴더 구조
```
/Editor
 ├─ KeyRebindButtonEditor.cs
 ├─ PlacementManagerEditor.cs
 ├─ StationIngredientViewerEditor.cs
 ├─ StationRecipeAssigner.cs
/Runtime
 ├─ PrintCurrentIngredients.cs
 ├─ RecipePreviewer.cs
```

## 🎯 핵심 기능의 설계 특징

## 📌 핵심 코드 예시
### KeyRebindButtonEditor.cs
```
[CustomEditor(typeof(KeyRebindButton))]
public class KeyRebindButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        KeyRebindButton button = (KeyRebindButton)target;

        if (GUILayout.Button("자동 인덱스 매핑"))
        {
            string name = button.gameObject.name.ToLower();

            if (name.Contains("up")) button.bindingIndex = 1;
            else if (name.Contains("down")) button.bindingIndex = 2;
            else if (name.Contains("left")) button.bindingIndex = 3;
            else if (name.Contains("right")) button.bindingIndex = 4;
            else Debug.LogWarning($"[{button.name}] 자동 인덱스 실패: 이름에 방향이 포함되지 않음");

            EditorUtility.SetDirty(button);
        }
    }
}
```
- KeyRebindButton의 이름을 기준으로 방향키 인덱스를 자동 설정하는 에디터 확장.

### PlacementManagerEditor.cs
```
[CustomEditor(typeof(PlacementManager))]
public class PlacementManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlacementManager manager = (PlacementManager)target;

        if (GUILayout.Button("1 배치 가능한 그리드 표시"))
            manager.tilemapController.ShowAllCells();

        if (GUILayout.Button("2 배치 불가능한 그리드 표시"))
        {
            foreach (var cell in manager.tilemapController.gridCells)
            {
                if (cell.transform.childCount == 0)
                {
                    GameObject blocker = new GameObject("DummyStation");
                    blocker.tag = "Station";
                    blocker.transform.SetParent(cell.transform);
                }
            }
            manager.tilemapController.UpdateGridCellStates();
        }

        if (GUILayout.Button("3 선택된 GridCell에 배치 시도"))
            manager.TestTryPlaceStationAt(manager.testTargetGridCell);

        if (GUILayout.Button("셀 숨기기"))
            manager.tilemapController.HideAllCells();
    }
}
```
- PlacementManager에서 에디터 상에서 직접 그리드 표시/차단/설비 배치 테스트를 할 수 있는 디버그 버튼 제공.

### PrintCurrentIngredients.cs
```
public class PrintCurrentIngredients : MonoBehaviour
{
    public string stationId;
    public StationManager stationManager;
    public string[] ingredientsToCheck;
    public List<string> currentIngredients = new();
    public string resultLog;

    public void UpdateCurrentIngredients()
    {
        currentIngredients = stationManager.GetCurrentIngredients(stationId);
        if (ingredientsToCheck.Length > 0)
            stationManager.CheckIngredients(stationId, ingredientsToCheck);
    }

    public void TestIngredientPresence()
    {
        stationManager.CheckIngredients(stationId, ingredientsToCheck);
        resultLog = $"→ Station ID: '{stationId}'\n→ 검사 재료 수: {ingredientsToCheck.Length}";
    }
}
```
- 특정 Station의 현재 재료 목록을 가져오고, 지정 재료가 있는지 검사하여 결과를 Inspector에 표시.

### RecipePreviewer.cs
```
public static class RecipePreviewer
{
    public static RecipePreviewResult GetPreview(List<string> currentIngredients, bool showDebug = false)
    {
        var matches = RecipeManager.Instance.FindMatchingTodayRecipes(currentIngredients);
        var best = matches.OrderByDescending(m => m.MatchRatio).First();

        var availableFoodIds = matches
            .SelectMany(m => m.recipe.ingredients)
            .Distinct()
            .ToList();

        return new RecipePreviewResult
        {
            BestMatchedRecipeText = $"{best.recipe.displayName} ({best.matchedCount}/{best.totalRequired})",
            CookedIngredient = best.recipe,
            AvailableFoodIds = availableFoodIds
        };
    }
}
```
- 현재 재료 목록에 기반해 가장 잘 맞는 레시피를 미리 보여주는 유틸리티.

### StationRecipeAssigner.cs
```
public static class StationRecipeAssigner
{
    [MenuItem("Tools/Editor/Assign Recipes To Stations")]
    public static void AssignRecipes()
    {
        var allStationData = Resources.LoadAll<StationData>("Datas/Station");
        var allFoodData = Resources.LoadAll<FoodData>("Datas/Food");

        foreach (var station in allStationData)
        {
            var matchingFoods = allFoodData
                .Where(food => food.stationType.Contains(station.stationType))
                .ToList();

            station.availableRecipes = matchingFoods;
            EditorUtility.SetDirty(station);
        }
        AssetDatabase.SaveAssets();
    }
}
```

