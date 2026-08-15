using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Roguelike.Game
{
    /// <summary>
    /// Gives a light a breathing effect for aesthetics.
    /// </summary>
    public class GlowPulse : MonoBehaviour
    {
        [SerializeField] Light2D light;
        [SerializeField] float minimum_intensity = 0.5f;
        [SerializeField] float maximum_intensity = 0.9f;
        [SerializeField] float cycles_per_second = 0.5f;

        private void Update()
        {
            if (light == null || !light.enabled) return;
            
            // Use a sine wave to fluctuate the light intensity for a pulse effect.
            float t = (Mathf.Sin(Time.time * cycles_per_second * Mathf.PI * 2f) + 1f) * 0.5f;
            light.intensity = Mathf.Lerp(minimum_intensity, maximum_intensity, t);
        }
    }
}