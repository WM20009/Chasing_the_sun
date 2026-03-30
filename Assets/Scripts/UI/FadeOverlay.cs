using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        CacheComponents();
        SetAlpha(canvasGroup.alpha);
    }

    private void OnValidate()
    {
        CacheComponents();
    }

    public IEnumerator FadeTo(float targetAlpha, float duration)
    {
        CacheComponents();

        var startAlpha = canvasGroup.alpha;
        if (Mathf.Approximately(duration, 0f))
        {
            SetAlpha(targetAlpha);
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    public void SetAlpha(float alpha)
    {
        CacheComponents();
        canvasGroup.alpha = Mathf.Clamp01(alpha);
        canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.001f;
        canvasGroup.interactable = canvasGroup.blocksRaycasts;
    }

    private void CacheComponents()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
