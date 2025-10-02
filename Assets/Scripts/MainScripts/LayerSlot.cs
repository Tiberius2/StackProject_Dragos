using UnityEngine;

public class LayerSlot : MonoBehaviour
{
    public DraggableLayer.LayerType slotType;
    public Transform airAnchor; // position in air above the final slot
    public Transform finalTransform; // final resting transform (position/rotation)
    public GameObject highlight; // toggle highlight

    public void SetHighlight(bool on)
    {
        if (highlight) highlight.SetActive(on);
    }
}
