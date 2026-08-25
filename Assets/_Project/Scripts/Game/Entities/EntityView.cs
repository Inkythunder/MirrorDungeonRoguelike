using Roguelike.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace Roguelike.Game
{
    public class EntityView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sprite_renderer;
        [SerializeField] float move_speed = 14f;
        [SerializeField] private Light2D glow;
        [SerializeField] Image hp_fill;
        [SerializeField] Image hp_bar_back;
        
        // Entitiy's default colour. Stone grey for guardians, white for everything else.
        Color base_colour = Color.white;
        // Tint of whatever world we're currently in.
        Color world_tint = Color.white;
        
        // Allowed when we spawn an enemy but not when we spawn the player.
        // For enemy, always allowed, but only visible when they take damage.
        private bool health_bar_allowed;

        /// <summary>
        /// The key enemy is lit while player is in the mirror world.
        /// </summary>
        public void SetGlow(bool on)
        {
            if (glow != null) glow.enabled = on;
        }

        public void ShowHealthBar(bool on)
        {
            health_bar_allowed = on;
            UpdateHealthBar();
        }

        /// <summary>
        /// Bar hidden at full health and appears when enemy is hurt.
        /// </summary>
        void UpdateHealthBar()
        {
            if (hp_fill == null || hp_bar_back == null) return;
            
            bool visible = health_bar_allowed && state != null && state.hp < state.max_hp;
            hp_fill.enabled = visible;
            hp_bar_back.enabled = visible;

            if (visible)
            {
                hp_fill.fillAmount = Mathf.Clamp01((float)state.hp / state.max_hp);
            }
        }
        
        public EntityState state { get; private set; }

        public void Bind(EntityState state)
        {
            this.state = state;
            transform.position = CellToWorld(state.position);
            UpdateSortingOrder();
        }

        public void Update()
        {
            if (state == null) return;

            if (!state.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 target = CellToWorld(state.position);
            transform.position = Vector3.MoveTowards(
                transform.position, target, move_speed * Time.deltaTime
            );

            UpdateHealthBar();

            UpdateSortingOrder();
        }
        
        /// <summary>
        /// Sort entities by height on screen such that entities that are lower are drawn in front.
        /// </summary>
        void UpdateSortingOrder()
        {
            if (sprite_renderer != null)
            {
                sprite_renderer.sortingOrder = -(state.position.y);
            }
        }

        public static Vector3 CellToWorld(Vector2Int cell) => new Vector3(cell.x + 0.5f, cell.y, 0f);

        public void SetSprite(Sprite sprite)
        {
            if (sprite_renderer != null && sprite != null)
            {
                sprite_renderer.sprite = sprite;
            }
        }

        public void SetBaseColour(Color colour)
        {
            base_colour = colour;
            Repaint();
        }

        public void SetTint(Color tint)
        {
            world_tint = tint;
            Repaint();
        }

        void Repaint()
        {
            if (sprite_renderer != null) sprite_renderer.color = base_colour * world_tint;
        }
    }
}