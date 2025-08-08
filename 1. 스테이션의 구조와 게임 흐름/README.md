# 🎮스테이션의 구조와 게임 흐름
- Unity 기반 레스토랑 시뮬레이션에서 게임 진행중 스테이션을 담당하는 시스템입니다.
- StationManager가 전체적인 큰 흐름을 제어하며 각각의 설비들이 기능을 담당합니다.

 ## 📂폴더 구조
 ```
 GameFlow/
 ├── StationManager.cs        # 게임 전역에서 모든 스테이션(조리대, 재료대 등)의 배치·저장·복원·해금·관리를 담당하는 매니저
 ├── RecipeManager.cs         # 모든 음식 데이터(FoodData)와 레시피의 로딩, 검색, 매칭 로직을 관리
 ├── BaseStation.cs           # 플레이어가 재료를 올려놓고, 조리를 진행하고, 결과물을 픽업하는 기본형 조리 설비의 동작을 처리
 ├── AutomaticStation.cs      # 자동 조리 스테이션 기능을 담당
 ├── PassiveStation.cs        # 플레이어가 직접 상호작용(J키 등)해서 조리를 진행하는 스테이션
 ├── IngredientStation.cs     # 플레이어가 상호작용하면 즉시 재료를 생성해 주는 간단한 재료 공급 설비
 ├── Shelf.cs                 # 플레이어가 올바른 재료나 메뉴를 올려놓으면 그 상태를 유지하는 저장형 스테이션
 ├── Trashcan.cs              # 쓰레기통의 기능 동작을 담당
 ├── FoodDisplay.cs           # 게임 내에서 보여지는 음식 오브젝트(재료나 완성 음식)의 상호작용 처리
 ├── Station.cs               # 스테이션(조리기기) 데이터를 담는 순수 데이터 클래스
 ├── StationData.cs           # 주방 설비(스테이션)의 속성을 ScriptableObject 데이터로 정의
 └── VisualObjectFactory.cs   # 재료나 음식 같은 시각적 게임 오브젝트를 생성하는 유틸리티 클래스
```

## 🎯 핵심 기능의 설계 특징
### 1. StationManager
- Singleton 패턴
  → 전역에서 하나의 인스턴스만 존재, 어디서나 접근 가능.

- 타일맵 기반 배치 관리
  - TilemapController의 GridCell / StorageGridCell 구조와 연결.
  - 각 셀에 어떤 스테이션이 있는지 stationGroups 리스트로 관리.

- 데이터베이스화
  - StationData를 ID 기준으로 딕셔너리에 저장.
  - 프리팹도 ID-GameObject 맵핑(stationPrefabDatabase).

- 저장/복원 기능
  - 게임 종료, 페이즈 변경 시 현재 배치 상태를 JSON으로 저장.
  - 재시작 또는 특정 튜토리얼 조건에서 복원(RestoreStations).

- 게임 로직 지원
  - 특정 스테이션의 재료 조건 확인(CheckIngredients)
  - 결과물 존재 여부(CheckObjectOnStation)
  - 특정 셀 위치 확인(CheckStationPlacedOnCell)

### 2. RecipeManager
- Singleton 패턴
  → 전역 레시피 참조 일원화.

- FoodDatabase
  - FoodData를 ID로 매핑하여 빠른 검색.
  - Resources.LoadAll로 모든 데이터 로드.

- 원재료 추출
  - 메뉴나 레시피에서 최종 원재료까지 재귀 탐색(CollectRawIngredients).

- 레시피 매칭 시스템
  - 오늘의 메뉴(todayMenus) + 예외 레시피(AlwaysIncludedRecipeIds) 기반 후보군 생성.
  - 플레이어가 넣은 재료 리스트와 후보 레시피를 비교하여 일치율 계산(MatchRatio).
  - AND/OR 조건 분기 처리 (재료 수 1개 → OR, 그 이상 → AND).

- 정렬 기준
  - 일치율 → 일치 개수 순으로 정렬해 가장 적합한 레시피 선택 가능.
 
### 3. BaseStation
- IPlaceableStation, IMovableStation 인터페이스 구현
  → 배치 가능하고, 플레이어와 상호작용 가능한 공통 규격 유지.

- 재료 관리
  - 현재 올려진 재료 ID(currentIngredients)
  - 재료 오브젝트(placedIngredients)
  - 가능한 레시피 후보(availableMatchedRecipes)

- 레시피 매칭
  - RecipeManager.FindMatchingTodayRecipes() 호출로 현재 재료 조합 분석.
  - 가장 높은 일치율 레시피(cookedIngredient)를 선택.

- 조리 로직
  - 자동(Automatic)과 패시브(Passive) 모드 지원.
  - CookingTimer와 StationTimerController로 시간 제어 & UI 표시.

