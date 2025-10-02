using UnityEngine;
using System.Collections;

public class DropZoneFeedback : MonoBehaviour
{
    public float shakeAmount = 0.5f;
    public float shakeDuration = 0.3f;
    public float shakeSpeed = 20f;

    public void ShakeZ()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeZCoroutine());
    }

    IEnumerator ShakeZCoroutine()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offset = Mathf.Sin(elapsed * shakeSpeed) * shakeAmount;
            transform.localPosition = originalPos + new Vector3(0f, 0f, offset);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
