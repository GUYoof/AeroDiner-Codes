using UnityEngine;

/// <summary>
/// 플레이어가 올바른 재료(또는 메뉴)를 선반에 놓으면 저장되는 스테이션
/// </summary>
public class Shelf : MonoBehaviour, IPlaceableStation, IMovableStation
{
    public Transform GetTransform() => transform;

    [SerializeField] private StationData stationData;
    public StationData StationData => stationData;
    [Header("생성할 Food/Menu SO")]
    public FoodData currentData; // 선반에 올려진 음식 데이터

    [Header("비주얼 오브젝트 생성 위치")]
    public Transform spawnPoint;

    [SerializeField] protected bool showDebugInfo;
    
    private OutlineShaderController outline;

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();

        string objName = gameObject.name;
        string resourcePath = $"Datas/Station/{objName}Data";

        // SO 로드
        StationData data = Resources.Load<StationData>(resourcePath);
        if (data != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = data.stationIcon;   // StationData에 있는 아이콘 사용
            }
        }
    }

    // 선반 자체는 직접 Interact 불필요
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType) { }

    // IPlaceableStation 인터페이스 구현
    public void PlaceObject(FoodData data)
    {
        // 이미 올려진 게 있으면 차단
        if (currentData != null)
        {
            if (showDebugInfo) Debug.Log("이미 항목이 배치되어 있습니다.");
            return;
        }

        // 상태 갱신
        currentData = data;

        // 비주얼 생성
        CreateIngredientDisplay(data);
    }

    // FoodData 전반을 검사
    public bool CanPlaceIngredient(FoodData data)
    {
        if (currentData != null)
        {
            if (showDebugInfo) Debug.Log("[Shelf] 이미 항목이 배치되어 있습니다.");
            return false;
        }

        if (showDebugInfo) Debug.Log($"[Shelf] '{data.displayName}' 배치 가능");
        return true;
    }

    /// <summary>
    /// rawData(FoodData or MenuData) 기반으로 VisualObjectFactory 사용해 오브젝트 생성
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        if (data == null || spawnPoint == null)
        {
            if (showDebugInfo) Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        // VisualObjectFactory 호출
        GameObject obj = VisualObjectFactory.CreateIngredientVisual(
            parent: spawnPoint,
            name: data.foodName,
            icon: data.foodIcon
        );
        if (obj == null) return null;

        // FoodDisplay 세팅 (원본 데이터만 연결)
        var display = obj.AddComponent<FoodDisplay>();
        display.foodData = data;
        display.originPlace = this;
        return obj;
    }

    /// <summary>
    /// 플레이어가 재료를 들 때 호출됨.
    /// 스테이션에 남아있는 재료 오브젝트들을 제거하고 상태를 초기화
    /// </summary>
    public void OnPlayerPickup()
    {
        currentData = null;
        if (showDebugInfo) Debug.Log("플레이어가 재료를 들었고, 스테이션이 초기화되었습니다.");
    }

    public void OnHoverEnter()
    {
        outline?.EnableOutline();
    }

    public void OnHoverExit()
    {
        outline?.DisableOutline();
    }
}