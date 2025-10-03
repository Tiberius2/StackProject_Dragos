using UnityEngine;
using UnityEngine.UI;

public class GlowEffect : MonoBehaviour
{
    public Material mat;             // Assign in Inspector
    public Color glowColor = Color.cyan;
    public float glowIntensityMin = 0.2f;
    public float glowIntensityMax = 2f;
    public float glowSpeed = 2f;

    public Image vignette;

    private void Update()
    {
        // PingPong gives a repeating 0->1->0 value
        float t = Mathf.PingPong(Time.time * glowSpeed, 1f);

        // Smoothly interpolate intensity
        float intensity = Mathf.Lerp(glowIntensityMin, glowIntensityMax, t);

        // Set emission color
        Color finalColor = glowColor * intensity;
        mat.SetColor("_EmissionColor", finalColor);

        // Make sure emission is enabled on the shader
        DynamicGI.SetEmissive(GetComponent<Renderer>(), finalColor);
    }

    public void SetGlowColorHex(string hex, float alpha)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color parsed))
        {
            glowColor = parsed;
            vignette.color = parsed;
            vignette.GetComponent<CanvasGroup>().alpha = alpha;
        }
    }

}
