using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class StationRecipeAssigner
{
    [MenuItem("Tools/Editor/Assign Recipes To Stations")]
    public static void AssignRecipes()
    {
        // 모든 StationData 에셋을 불러오기
        StationData[] allStationData = Resources.LoadAll<StationData>("Datas/Station");
        FoodData[] allFoodData = Resources.LoadAll<FoodData>("Datas/Food");

        int totalAssigned = 0;

        // 스테이션 하나씩 처리
        foreach (var station in allStationData)
        {
            // 매칭되는 레시피 필터링
            List<FoodData> matchingFoods = allFoodData
                .Where(food => food.stationType.Contains(station.stationType))
                .ToList();

            // 레시피 할당
            station.availableRecipes = matchingFoods;
            EditorUtility.SetDirty(station);
            totalAssigned += matchingFoods.Count;

            Debug.Log($"Assigned {matchingFoods.Count} recipes to '{station.stationName}' ({station.stationType})");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Done! Total {totalAssigned} recipes assigned to StationData assets.");
    }
}