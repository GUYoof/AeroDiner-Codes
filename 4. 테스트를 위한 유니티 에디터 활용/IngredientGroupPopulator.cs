using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class IngredientGroupPopulator
{
    [MenuItem("Tools/Editor/Populate Ingredient Group")]
    public static void PopulateIngredientGroup()
    {
        // IngredientGroup 에셋 불러오기
        var group = AssetDatabase.LoadAssetAtPath<IngredientSOGroup>("Assets/03.Datas/SOGroup/IngredientGroup.asset");
        if (group == null)
        {
            Debug.LogError("IngredientGroup.asset 파일을 찾을 수 없습니다!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:FoodData", new[] {"Resource/Datas/Food"});
        var validTypes = new HashSet<FoodType> {
            FoodType.Raw,
            FoodType.Water,
            FoodType.DryNoodle,
            FoodType.Cheese,
            FoodType.Chili,
            FoodType.Capsicum,
            FoodType.Pepper
        };
        var entries = new List<IngredientSOGroup.Entry>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var food = AssetDatabase.LoadAssetAtPath<FoodData>(path);

            if (food != null && validTypes.Contains(food.foodType))
            {
                entries.Add(new IngredientSOGroup.Entry
                {
                    ingredientData = food
                });
            }
        }

        group.ingredientList = entries.ToArray();
        EditorUtility.SetDirty(group);
        AssetDatabase.SaveAssets();

        Debug.Log($"IngredientGroup에 {entries.Count}개의 재료가 추가되었습니다.");
    }

}