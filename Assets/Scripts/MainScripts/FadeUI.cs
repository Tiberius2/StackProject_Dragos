using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeUI : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f, 1f));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f, 0f, deactivateAfter: true));
    }

    IEnumerator FadeRoutine(float from, float to, bool deactivateAfter = false)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cg.alpha = to;
        cg.interactable = (to == 1f);
        cg.blocksRaycasts = (to == 1f);

        if (deactivateAfter) gameObject.SetActive(false);
    }
}
