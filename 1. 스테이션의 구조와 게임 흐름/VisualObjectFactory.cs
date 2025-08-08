using UnityEngine;

/// <summary>
/// 재료, 메뉴 등의 시각적 오브젝트를 생성해주는 유틸리티 클래스
/// </summary>
public static class VisualObjectFactory
{
    /// <summary>
    /// 지정된 부모 위치에 비주얼 오브젝트를 생성
    /// SpriteRenderer, CircleCollider2D, Rigidbody2D를 기본으로 포함
    /// </summary>
    /// <param name="parent">부모 Transform</param>
    /// <param name="name">오브젝트 이름</param>
    /// <param name="icon">아이콘 이미지(Sprite)</param>
    /// <returns>생성된 GameObject</returns>
    public static GameObject CreateIngredientVisual(Transform parent, string name, Sprite icon)
    {
        if (parent == null)
        {
            Debug.LogError("[VisualObjectFactory] parent가 null입니다.");
            return null;
        }

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.tag = "Ingredient";
        obj.layer = 6;

        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 110;
        renderer.sprite = icon;
        if (icon == null) renderer.color = Color.gray;

        var collider = obj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.3f;

        var rigidbody = obj.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;

        return obj;
    }

    public static GameObject PlaceIngredientVisual(Transform parent, string name, Sprite icon)
    {
        if (parent == null)
        {
            Debug.LogError("[VisualObjectFactory] parent가 null입니다.");
            return null;
        }

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.tag = "Ingredient";
        obj.layer = 6;

        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 110;
        renderer.sprite = null;
        if (icon == null) renderer.color = Color.gray;

        var collider = obj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.3f;

        var rigidbody = obj.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;

        return obj;
    }
}
