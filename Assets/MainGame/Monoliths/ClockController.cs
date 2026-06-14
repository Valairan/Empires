using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class ClockController : NetworkBehaviour
{
    [Header("Lighting References")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private float maxSunIntensity = 90000; // Lux for HDRP
    [SerializeField] private float maxMoonIntensity = 1000f;

    [Header("HDRP Volume References")]
    [SerializeField] private Volume dayVolume;

    [Header("Visual Blending Profile")]
    [Tooltip("X-Axis: Time of day (0 to 1). Y-Axis: Day Volume weight (0 = Night, 1 = Day).")]
    [SerializeField] private AnimationCurve dayVolumeCurve;

    private void Update()
    {
        if (NetworkGamePropertiesStorage.Singleton == null) return;

        float timeSample = NetworkGamePropertiesStorage.Singleton.CurrentTime.Value;
        EvaluateEnvironmentVisuals(timeSample);
    }

    private void EvaluateEnvironmentVisuals(float time)
    {
        // 1. Map 0.0 - 1.0 to rotations.
        float sunAngle = (time * 360f) - 90f;
        float moonAngle = sunAngle + 180f;

        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, 0f, 0f);

        // 2. Evaluate Continuous Volume Weights
        float dayWeight = dayVolumeCurve.Evaluate(time);
        dayVolume.weight = dayWeight;

    }

    /// <summary>
    /// Updates intensity and disables the light component completely when dark 
    /// to eliminate unnecessary shadow map rendering passes.
    /// </summary>
    private void ManageLightComponent(Light targetLight, float targetIntensity)
    {
        if (targetIntensity > 0f)
        {
            // If the light needs to be bright, ensure it's on first, then apply intensity
            if (!targetLight.enabled) targetLight.enabled = true;
            targetLight.intensity = targetIntensity;
        }
        else
        {
            // Once intensity zeroes out, kill the component to save performance
            if (targetLight.enabled)
            {
                targetLight.intensity = 0f;
                targetLight.enabled = false;
            }
        }
    }
}