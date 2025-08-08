using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 모든 FoodData와 레시피를 관리.
/// </summary>
public class RecipeManager : Singleton<RecipeManager>
{
    public Dictionary<string, FoodData> FoodDatabase { get; private set; } = new ();

    [Header("Debug")]
    private bool showDebugInfo;
    
    protected override void Awake()
    {
        base.Awake();
        LoadFoodDatabase();
        
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// 모든 FoodData를 딕셔너리에 로드 (ID, FoodData)
    /// </summary>
    private void LoadFoodDatabase()
    {
        FoodData[] allFoods = Resources.LoadAll<FoodData>(StringPath.FOOD_DATA_PATH);
    
        FoodDatabase.Clear();
        foreach (var food in allFoods)
        {
            if (!string.IsNullOrEmpty(food.id))
            {
                FoodDatabase[food.id] = food;
            }
        }
    
        if (showDebugInfo) Debug.Log($"[MenuManager] {FoodDatabase.Count}개 음식 데이터 로드 완료");
    }

    public FoodData FindFoodDataById(string id)
    {
        return FoodDatabase.GetValueOrDefault(id);
    }

    #region 레시피 원재료 찾기

    /// <summary>
    /// 현재 메뉴들의 모든 원재료 리스트 반환
    /// </summary>
    public List<FoodData> GetAllRecipe(List<Menu> availableMenus)
    {
        HashSet<string> rawIngredientIds = new HashSet<string>();
    
        // 모든 메뉴에 대해 원재료 수집
        foreach (var menu in availableMenus)
        {
            if (menu != null)
            {
                CollectRawIngredients(menu.foodData.id, rawIngredientIds);
            }
        }
    
        // ID를 실제 FoodData로 변환
        List<FoodData> result = new List<FoodData>();
        foreach (string id in rawIngredientIds)
        {
            if (FoodDatabase.TryGetValue(id, out var food))
            {
                result.Add(food);
            }
        }
    
        return result;
    }

    /// <summary>
    /// 특정 음식의 모든 원재료를 재귀적으로 수집
    /// </summary>
    private void CollectRawIngredients(string foodId, HashSet<string> collectedIds)
    {
        if (string.IsNullOrEmpty(foodId) || !FoodDatabase.TryGetValue(foodId, out var food))
            return;

        // 원재료거나 ingredients가 없으면 최종 재료
        if (food.foodType == FoodType.Raw || 
            food.ingredients == null || 
            food.ingredients.Length == 0)
        {
            collectedIds.Add(foodId);
            return;
        }
    
        // ingredients가 있으면 재귀적으로 탐색
        foreach (string ingredientId in food.ingredients)
        {
            CollectRawIngredients(ingredientId, collectedIds);
        }
    }

    #endregion

    #region 레시피 관리

    // 항상 레시피로 포함될 예외 재료 ID 리스트
    private static readonly HashSet<string> AlwaysIncludedRecipeIds = new()
    {
        "f18", "f19", "f20", "f21", "f22", "f23", "f24", "f25"
    };

    /// <summary>
    /// 유저가 선택한 재료 ID 리스트를 기반으로, 후보 레시피 중 일치하는 레시피를 찾아 반환
    /// </summary>
    /// <param name="ingredientIds">유저가 선택한 재료 ID 리스트</param>
    /// <returns>일치율 순으로 정렬된 레시피 결과 리스트</returns>
    public List<RecipeMatchResult> FindMatchingTodayRecipes(List<string> ingredientIds)
    {
        var menuManager = MenuManager.Instance;

        // MenuManager 또는 읽기 전용 todayMenus 속성이 없을 경우 안전하게 빈 결과 반환
        var todayMenuList = menuManager?.GetTodayFoodData();

        if (todayMenuList == null || todayMenuList.Count == 0)
        {
            if (showDebugInfo) Debug.LogWarning("[RecipeManager] MenuManager 또는 TodayMenus가 존재하지 않음");
            return new List<RecipeMatchResult>();
        }
        
        // 예외 재료 레시피도 포함시키기
        var exceptionRecipes = FoodDatabase.Values
            .Where(food => AlwaysIncludedRecipeIds.Contains(food.id))
            .Where(food => food.ingredients != null && food.ingredients.Length > 0)
            .ToList();

        // 최종 레시피 후보 결합
        var finalCandidates = todayMenuList
            .Concat(exceptionRecipes)
            .Distinct()
            .ToList();

        // 기존의 매칭 함수 호출 — 유저 재료 ID와 오늘 메뉴 레시피 간 매칭
        return FindMatchingRecipes(finalCandidates, ingredientIds);
    }

    /// <summary>
    /// 주어진 레시피 후보들 중에서 사용자가 선택한 재료와 얼마나 일치하는지를 판단하여 결과를 반환함
    /// </summary>
    /// <param name="candidateRecipes">검사할 레시피 후보 목록</param>
    /// <param name="ingredientIds">플레이어가 넣은 재료 ID 리스트</param>
    /// <returns>매칭 결과 객체 리스트 (일치율 기준 정렬)</returns>
    public List<RecipeMatchResult> FindMatchingRecipes(List<FoodData> candidateRecipes, List<string> ingredientIds)
    {
        var matches = candidateRecipes
            // 재료가 null이 아니고 최소 1개 이상일 경우에만 처리
            .Where(r => r.ingredients != null && r.ingredients.Length > 0)
            // 각 레시피별로 사용자의 재료와 얼마나 일치하는지 계산
            .Select(r =>
            {
                int matchedCount = r.ingredients.Count(ingredientIds.Contains); // 일치한 개수 계산
                return new RecipeMatchResult
                {
                    recipe = r,
                    matchedCount = matchedCount,
                    totalRequired = r.ingredients.Length
                };
            })
            // 재료 수에 따라 OR / AND 조건 분기
            .Where(r =>
            {
                if (ingredientIds.Count <= 1)
                {
                    return r.matchedCount > 0; // OR 조건
                }

                return ingredientIds.All(id => r.recipe.ingredients.Contains(id)); // AND 조건
            })
            .ToList();

        // 일치 개수 많은 순으로 정렬하여 반환
        return matches
            .OrderByDescending(r => r.MatchRatio)
            .ThenByDescending(r => r.matchedCount)
            .ToList();
    }

    /// <summary>
    /// 개별 레시피와 유저 재료의 일치 결과 정보를 담는 클래스
    /// </summary>
    public class RecipeMatchResult
    {
        public FoodData recipe;           // 해당 레시피 데이터
        public int matchedCount;          // 유저 재료와 일치한 재료 개수
        public int totalRequired;         // 해당 레시피가 요구하는 전체 재료 개수

        // 전체 재료 중 일치한 재료의 비율을 계산
        public float MatchRatio => totalRequired == 0 ? 0f : (float)matchedCount / totalRequired;
    }

    #endregion
}