using UnityEngine;

public class ChangeSprite : MonoBehaviour
{
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite cookSprite;

    private SpriteRenderer spriteRenderer;
    private BaseStation baseStation;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseStation = GetComponent<BaseStation>();
    }

    private void Update()
    {
        if (baseStation == null || spriteRenderer == null) return;

        // IsCookingOrWaiting 값에 따라 스프라이트 교체
        if (baseStation.IsCookingOrWaiting)
        {
            spriteRenderer.sprite = cookSprite;
        }
        else
        {
            spriteRenderer.sprite = baseSprite;
        }
    }
}