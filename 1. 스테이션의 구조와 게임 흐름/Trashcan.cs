using UnityEngine;

public class Trashcan : MonoBehaviour, IInteractable, IMovableStation

{
    public Transform GetTransform() => transform;
    [SerializeField] private StationData stationData;
    public StationData StationData => stationData;

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


    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {

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
