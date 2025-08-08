using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GridCell의 상태를 판단하고 Sprite 및 Outline을 제어
/// </summary>
public class GridCellStatus : MonoBehaviour, IInteractable
{
    public GameObject placementStation;

    private SpriteRenderer sr;
    private OutlineShaderController outline;

    private Material placeableMaterial;
    private Material notPlaceableMaterial;
    private Material selectMaterial;

    private bool isSelected = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        outline = GetComponent<OutlineShaderController>();

        FindPlacementStation();
    }

    /// <summary>
    /// 자식 오브젝트 중 IMovableStation 인터페이스를 구현한 오브젝트를 모두 수집
    /// </summary>
    private void FindPlacementStation()
    {
        placementStation = null;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<IMovableStation>() != null)
            {
                placementStation = child.gameObject;
                break; // 하나만 찾으면 끝
            }
        }
    }

    /// <summary>
    /// 머티리얼들을 설정
    /// </summary>
    public void SetMaterials(Material placeable, Material notPlaceable, Material select)
    {
        placeableMaterial = placeable;
        notPlaceableMaterial = notPlaceable;
        selectMaterial = select;
    }

    /// <summary>
    /// 선택 상태를 설정, true면 selectMaterial을 적용
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateStatus();
    }

    /// <summary>
    /// 현재 자식 유무에 따라 배치 가능 여부를 판단하고 상태 업데이트
    /// </summary>
    public void UpdateStatus()
    {
        if (sr == null) return;

        if (isSelected)
        {
            sr.sharedMaterial = selectMaterial;
        }
        else
        {
            bool isPlaceable = !HasStation();
            sr.sharedMaterial = isPlaceable ? placeableMaterial : notPlaceableMaterial;
        }

        // outline은 항상 허용
        outline?.EnableOutline();
    }

    private bool HasStation()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<IMovableStation>() != null)
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    // 에디터에서 강제로 Outline 켜기/끄기 토글 테스트용 함수
    [ContextMenu("Force Enable Outline")]
    public void ForceEnableOutline()
    {
        if (outline != null)
            outline.EnableOutline();
    }

    [ContextMenu("Force Disable Outline")]
    public void ForceDisableOutline()
    {
        if (outline != null)
            outline.DisableOutline();
    }
#endif

    public void SetMaterials(Material placeable, Material notPlaceable)
    {
        placeableMaterial = placeable;
        notPlaceableMaterial = notPlaceable;
    }

    public void Interact(PlayerInventory inventory, InteractionType interactionType) { }
    public void OnHoverEnter()
    {
        outline?.EnableOutline();
    }
    public void OnHoverExit()
    {
        outline?.DisableOutline();
    }
}