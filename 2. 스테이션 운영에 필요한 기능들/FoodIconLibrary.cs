using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FoodType별 아이콘 스프라이트를 반환하는 유틸
/// </summary>
public static class FoodIconLibrary
{
    private static Dictionary<FoodType, Sprite> iconDict;

    public static void Initialize(Dictionary<FoodType, Sprite> map)
    {
        iconDict = map;
    }

    public static Sprite GetIcon(FoodType type)
    {
        if (iconDict != null && iconDict.TryGetValue(type, out var sprite))
            return sprite;

        return null;
    }
}
