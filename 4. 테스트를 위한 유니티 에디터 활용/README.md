# 테스트를 위한 유니티 에디터 활용
- 스테이션을 구현하면서 기틍을 테스트 하기거나 런타임 중 현재 재료 상태 기반 최적 레시피 및 유효 재료 목록 산출하기 위해 사용

##  📂 폴더 구조
```
/Editor
 ├─ KeyRebindButtonEditor.cs         // 버튼 이름 기반 자동 입력 인덱스 매핑 기능 제공
 ├─ PlacementManagerEditor.cs        // 배치 가능한 셀 표시·테스트용 장애물 생성·설비 배치 시도 기능 제공
 ├─ StationEditor.cs                 // 현재 재료 목록 기반 레시피 미리보기 기능 제공
 ├─ StationIngredientViewerEditor.cs // 특정 스테이션의 현재 재료 정보와 상태를 인스펙터에서 실시간 확인
 ├─ StationManagerEditor.cs          // 지정 폴더의 스테이션 프리팹을 자동 로드하여 할당
 ├─ StationRecipeAssigner.cs         // 모든 스테이션 데이터에 해당 유형 레시피를 일괄 할당하는 메뉴 기능
 ├─ TilemapControllerEditor.cs       // 타일맵 셀 표시·숨김 버튼 제공

/Runtime
 ├─ PrintCurrentIngredients.cs       // StationManager와 연동해 현재 재료 목록과 검사 결과를 유지·출력
 ├─ RecipePreviewer.cs               // 현재 재료 상태 기반 최적 레시피 및 유효 재료 목록 산출
 ├─ TestInventory.cs                 // 플레이어가 들고 있는 아이템을 테스트 환경에서 가상 설정
```

## 🎯 핵심 기능의 설계 특징
- 에디터 확장 자동화
 - /Editor 폴더의 스크립트들은 Unity 에디터 전용으로, 게임 실행 없이도 데이터 세팅과 기능 검증이 가능하도록 설계.
 - 반복 수작업 최소화를 위해 메뉴 버튼(MenuItem)과 인스펙터 버튼을 제공.
 - 스테이션, 레시피, 배치 시스템, 입력 키 매핑 등 다양한 기능을 한 번에 설정 가능.

- 런타임 연동 디버깅
 - /Runtime 폴더의 스크립트들은 실제 플레이 환경에서 동작하며, StationManager나 RecipeManager와 직접 연결.
 - 인게임 데이터 상태를 즉시 분석하고 시각화하여 개발·테스트를 빠르게 진행 가능.

- 데이터·기능 분리 구조
 - /Editor는 개발 편의 및 데이터 준비 중심, /Runtime은 게임 로직 실행 중심.
 - 빌드 시 Editor 전용 스크립트가 포함되지 않도록 Unity의 폴더 구조 규칙을 준수.

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
- 에디터 메뉴에서 클릭 한 번으로 모든 StationData에 레시피를 일괄 매칭하는 코드.

## 🔥에디터 활용 스크린샷
### 1. StationManagerEditor.cs
<img width="489" height="192" alt="image" src="https://github.com/user-attachments/assets/d26e8683-3efb-4d2a-b00e-e5d91d33048a" />

- 버튼 클릭시 자동으로 프리팹을 로드하여 할당
<img width="488" height="801" alt="image" src="https://github.com/user-attachments/assets/509756ee-34db-4a4c-aa8d-7e968e48fdec" />

### 2. StationRecipeAssigner.cs
<img width="481" height="149" alt="image" src="https://github.com/user-attachments/assets/6c686f97-f961-40bf-8174-88e77dff5db8" />
