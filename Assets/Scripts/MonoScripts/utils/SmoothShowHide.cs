using UnityEngine;
using System.Collections;

public class SmoothShowHide : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public Vector3 hiddenScale = Vector3.zero;
    public Vector3 visibleScale = Vector3.one;

    private Coroutine currentAnimation;

    void Awake()
    {
        // Optionally, ensure the starting scale is correct
        if (!gameObject.activeSelf)
        {
            transform.localScale = hiddenScale;
        }
    }

    public void Show()
    {
        if (gameObject.activeSelf)
        {
            // Already active, do nothing
            return;
        }

        gameObject.SetActive(true);

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateScale(hiddenScale, visibleScale));
    }

    public void Hide()
    {
        if (!gameObject.activeSelf)
        {
            // Already inactive, do nothing
            return;
        }

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateAndDisable(visibleScale, hiddenScale));
    }

    private IEnumerator AnimateScale(Vector3 from, Vector3 to)
    {
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localScale = to;
        currentAnimation = null;
    }

    private IEnumerator AnimateAndDisable(Vector3 from, Vector3 to)
    {
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localScale = to;
        gameObject.SetActive(false);

        currentAnimation = null;
    }
}
