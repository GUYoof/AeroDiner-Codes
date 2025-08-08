using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseStation : MonoBehaviour, IPlaceableStation, IMovableStation
{
    public Transform GetTransform() => transform;
    public Transform spawnPoint;
    private CookingTimer cookingTimer;
    [SerializeField] private StationData stationData;
    public StationData StationData => stationData;
    public List<string> currentIngredients = new();
    [SerializeField] protected List<string> availableFoodIds = new();
    [SerializeField] protected string bestMatchedRecipe;
    [SerializeField] protected FoodSlotIconDisplay iconDisplay;
    [SerializeField] protected float slotSpacing = 0.5f;
    [SerializeField] protected StationTimerController timerController; // 타이머 UI 컨트롤러
    [SerializeField] protected bool showDebugInfo;

    private List<FoodData> placedIngredientList = new();             // 실제 등록된 재료의 데이터 목록
    private List<GameObject> placedIngredients = new();              // 화면에 보여지는 재료 오브젝트들
    private List<FoodData> availableMatchedRecipes = new();          // 현재 조건에서 가능한 레시피 리스트
    private OutlineShaderController outline;                         // 외곽선 효과를 제어하는 컴포넌트
    
    protected FoodData cookedIngredient;                               // 조리 완료 시 결과가 되는 레시피
    private FoodData cookedResult;                                     // 조리 완료된 결과물
    protected CookingTimer timer;
    protected bool isCooking = false;

    public string StationId => stationData? stationData.id : string.Empty;
    public bool IsCookingOrWaiting
    {
        get
        {
            // 자동 조리: 타이머가 돌아가고 있으면 true
            if (stationData.workType == WorkType.Automatic)
            {
                return cookedIngredient && timer != null && timer.Remaining > 0f;
            }

            // 패시브 조리: 재료가 모두 충족되면 true
            if (stationData.workType == WorkType.Passive)
            {
                return cookedIngredient &&
                       cookedIngredient.ingredients.All(id => currentIngredients.Contains(id));
            }

            return false;
        }
    }

    private void Awake()
    {
        timer = new CookingTimer(cookedIngredient);
        timerController.gameObject.SetActive(false);

        outline = GetComponent<OutlineShaderController>(); // 외곽선 컴포넌트 연결
    }

    private void Start()
    {
        if (stationData != null && stationData.slotDisplays != null)
        {
            iconDisplay.Initialize(stationData.slotDisplays); // 슬롯 전체 전달
            iconDisplay.ResetAll();
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("stationData 또는 slotDisplays가 할당되지 않았습니다.");
        }
    }
    
    public void Initialize(StationData data)
    {
        if (!data) return;
        
        stationData = data; 
        
        if (TryGetComponent<SpriteRenderer>(out var sr) && StationData.stationIcon != null)
        {
            sr.sprite = StationData.stationIcon;
        }
    }

    /// <summary>
    /// 플레이어가 재료를 놓았을 때 호출됨
    /// </summary>
    public void PlaceObject(FoodData data)
    {
        // 유효하지 않은 데이터거나 등록 불가한 재료일 경우 경고 출력 후 무시
        if (!CanPlaceIngredient(data))
        {
            if (showDebugInfo) Debug.LogWarning($"'{data.foodName}'은 등록할 수 없는 재료입니다.");
            return;
        }

        iconDisplay.UseSlot(data.foodType); // 아이콘 끄기

        RegisterIngredient(data);     // 재료 등록 및 선택 처리
        UpdateCandidateRecipes();     // 현재 재료 조합으로 가능한 레시피 탐색

        // 모든 필요한 재료가 충족되었는지 확인 후 조리 시작
        bool hasAllIngredients = cookedIngredient != null && cookedIngredient.ingredients.All(id => currentIngredients.Contains(id));

        if (hasAllIngredients && stationData.workType == WorkType.Automatic)
        {
            StartCooking();
        }
        else if (hasAllIngredients && stationData.workType == WorkType.Passive)
        {
            // 패시브 조리: 조리는 시작하지 않지만, 픽업 금지 상태로 진입
            if (showDebugInfo) Debug.Log("[Player] 조리 중인 스테이션 → 픽업 금지");

            timerController?.ShowPassiveCookingState();
        }
        else
        {
            // 조리 조건 불충분 시 아무 타이머도 시작하지 않음
            timerController?.gameObject.SetActive(false);
            if (showDebugInfo) Debug.Log("조건에 맞는 레시피가 부족하여 대기 중...");
        }
    }

    public void Interact(PlayerInventory inventory, InteractionType interactionType)
    {
        
    }
    
    /// <summary>
    /// 특정 재료를 등록할 수 있는지 검증
    /// </summary>
    public bool CanPlaceIngredient(FoodData data)
    {
        // 중복 방지
        if (currentIngredients.Contains(data.id))
        {
            if (showDebugInfo) Debug.LogWarning($"[Station] 중복 재료: {data.id} 이미 추가됨");
            return false;
        }

        // 첫 번째 재료
        if (currentIngredients.Count == 0)
        {
            bool typeMatch = data.stationType.Any(type => type == stationData.stationType);

            if (showDebugInfo) Debug.Log($"[Debug] Food station types: {string.Join(",", data.stationType)}");
            if (showDebugInfo) Debug.Log($"[Debug] Station type: {stationData.stationType}");

            return typeMatch;
        }

        // 이후에는 matchedRecipeNames에 포함된 재료만 허용
        if (availableFoodIds.Contains(data.id))
        {
            if (showDebugInfo) Debug.Log($"[Station] '{data.id}' matchedRecipeNames에 포함 → 등록 가능");
            return true;
        }

        if (showDebugInfo) Debug.Log($"[Station] '{data.id}'은 현재 어떤 레시피에도 포함되지 않음");
        return false;
    }

    /// <summary>
    /// 재료를 등록하고 시각화 목록 생성
    /// </summary>
    private void RegisterIngredient(FoodData data)
    {
        if (data == null)
        {
            if (showDebugInfo) Debug.LogError("[RegisterIngredient] 등록 시도된 데이터가 null입니다!");
            return;
        }

        // 시각 오브젝트 생성
        if (showDebugInfo) Debug.Log($"[RegisterIngredient] ID: {data.id}, Name: {data.foodName}");
        currentIngredients.Add(data.id);
        placedIngredientList.Add(data);

        GameObject visual = VisualObjectFactory.PlaceIngredientVisual(transform, data.foodName, data.foodIcon);
        if (visual)
        {
            var display = visual.AddComponent<FoodDisplay>();
            display.foodData = data;
            display.originPlace = this;

            placedIngredients.Add(visual); // 오브젝트 추적 리스트에 등록

            if (showDebugInfo) Debug.Log($"'{data.foodName}' 시각 오브젝트 생성 및 배치 완료");
            EventBus.Raise(GameEventType.StationUsed, StationData.id);
        }
    }

    /// <summary>
    /// 현재 스테이션에 놓인 재료 목록을 기반으로 가능한 레시피 후보를 탐색하고
    /// 가장 일치하는 요리를 선정함
    /// </summary>
    private void UpdateCandidateRecipes()
    {
        Debug.Log($"[UpdateCandidateRecipes] currentIngredients: {string.Join(", ", currentIngredients)}");

        // 현재 재료 ID 목록 가져오기
        var currentIds = currentIngredients;

        // 오늘 레시피 중 매칭되는 목록 탐색
        var matches = RecipeManager.Instance.FindMatchingTodayRecipes(currentIds);

        // 초기화
        availableMatchedRecipes = new();
        availableFoodIds = new();
        cookedIngredient = null;

        if (matches is { Count: > 0 })
        {
            // 가장 일치율 높은 레시피 선정
            var best = matches
                .OrderByDescending(m => m.MatchRatio)
                .ThenByDescending(m => m.matchedCount)
                .FirstOrDefault();

            availableMatchedRecipes = matches
                .Select(m => m.recipe)
                .ToList();

            availableFoodIds = availableMatchedRecipes
                .SelectMany(recipe => recipe.ingredients)
                .Distinct()
                .ToList();

            if (best?.MatchRatio >= 1f)
            {
                cookedIngredient = best.recipe;
                bestMatchedRecipe = $"{best.recipe.id} ({best.matchedCount}/{best.totalRequired})";

                if (showDebugInfo) Debug.Log($"완전 일치 — '{cookedIngredient.foodName}' 선택됨");
            }
            else
            {
                bestMatchedRecipe = "요리 대기 중 (재료 부족)";
                if (showDebugInfo) Debug.Log("아직 요리 조건 불충분");
            }
        }
        else
        {
            bestMatchedRecipe = "조건에 맞는 레시피 없음";
            if (showDebugInfo) Debug.Log("조건에 맞는 레시피가 없습니다.");
        }
    }

    /// <summary>
    /// 조리를 시작하고 타이머 설정
    /// </summary>
    public void StartCooking()
    {
        // 만약 타이머가 없거나 0이면 새로 시작
        if (timer == null || timer.Remaining <= 0f)
        {
            timer = new CookingTimer(cookedIngredient);
            timer.Start();                                                              // Duration으로 리셋 시작
            if (showDebugInfo) Debug.Log("[StartCooking] 새 타이머 시작");
        }
        else if (!timer.IsRunning)
        {
            timer.Start(timer.Remaining);                                               // 중단 시점에서 이어서 시작
            if (showDebugInfo) Debug.Log($"[StartCooking] 이어서 시작 / 남은 시간: {timer.Remaining:F2}");
        }

        if (timerController)
            timerController.gameObject.SetActive(true);                                  // UI 활성화

        if (stationData)
        {
            var sfx = StationSFXResolver.GetSFXFromStationData(stationData);

            // 루프 사운드는 자동인 경우만 재생
            if (stationData.workType == WorkType.Automatic)
            {
                EventBus.PlayLoopSFX(sfx);
            }
            else if (stationData.workType == WorkType.Passive)
            {
                EventBus.PlayLoopSFX(sfx);
            }
        }
    }

    /// <summary>
    /// 조리 완료 시 결과물 생성
    /// </summary>
    protected void ProcessCookingResult()
    {
        cookedResult = cookedIngredient; // 결과물 저장

        ClearPlacedObjects();

        if (!cookedIngredient)
        {
            if (showDebugInfo) Debug.LogWarning("조리 결과 레시피가 없습니다.");
            return;
        }

        if (showDebugInfo) Debug.Log($"조리 완료: '{cookedIngredient.foodName}' 생성");

        GameObject result = VisualObjectFactory.CreateIngredientVisual(transform, cookedIngredient.foodName, cookedIngredient.foodIcon);
        
        var sfx = StationSFXResolver.GetSFXFromStationData(stationData);
        EventBus.StopLoopSFX(sfx);
        Invoke(nameof(PlayCookingFinishSound), 0.2f);

        if (result)
        {
            var display = result.AddComponent<FoodDisplay>();
            display.foodData = cookedIngredient;
            display.originPlace = this;

            EventBus.Raise(GameEventType.StationUsed, StationData.id);
        }
    }

    private void PlayCookingFinishSound()
    {
        EventBus.PlaySFX(SFXType.DoneCooking);
    }
    
    /// <summary>
    /// 스테이션을 초기 상태로 리셋
    /// </summary>
    protected void ResetStation()
    {
        cookedIngredient = null;
        timer = new CookingTimer(cookedIngredient);
        ClearPlacedObjects();                      // 오브젝트 제거
        var sfx = StationSFXResolver.GetSFXFromStationData(stationData);
        EventBus.StopLoopSFX(sfx);                 // 사운드 중지

        // 두 컬렉션 모두 초기화
        currentIngredients.Clear();
        placedIngredientList.Clear();

        if (timerController)
            timerController.gameObject.SetActive(false);
    }

    /// <summary>
    /// 화면에 놓인 재료 오브젝트 제거
    /// </summary>
    private void ClearPlacedObjects()
    {
        foreach (var obj in placedIngredients)
            if (obj) Destroy(obj);
        placedIngredients.Clear();
    }

    /// <summary>
    /// 플레이어가 결과물을 픽업할 때 호출됨
    /// </summary>
    public void OnPlayerPickup()
    {
        StartCoroutine(HandlePickup());            // 픽업 처리
    }

    /// <summary>
    /// 플레이어가 재료 또는 요리 결과물을 픽업할 때 실행되는 코루틴 핸들러
    /// 조리된 결과물이 있는 경우: 아이콘 초기화 후 종료
    /// 조리되지 않은 경우: 기존 재료 오브젝트 제거 후 딜레이를 두고, 선택된 재료를 시각화 오브젝트로 재생성
    /// 해당 재료가 등록된 재료인지 확인한 뒤, UI 상태 및 아이콘 복구를 수행
    /// 프레임 딜레이를 통해 생성/삭제 타이밍 간 충돌을 방지하고 상호작용 안정성을 확보
    /// </summary>

    private IEnumerator HandlePickup()
    {
        // 1. cookedIngredient 존재 → 결과물 꺼냄
        if (cookedResult != null)
        {
            if (showDebugInfo) Debug.Log($"[Pickup] 결과물 픽업: {cookedResult.foodName}");

            iconDisplay?.ResetAll();
            ClearPlacedObjects();
            currentIngredients.Clear();

            foreach (var obj in placedIngredients)
            {
                if (obj != null) Destroy(obj);
            }
            placedIngredients.Clear(); 
            placedIngredientList.Clear();

            EventBus.Raise(GameEventType.PlayerPickedUpItem, cookedResult);

            cookedResult = null;
            UpdateCandidateRecipes();
            yield break;
        }

        // 2. 일반 재료 처리
        if (placedIngredientList.Count == 0)
        {
            if (showDebugInfo) Debug.LogWarning("[Pickup] 등록된 재료가 없습니다.");
            yield break;
        }

        FoodData targetData = placedIngredientList[^1];
        placedIngredientList.RemoveAt(placedIngredientList.Count - 1);
       
        int index = currentIngredients.IndexOf(targetData.id);
        if (index >= 0)
            currentIngredients.RemoveAt(index);
        iconDisplay?.ShowSlot(targetData.foodType);

        GameObject targetObject = null;
        for (int i = placedIngredients.Count - 1; i >= 0; i--)
        {
            var fd = placedIngredients[i]?.GetComponent<FoodDisplay>();
            if (fd != null && fd.foodData == targetData)
            {
                targetObject = placedIngredients[i];
                placedIngredients.RemoveAt(i);
                break;
            }
        }

        if (targetObject != null)
        {
            var food = targetObject.GetComponent<FoodDisplay>();
            if (food != null)
            {
                AttachFoodIcon(targetObject, food.foodData);
                EventBus.Raise(GameEventType.PlayerPickedUpItem, food.foodData);
            }
        }

        if (currentIngredients.Count > 0)
            UpdateCandidateRecipes();
        else
        {
            cookedIngredient = null;
            bestMatchedRecipe = "조건에 맞는 레시피 없음";
        }

        yield return null;
    }

    /// <summary>
    /// 전달된 GameObject에 해당 FoodData의 아이콘을 SpriteRenderer로 표시
    /// 아이콘이 없을 경우 회색으로 표시
    /// </summary>
    /// <param name="obj">아이콘을 부착할 대상 GameObject</param>
    /// <param name="foodData">표시할 음식 데이터(FoodData)</param>
    public static void AttachFoodIcon(GameObject obj, FoodData foodData)
    {
        if (!obj || !foodData) return;

        // 대상 객체에서 SpriteRenderer 컴포넌트를 가져 옴
        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer)
        {
            // 음식 데이터에 아이콘이 존재할 경우 해당 스프라이트를 설정
            renderer.sprite = foodData.foodIcon;

            // 만약 아이콘이 null이라면 시각적으로 구분하기 위해 색상을 회색으로 설정
            if (!foodData.foodIcon)
                renderer.color = Color.gray;
        }
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
