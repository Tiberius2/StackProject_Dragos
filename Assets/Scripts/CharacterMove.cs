using System;
using System.Collections;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    public Vector3 from;
    public Vector3 to;
    public float speed = 1f;
    
    private RectTransform rectTransform;

    void OnEnable()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = from;
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rectTransform.anchoredPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
