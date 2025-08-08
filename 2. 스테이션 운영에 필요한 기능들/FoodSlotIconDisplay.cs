using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 설비 위에 FoodType별 슬롯 아이콘을 표시하고, 재료가 추가되면 해당 아이콘을 순서대로 숨김
/// </summary>
public class FoodSlotIconDisplay : MonoBehaviour
{
    [Header("아이콘 프리팹 (SpriteRenderer 포함)")]
    [SerializeField] private GameObject iconPrefab;

    [Header("아이콘 표시 기준 위치")]
    [SerializeField] private Transform basePosition;

    [Header("아이콘 간 간격")]
    [SerializeField] private float spacing = 0.5f;

    [SerializeField] protected bool showDebugInfo;

    private Dictionary<FoodType, List<GameObject>> iconMap = new();

    public void Initialize(List<SlotDisplayData> slots)
    {
        if (!iconPrefab || !basePosition)
        {
            if (showDebugInfo) Debug.LogError("iconPrefab 또는 basePosition이 설정되지 않았습니다.");
            return;
        }

        // 기존 아이콘 제거
        foreach (var list in iconMap.Values)
            foreach (var obj in list)
                if (obj) Destroy(obj);
        iconMap.Clear();

        float totalWidth = (slots.Count - 1) * spacing;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var type = slot.foodType;
            var iconSprite = slot.placeholderIcon;

            Vector3 pos = basePosition.position + new Vector3(i * spacing - totalWidth / 2f, 1f, 0);
            GameObject icon = Instantiate(iconPrefab, pos, Quaternion.identity, transform);

            var sr = icon.GetComponent<SpriteRenderer>();
            if (sr) sr.sprite = iconSprite;

            if (!iconMap.ContainsKey(type))
                iconMap[type] = new List<GameObject>();

            iconMap[type].Add(icon);
        }
    }

    public void UseSlot(FoodType type)
    {
        if (iconMap.TryGetValue(type, out var list))
        {
            foreach (var icon in list)
            {
                if (icon.activeSelf)
                {
                    icon.SetActive(false);
                    break;
                }
            }
        }
    }

    public void ResetAll()
    {
        foreach (var list in iconMap.Values)
            foreach (var icon in list)
                if (icon) icon.SetActive(true);
    }

    public void ShowSlot(FoodType type)
    {
        if (iconMap.TryGetValue(type, out var list))
        {
            foreach (var icon in list)
            {
                if (!icon.activeSelf)
                {
                    icon.SetActive(true);
                    break;
                }
            }
        }
    }

    public void CloseSlot()
    {
        foreach (var list in iconMap.Values)
            foreach (var icon in list)
                if (icon) icon.SetActive(false);
    }
}