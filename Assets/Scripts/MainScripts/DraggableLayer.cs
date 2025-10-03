using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class DraggableLayer : MonoBehaviour
{

    public enum LayerType { SURFACE, BASE, SUBBASE, SUBGRADE }
    public LayerType layerType;
    [Header("Scale")]
    public Vector3 restScale = Vector3.one * 4f;
    public Vector3 dragScale = Vector3.one * 1f;
    public float scaleSpeed = 10f;
    [Header("Return")]
    public float returnDuration = 0.35f;
    Rigidbody rb;
    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 startScale;
    bool isDragging = false;
    Camera cam;
    Vector3 dragOffset;
    [HideInInspector] public bool isInsideDropZone = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
        transform.localScale = restScale;
        rb.isKinematic = true;
        rb.useGravity = false;
    }
    void Update()
    {
        Vector3 targetScale = isDragging ? dragScale : restScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale,
                                            Time.deltaTime * scaleSpeed);
        if (isDragging)
        {
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            Plane p = new Plane(Vector3.up, Vector3.zero);
            if (p.Raycast(r, out float enter))
            {
                Vector3 world = r.GetPoint(enter) + dragOffset;
                transform.position =
                    new Vector3(world.x, transform.position.y, world.z);
            }
        }

    }
    void OnMouseDown()
    {
        if (GameManager.Instance == null)
            return;
        if (!GameManager.Instance.CanDrag(this))
            return;
        isDragging = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        dragOffset = Vector3.zero;
        transform.position += Vector3.up * 1f;
        GameManager.Instance.OnPick(this);
    }
    void OnMouseUp()
    {
        if (!isDragging)
            return;
        isDragging = false;
        GameManager.Instance.OnDrop(this);
        if (isInsideDropZone)
        {
            var zone = GameObject.FindGameObjectWithTag("DropZone");
            if (zone != null)
            {
                var glow = zone.GetComponentInChildren<GlowEffect>();
                if (glow != null)
                {
                    int current = GameManager.Instance.CurrentIndex;
                    LayerSlot expected = GameManager.Instance.orderedSlots[current];

                    if (layerType != expected.slotType)
                    {
                        Debug.Log("we are heere!");
                        glow.SetGlowColorHex("69B3FF", 0); // or dim color
                        var feedback = zone.GetComponentInChildren<DropZoneFeedback>();
                        if (feedback != null)
                            feedback.ShakeZ();
                    }
                }
            }
        }
    }
    public void ResetToStart(bool instant = false)
    {
        StopAllCoroutines();
        rb.isKinematic = true;
        rb.useGravity = false;
        if (instant)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
            transform.localScale = restScale;
        }
        else
        {
            StartCoroutine(
                MoveBackCoroutine(startPosition, startRotation, returnDuration));
        }
    }
    IEnumerator MoveBackCoroutine(Vector3 pos, Quaternion rot, float dur)
    {
        float t = 0f;
        Vector3 p0 = transform.position;
        Quaternion q0 = transform.rotation;
        Vector3 s0 = transform.localScale;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.position = Vector3.Lerp(p0, pos, t);
            transform.rotation = Quaternion.Slerp(q0, rot, t);
            transform.localScale = Vector3.Lerp(s0, restScale, t);
            yield return null;
        }
        transform.position = pos;
        transform.rotation = rot;
        transform.localScale = restScale;
        rb.isKinematic = true;
        rb.useGravity = false;
    }
    public void ReleaseToFall()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }  // Expose setters for GameManager to override stored start transform
    public void SetStartTransform(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        startPosition = pos;
        startRotation = rot;
        startScale = scale;
    }


    void OnTriggerEnter(Collider other)
    {
        if (!isDragging) return;
        if (other.CompareTag("DropZone")) // tag your cube as "DropZone"
        {
            isInsideDropZone = true;
            var glow = other.GetComponentInChildren<GlowEffect>();
            if (glow != null)
            {
                int current = GameManager.Instance.CurrentIndex;
                LayerSlot expected = GameManager.Instance.orderedSlots[current];
                if (layerType != expected.slotType)
                {
                    glow.SetGlowColorHex("FF3600", .25f);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isDragging) return;
        if (other.CompareTag("DropZone"))
        {
            isInsideDropZone = false;
            var glow = other.GetComponentInChildren<GlowEffect>();
            if (glow != null)
            {
                glow.SetGlowColorHex("69B3FF", 0); // optional: neutral white or dim
            }
        }
    }
}