- 결과물 생성
  - 타이머 완료 시 ProcessCookingResult() 실행.
  - 결과물 시각화 및 이벤트 전송.

- 픽업 처리
  - 완성품 픽업 시 상태 초기화.
  - 재료 픽업 시 마지막 재료만 제거하고 UI 갱신.

- 시각 효과 & UI
  - 외곽선(OutlineShaderController)
  - 슬롯 아이콘(FoodSlotIconDisplay) 제어.

- 이벤트 연동
  - EventBus로 사운드, 게임 이벤트 발생.

## 📌 핵심 코드 예시
### 1. StationManager – 스테이션 복원 로직
```
public void RestoreStations(List<StationSaveInfo> infos, GamePhase currentPhase)
{
    stationGroups.Clear();
    for (int i = 0; i < tilemapController.gridCells.Count; i++)
        stationGroups.Add(new StationGroup());

    foreach (var info in infos)
    {
        StationData data = FindStationDataById(info.id);
        if (!data || !stationPrefabDatabase.TryGetValue(info.id, out GameObject prefab))
            continue;

        GameObject cell = tilemapController.gridCells
            .FirstOrDefault(c => c.name == info.gridCellName);
        if (!cell) continue;

        GameObject instance = Instantiate(prefab, cell.transform);
        instance.transform.localPosition = Vector3.zero;
        instance.GetComponent<BaseStation>()?.Initialize(data);

        bool isStorageCell = cell.GetComponent<StorageGridCell>();
        bool shouldActivate = currentPhase == GamePhase.EditStation || currentPhase == GamePhase.Day;
        instance.SetActive(isStorageCell ? shouldActivate : true);

        int index = tilemapController.gridCells.IndexOf(cell);
        stationGroups[index].station = instance;
    }
}
```
- 저장된 ID를 기반으로 프리팹을 찾아서 해당 GridCell에 배치.
- 스토리지 셀과 영업 셀에 따라 활성화 여부 결정.

### 2. RecipeManager – 재료와 레시피 매칭
```
public List<RecipeMatchResult> FindMatchingTodayRecipes(List<string> ingredientIds)
{
    var todayMenuList = MenuManager.Instance?.GetTodayFoodData();
    if (todayMenuList == null || todayMenuList.Count == 0)
        return new List<RecipeMatchResult>();

    var exceptionRecipes = FoodDatabase.Values
        .Where(food => AlwaysIncludedRecipeIds.Contains(food.id))
        .Where(food => food.ingredients?.Length > 0)
        .ToList();

    var finalCandidates = todayMenuList.Concat(exceptionRecipes).Distinct().ToList();

    return FindMatchingRecipes(finalCandidates, ingredientIds);
}
```
- 오늘의 메뉴 + 항상 포함해야 하는 예외 레시피를 결합.
- 이후 FindMatchingRecipes로 일치율 계산.

### 3. BaseStation – 재료 배치와 조리 시작
```
public void PlaceObject(FoodData data)
{
    if (!CanPlaceIngredient(data))
        return;

    iconDisplay.UseSlot(data.foodType);
    RegisterIngredient(data);
    UpdateCandidateRecipes();

    bool hasAllIngredients = cookedIngredient != null &&
        cookedIngredient.ingredients.All(id => currentIngredients.Contains(id));

    if (hasAllIngredients && stationData.workType == WorkType.Automatic)
    {
        StartCooking();
    }
    else if (hasAllIngredients && stationData.workType == WorkType.Passive)
    {
        timerController?.ShowPassiveCookingState();
    }
    else
    {
        timerController?.gameObject.SetActive(false);
    }
}
```
- 재료 등록 후 현재 조합이 레시피 조건을 만족하면 조리 시작.
- 자동/패시브 모드별로 처리 방식 분기.

### 4. BaseStation – 레시피 후보 갱신
```
private void UpdateCandidateRecipes()
{
    var currentIds = currentIngredients;
    var matches = RecipeManager.Instance.FindMatchingTodayRecipes(currentIds);

    availableMatchedRecipes.Clear();
    availableFoodIds.Clear();
    cookedIngredient = null;

    if (matches?.Count > 0)
    {
        var best = matches
            .OrderByDescending(m => m.MatchRatio)
            .ThenByDescending(m => m.matchedCount)
            .FirstOrDefault();

        availableMatchedRecipes = matches.Select(m => m.recipe).ToList();
        availableFoodIds = availableMatchedRecipes
            .SelectMany(recipe => recipe.ingredients)
            .Distinct()
            .ToList();

        if (best?.MatchRatio >= 1f)
        {
            cookedIngredient = best.recipe;
        }
    }
}
```
- 현재 재료로 만들 수 있는 레시피를 전부 검색 후 최고 일치율 레시피를 선택.
- 다음에 넣을 수 있는 재료 ID 리스트(availableFoodIds)도 업데이트.
