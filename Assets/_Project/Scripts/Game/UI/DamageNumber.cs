using TMPro;
using UnityEngine;

namespace Roguelike.Game
{
    public class DamageNumber : MonoBehaviour
    {
        [Header("Text Settings")]
        [SerializeField] TMP_Text text;
    
        [Tooltip("Number of seconds text should be visible")]
        [SerializeField] float lifetime = 0.7f;
    
        [Tooltip("How fast text should rise")]
        [SerializeField] float rise_speed = 1.5f;
        
        [Tooltip("How far above the entity the number starts")]
        [SerializeField] float spawn_offset = 0.7f;
        
        // Seconds since number was spawned
        float age;
        // Colour set on the prefab because we only need to change the alpha to make it fade
        Color start_colour;

        void Awake()
        {
            start_colour = text.color;
        }

        /// <summary>
        /// Called once when the number is spawned
        /// </summary>
        public void Show(int amount, Vector3 world_position)
        {
            text.text = amount.ToString();
            transform.position = world_position + Vector3.up * spawn_offset;

            age = 0f;
            start_colour.a = 1f;
            text.color = start_colour;
        }

        void Update()
        {
            age += Time.deltaTime;

            transform.position += Vector3.up * (rise_speed * Time.deltaTime);
            
            //Fade to invisible across "lifetime" time.
            Color colour = start_colour;
            colour.a = 1f - Mathf.Clamp01(age / lifetime);
            text.color = colour;
            
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}