using UnityEngine;

/// <summary>
/// 테스트용으로 Station 오브젝트를 들고 있는 상태를 흉내냄
/// </summary>
public class TestInventory : MonoBehaviour
{
    [SerializeField] private GameObject testHeldItem;

    public GameObject GetHeldItem()
    {
        return testHeldItem;
    }

    // 테스트 중에 런타임에서 다른 오브젝트로 교체하고 싶다면 사용
    public void SetHeldItem(GameObject newItem)
    {
        testHeldItem = newItem;
    }
}
