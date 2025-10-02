using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool autoStart = false; // set false in Inspector

    [Header("Slots (ordered top->bottom)")]
    public List<LayerSlot> orderedSlots;

    [Header("Scene layer instances (place these in the scene spawn area)")]
    public List<DraggableLayer> layerInstances;

    [Header("Spawn")]
    public Transform spawnArea;
    public float spawnSpacing = 1.2f;

    [Header("Car")]
    public GameObject carPrefab;
    public Transform carSpawnAbove;
    public bool gameStarted = false;


    public GameObject airAnchorHighlight;
    // runtime state
    int nextIndex = 0;
    bool busy = false;
    Coroutine activePlacementCoroutine = null;
    GameObject spawnedCar = null;
    public int CurrentIndex => nextIndex;

    void Awake() { Instance = this; }

    void Start()
    {
        if (autoStart) StartGame();
    }

    public void StartGame()
    {
        ResetInternalState();
        if (airAnchorHighlight != null) airAnchorHighlight.SetActive(true);
        gameStarted = true;
        var camZoom = Camera.main.GetComponent<CameraZoomOrtho>();
        if (camZoom != null) camZoom.ZoomOut();
        PlaceAndShuffleSceneInstances();
        UpdateHighlights();
    }

    // -------------------------
    // Spawning / placement
    // -------------------------
    private void PlaceAndShuffleSceneInstances()
    {
        if (layerInstances == null || layerInstances.Count == 0 || spawnArea == null) return;

        // make a shuffled copy of the list
        List<DraggableLayer> list = new List<DraggableLayer>(layerInstances);
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            var tmp = list[r]; list[r] = list[i]; list[i] = tmp;
        }

        // place them into the spawnArea row
        for (int i = 0; i < list.Count; i++)
        {
            var layer = list[i];
            Vector3 p = spawnArea.position + new Vector3(i * spawnSpacing, 0f, 0f);
            Quaternion rot = layer.transform.rotation;

            layer.transform.position = p;
            layer.transform.rotation = rot;
            layer.SetStartTransform(p, rot, layer.transform.localScale);
            layer.ResetToStart(true);

            var rb = layer.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                // Note: avoid using rb.linearVelocity / rb.angularVelocity (use rb.velocity / rb.angularVelocity if needed when non-kinematic)
            }
        }

        // reset indices/state
        nextIndex = 0;
        busy = false;
        UpdateHighlights();
    }

    // -------------------------
    // Drag/drop API (called from DraggableLayer)
    // -------------------------
    public bool CanDrag(DraggableLayer dl)
    {
        return gameStarted && !busy;
    }

    public void OnPick(DraggableLayer dl) { /* optional feedback */ }

    public void OnDrop(DraggableLayer dl)
    {
        if (busy || !dl.isInsideDropZone)
        {
            dl.ResetToStart();
            return;
        }

        if (orderedSlots == null || orderedSlots.Count == 0)
        {
            dl.ResetToStart();
            return;
        }

        LayerSlot expected = orderedSlots[nextIndex];
        if (dl.layerType == expected.slotType)
        {
            //var glow = airAnchorHighlight.GetComponent<GlowEffect>();
            //if(glow != null)
            //{
            //    string hex = (dl.layerType == expected.slotType) ? "46FF00" : "FF3600";
            //    glow.SetGlowColorHex(hex);
            //}
            if (activePlacementCoroutine != null) StopCoroutine(activePlacementCoroutine);
            activePlacementCoroutine = StartCoroutine(HandleCorrectDrop(dl, expected));
        }
        else
        {
            dl.ResetToStart();
        }
    }

    // -------------------------
    // Placement coroutine (waits for physics settle)
    // -------------------------
    IEnumerator HandleCorrectDrop(DraggableLayer dl, LayerSlot slot)
    {
        busy = true;
        dl.transform.rotation = slot.finalTransform.rotation;

        Vector3 from = dl.transform.position;
        Vector3 to = slot.airAnchor.position;
        float t = 0f;
        float duration = 0.18f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            dl.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        dl.ReleaseToFall();

        Rigidbody rb = dl.GetComponent<Rigidbody>();
        Collider col = dl.GetComponent<Collider>();

        // thresholds (tweak for your scale)
        float posTolerance = 0.12f;
        float linVelThreshold = 0.25f;
        float angVelThreshold = 0.25f;
        float settleTimeout = 3f;

        float elapsed = 0f;

        // Wait until Rigidbody is sleeping OR velocities are low and near final Y OR timeout
        while (elapsed < settleTimeout)
        {
            if (rb == null) break;

            if (rb.IsSleeping())
            {
                break;
            }

            if (rb.linearVelocity.magnitude <= linVelThreshold &&
                rb.angularVelocity.magnitude <= angVelThreshold &&
                Mathf.Abs(dl.transform.position.y - slot.finalTransform.position.y) <= posTolerance)
            {
                yield return null;
                if (rb.linearVelocity.magnitude <= linVelThreshold) break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // If still active/moving, reduce bounciness and damp velocities before snapping
        if (rb != null)
        {
            if (col != null)
            {
                var phys = col.sharedMaterial;
                if (phys != null)
                {
                    PhysicsMaterial tmp = new PhysicsMaterial(phys.name + "_temp_nobounce")
                    {
                        bounciness = 0f,
                        bounceCombine = PhysicsMaterialCombine.Minimum,
                        dynamicFriction = phys.dynamicFriction,
                        staticFriction = phys.staticFriction,
                        frictionCombine = phys.frictionCombine
                    };
                    col.material = tmp;
                }
            }

            // body should currently be non-kinematic (we called ReleaseToFall)
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // final snap to exact transform and freeze physics
        if (rb != null) rb.isKinematic = true;
        dl.transform.position = slot.finalTransform.position;
        dl.transform.rotation = slot.finalTransform.rotation;

        // advance game state
        var zone = GameObject.FindGameObjectWithTag("DropZone");
        if (zone != null)
        {
            var glow = zone.GetComponentInChildren<GlowEffect>();
            if (glow != null)
            {
                glow.SetGlowColorHex("69B3FF"); // or your neutral/default color
            }
        }
        nextIndex++;
        UpdateHighlights();

        // clear active coroutine reference
        activePlacementCoroutine = null;
        busy = false;
        CheckComplete();
    }

    // -------------------------
    // Highlights / completion
    // -------------------------
    void UpdateHighlights()
    {
        if (orderedSlots == null) return;
        for (int i = 0; i < orderedSlots.Count; i++)
            orderedSlots[i].SetHighlight(i == nextIndex);
    }

    void CheckComplete()
    {
        if (nextIndex >= orderedSlots.Count)
        {
            if (airAnchorHighlight != null) airAnchorHighlight.SetActive(false);
            StartCoroutine(DropCar());
            // Show replay UI: UIController.ShowReplay() or similar
            var ui = FindObjectOfType<UIController>();
            if (ui != null) ui.ShowReplayButton();
            var camZoom = Camera.main.GetComponent<CameraZoomOrtho>();
            if (camZoom != null) camZoom.ZoomIn();
        }
    }

    IEnumerator DropCar()
    {
        yield return new WaitForSeconds(0.25f);
        if (carPrefab != null && carSpawnAbove != null)
        {
            if (spawnedCar != null) Destroy(spawnedCar);
            spawnedCar = Instantiate(carPrefab, carSpawnAbove.position, carSpawnAbove.rotation);
            Rigidbody rb = spawnedCar.GetComponent<Rigidbody>();
            if (rb) rb.useGravity = true;
        }
    }

    // -------------------------
    // Reset / Replay without scene reload
    // -------------------------
    public void ResetGame()
    {
        ResetInternalState();
        // reshuffle and place scene instances again
        PlaceAndShuffleSceneInstances();
        var camZoom = Camera.main.GetComponent<CameraZoomOrtho>();
        if (camZoom != null) camZoom.ZoomOut();
        // reset indices/highlights
        nextIndex = 0;
        busy = false;
        UpdateHighlights();
    }

    void ResetInternalState()
    {
        if (activePlacementCoroutine != null) { StopCoroutine(activePlacementCoroutine); activePlacementCoroutine = null; }

        if (spawnedCar != null) { Destroy(spawnedCar); spawnedCar = null; }

        if (layerInstances != null)
        {
            foreach (var dl in layerInstances)
            {
                if (dl == null) continue;
                dl.StopAllCoroutines();
                dl.ResetToStart(true);

                var rb = dl.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;      // make kinematic first
                    rb.useGravity = false;
                    // DO NOT set rb.velocity or rb.angularVelocity here while kinematic
                }
            }
        }

        nextIndex = 0;
        busy = false;
        activePlacementCoroutine = null;
        gameStarted = false;
    }
}
