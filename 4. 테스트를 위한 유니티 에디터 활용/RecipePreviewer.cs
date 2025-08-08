using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 현재 재료 목록에 기반해 최적의 레시피 미리보기 결과를 반환하는 유틸리티
/// </summary>
public static class RecipePreviewer
{
    public static RecipePreviewResult GetPreview(List<string> currentIngredients, bool showDebug = false)
    {
        if (RecipeManager.Instance == null)
        {
            if (showDebug) Debug.LogWarning("[RecipePreviewer] RecipeManager 인스턴스가 없음");
            return RecipePreviewResult.None("레시피 매니저 없음");
        }

        var matches = RecipeManager.Instance.FindMatchingTodayRecipes(currentIngredients);
        if (matches == null || matches.Count == 0)
        {
            if (showDebug) Debug.Log("[RecipePreviewer] 일치하는 레시피 없음");
            return RecipePreviewResult.None("매칭되는 레시피 없음");
        }

        var best = matches
            .OrderByDescending(m => m.MatchRatio)
            .ThenByDescending(m => m.matchedCount)
            .First();

        var availableFoodIds = matches
            .SelectMany(m => m.recipe.ingredients)
            .Distinct()
            .ToList();

        if (showDebug)
        {
            var debugList = string.Join("\n", availableFoodIds.Select(id => "- " + id));
            Debug.Log("[RecipePreviewer] 유효 재료 ID 목록\n" + debugList);
        }

        return new RecipePreviewResult
        {
            BestMatchedRecipeText = $"{best.recipe.displayName} ({best.matchedCount}/{best.totalRequired})",
            CookedIngredient = best.recipe,
            AvailableFoodIds = availableFoodIds
        };
    }
}
