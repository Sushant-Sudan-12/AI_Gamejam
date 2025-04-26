using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class FogDensityController : MonoBehaviour
{
    [Header("Fog Density Settings")]
    [Tooltip("Initial fog density at rest")] public float startDensity = 1f;
    [Tooltip("Reduced fog density when Q is pressed")] public float reducedDensity = 0.082f;

    [Header("Transition Timings (seconds)")]
    [Tooltip("Time to fade from startDensity to reducedDensity")] public float fadeOutDuration = 1f;
    [Tooltip("Time to hold at reducedDensity")] public float holdDuration = 0.2f;
    [Tooltip("Time to fade from reducedDensity back to startDensity")] public float fadeInDuration = 1f;

    [Header("Cooldown Settings")]
    [Tooltip("Cooldown time between Q presses after full transition")] public float cooldownDuration = 4f;

    private bool isFading = false;
    private float lastActivationTime = -Mathf.Infinity;

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = startDensity;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isFading && Time.time >= lastActivationTime)
        {
            StartCoroutine(FadeFogDensity());
        }
    }

    private IEnumerator FadeFogDensity()
    {
        isFading = true;

        // Fade out (startDensity -> reducedDensity) with smooth interpolation
        float t = 0f;
        while (t < fadeOutDuration)
        {
            float normalizedTime = t / fadeOutDuration;
            float smoothStep = normalizedTime * normalizedTime * (3f - 2f * normalizedTime); // smoothstep
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, reducedDensity, smoothStep);
            t += Time.deltaTime;
            yield return null;
        }
        RenderSettings.fogDensity = reducedDensity;

        // Hold at reduced density
        yield return new WaitForSeconds(holdDuration);

        // Fade in (reducedDensity -> startDensity) with smooth interpolation
        t = 0f;
        while (t < fadeInDuration)
        {
            float normalizedTime = t / fadeInDuration;
            float smoothStep = normalizedTime * normalizedTime * (3f - 2f * normalizedTime); // smoothstep
            RenderSettings.fogDensity = Mathf.Lerp(reducedDensity, startDensity, smoothStep);
            t += Time.deltaTime;
            yield return null;
        }
        RenderSettings.fogDensity = startDensity;

        isFading = false;
        lastActivationTime = Time.time + cooldownDuration;
    }
}
