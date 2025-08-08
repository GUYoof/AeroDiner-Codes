using UnityEngine;

/// <summary>
/// 플레이어가 상호작용하면 재료를 생성해주는 스테이션
/// </summary>
public class IngredientStation : MonoBehaviour, IInteractable, IMovableStation
{
    [SerializeField] private StationData stationData;
    public StationData StationData => stationData;
    public Transform GetTransform() => transform;

    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

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

    /// <summary>
    /// 플레이어와 상호작용하면 재료 생성 + 즉시 인벤토리로 들어감
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (!selectedIngredient || !playerInventory)
        {
            if (showDebugInfo) Debug.LogError("필수 데이터가 누락되었습니다.");
            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            if (showDebugInfo) Debug.Log("플레이어가 이미 아이템을 들고 있음");
            return;
        }
        switch (interactionType)
        {
            case InteractionType.Pickup:
                CreateIngredient(playerInventory);
                break;

            case InteractionType.Use:
                // 향후 확장용: 예) 레시피 변경, UI 열기 등
                if (showDebugInfo) Debug.Log("Use 상호작용은 현재 정의되어 있지 않음");
                break;
        }
    }

    private void CreateIngredient(PlayerInventory playerInventory)
    {
        string displayName = selectedIngredient.foodName;
        Sprite displayIcon = selectedIngredient.foodIcon;

        Transform slot = playerInventory.GetItemSlotTransform();
        GameObject pickupObj = VisualObjectFactory.CreateIngredientVisual(
            parent: slot,
            name: displayName,
            icon: displayIcon
        );

        if (!pickupObj)
        {
            if (showDebugInfo) Debug.LogError("비주얼 오브젝트 생성 실패");
            return;
        }

        var display = pickupObj.AddComponent<FoodDisplay>();
        display.foodData = selectedIngredient;

        var col = pickupObj.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        var rb = pickupObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        playerInventory.SetHeldItem(display);
        if (showDebugInfo) Debug.Log($"[{name}] {displayName} 생성 → 즉시 플레이어 손으로 이동");
    }


    public bool PlaceIngredient(FoodData data)
    {
        if (data == null || selectedIngredient == null)
        {
            if (showDebugInfo) Debug.Log("유효하지 않은 재료입니다.");
            return false;
        }

        if (data.id == selectedIngredient.id)
        {
            if (showDebugInfo) Debug.Log("재료 일치: 내려놓기 허용");
            return true;
        }
        else
        {
            if (showDebugInfo) Debug.Log("재료 불일치: 내려놓기 차단");
            return false;
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