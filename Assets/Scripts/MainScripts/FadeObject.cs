using UnityEngine;
using System.Collections;

public class FadeObject : MonoBehaviour
{
    public Renderer targetRenderer; // Assign in Inspector
    public float fadeDuration = 0.5f;

    Material mat;
    Color originalColor;

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        mat = targetRenderer.material;
        originalColor = mat.color;
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f, originalColor.a));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(originalColor.a, 0f));
    }

    IEnumerator FadeRoutine(float from, float to)
    {
        float t = 0f;
        Color c = originalColor;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            c.a = Mathf.Lerp(from, to, t);
            mat.color = c;
            yield return null;
        }

        c.a = to;
        mat.color = c;
    }
}