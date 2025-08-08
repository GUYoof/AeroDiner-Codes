using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationTimerController : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private Image lastTimerImg;

    private Image timerImg;

    private void Awake()
    {
        timerImg = GetComponent<Image>();
    }

    private void Start()
    {
        SetupCanvasSorting(gameObject);

        if (lastTimerImg != null)
            SetupCanvasSorting(lastTimerImg.gameObject);
    }


    /// <summary>
    /// 전체 시간 중 현재 남은 시간에 해당하는 스프라이트 인덱스를 계산하여 반환
    /// </summary>
    /// <param name="currentCookingTime">현재 남은 조리 시간</param>
    /// <param name="totalTime">총 조리 시간</param>
    /// <returns>보여줄 스프라이트의 인덱스</returns>
    private int GetSpriteIndex(float currentCookingTime, float totalTime)
    {
        var threshold = totalTime / sprites.Count;

        for (int i = 0; i < sprites.Count; i++)
        {
            var divided = threshold * i;

            if (currentCookingTime <= divided)
            {
                return i;
            }
        }

        return sprites.Count - 1; // 마지막 인덱스 반환
    }

    /// <summary>
    /// 현재 조리 시간에 맞는 스프라이트로 타이머 이미지 업데이트
    /// </summary>
    /// <param name="currentCookingTime">현재 남은 조리 시간</param>
    /// <param name="totalTime">총 조리 시간</param>
    public void UpdateTimer(float currentCookingTime, float totalTime)
    {
        if (timerImg == null)
        {
            return;
        }

        if (sprites == null || sprites.Count == 0)
        {
            return;
        }

        int index = GetSpriteIndex(currentCookingTime, totalTime);

        if (index < 0 || index >= sprites.Count)
        {
            return;
        }

        timerImg.sprite = sprites[index];
    }

    public void ShowPassiveCookingState()
    {
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogWarning("[TimerController] sprites 리스트가 비어있거나 null입니다.");
            return;
        }

        if (lastTimerImg == null)
        {
            Debug.LogError("[TimerController] timerImg가 연결되지 않았습니다. Inspector에서 Image 컴포넌트를 연결하세요.");
            return;
        }

        int lastIndex = sprites.Count - 1;
        Sprite passiveSprite = sprites[lastIndex];

        if (passiveSprite == null)
        {
            Debug.LogWarning("[TimerController] 마지막 스프라이트가 null입니다.");
            return;
        }

        lastTimerImg.sprite = passiveSprite;
        lastTimerImg.enabled = true;           // 이미지 표시
        gameObject.SetActive(true);        // 타이머 오브젝트 활성화
    }
    private void SetupCanvasSorting(GameObject obj)
    {
        var canvas = obj.GetComponent<Canvas>();
        if (canvas == null)
            canvas = obj.AddComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = 110;
    }
}
