using System.Collections.Generic;
using UnityEngine;

public enum StationType
{
    // 조리기구 (수동형 + 자동형 공통)
    CuttingBoard,    // 도마
    FryingPan,       // 프라이팬
    Pot,             // 냄비
    Grater,
    Grinding,
    Mixing,
    Oven,            // 오븐
    Kneading,

    // 기타 스테이션
    IngredientBox,   // 재료 박스
    Fridge,          // 냉장고 (재료)
    Shelf,           // 선반
    TrashCan,        // 쓰레기통
    None             // 빈 공간
}

public enum WorkType
{
    None,
    Automatic,
    Passive
}

[CreateAssetMenu(fileName = "StationData", menuName = "CookingGame/StationData")]
public class StationData : ScriptableObject
{
    [Header("설비 정보")]
    public string id;
    public string stationName;
    public string displayName; 
    public StationType stationType;
    public WorkType workType ;
    public Sprite stationIcon;
    public string description;

    // 해당 스테이션에서 지원하는 레시피 목록
    public List<FoodData> availableRecipes;

    [Header("Ingredient Slot Settings")]
    public int ingredientSlots = 3;                 // 슬롯 개수
    public List<SlotDisplayData> slotDisplays; // 슬롯별 타입과 아이콘 설정
}

[System.Serializable]
public class SlotDisplayData
{
    public FoodType foodType;             // 슬롯이 담당하는 재료 타입
    public Sprite placeholderIcon;        // 슬롯에 표시할 아이콘
}
