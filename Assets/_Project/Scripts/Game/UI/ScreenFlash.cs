using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Roguelike.Game
{
    /// <summary>
    /// A full-screen panel that flashes and fades out.
    /// An effect for switching between worlds. 
    /// </summary>
    public class ScreenFlash : MonoBehaviour
    {
        [SerializeField] Image flash_panel;
        [SerializeField] float duration = 0.3f;
        
        Coroutine running;

        void Awake()
        {
            SetAlpha(0f);
        }

        public void Play(Color colour)
        {
            // This check makes it so if two world transitions happen quickly (before the first one had time to finish)
            // it kills the first coroutine before starting another.
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Flash(colour));
        }
        
        IEnumerator Flash(Color colour)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // 1 at the start, 0 at the end
                colour.a = 1f - elapsed / duration;
                flash_panel.color = colour;
                yield return null;
            }

            SetAlpha(0f);
            running = null;
        }

        void SetAlpha(float alpha)
        {
            // have to set the colour this way because "flash_panel.color.a" = isn't allowed.
            Color colour = flash_panel.color;
            colour.a = alpha;
            flash_panel.color = colour;
        }
    }
}

