using UnityEngine;
using System.Collections;

public class CameraZoomOrtho : MonoBehaviour
{
    public float zoomedSize = 4f;
    public float normalSize = 7f;
    public float zoomDuration = 0.5f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public void ZoomIn()
    {
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(cam.orthographicSize, zoomedSize));
    }

    public void ZoomOut()
    {
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(cam.orthographicSize, normalSize));
    }

    IEnumerator ZoomRoutine(float from, float to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / zoomDuration;
            cam.orthographicSize = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cam.orthographicSize = to;
    }
}
