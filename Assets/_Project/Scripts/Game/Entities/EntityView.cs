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

        [Header("Idle animation")] 
        [SerializeField] Sprite[] default_idle_frames;
        [SerializeField] int idle_frames_per_second = 6;
        
        [Tooltip("Sprite is facing right by default")]
        [SerializeField] bool sprite_faces_right = true;
        
        Vector2Int last_known_cell;
        
        Sprite[] idle_frames;
        Sprite standing_sprite;
        Sprite downed_sprite;
        private bool showing_downed;
        float frame_timer;
        int frame_index;
        TurnManager turn_manager;

        public void SetDownedSprite(Sprite sprite) => downed_sprite = sprite;

        public void SetTurnManager(TurnManager turn_manager) => this.turn_manager = turn_manager;

        public void SetIdleFrames(Sprite[] frames, int frames_per_second, bool desynchronise = true)
        {
            idle_frames = (frames != null && frames.Length > 0) ? frames : null;
            if (frames_per_second > 0) idle_frames_per_second = frames_per_second;
            if (idle_frames == null || state == null) return;

            if (desynchronise)
            {
                int hash = (state.position.x * 73856093 ^ state.position.y * 19349663) & 0x7fffffff;
                frame_index = hash % idle_frames.Length;
                frame_timer = (hash % 16) / 16f / idle_frames_per_second;
            }
            
            ShowFrame(frame_index);
        }

        bool ShouldIdle()
        {
            // Statues shouldn't move
            if (state.faction == Faction.Petrified) return false;
            if (state.is_downed) return false;
            return true;
        }

        void TickIdle()
        {
            if (idle_frames == null || idle_frames.Length < 2) return;
            if (idle_frames_per_second <= 0f) return;

            if (!ShouldIdle())
            {
                if (!state.is_downed && state.faction == Faction.Petrified && frame_index != 0)
                {
                    frame_index = 0;
                    frame_timer = 0f;
                    ShowFrame(0);
                }

                return;
            }

            frame_timer += Time.deltaTime;
            float frame_duration = 1f / idle_frames_per_second;
            while (frame_timer >= frame_duration)
            {
                frame_timer -= frame_duration;
                frame_index = (frame_index + 1) % idle_frames.Length;
                ShowFrame(frame_index);
            }
        }

        void ShowFrame(int index)
        {
            if (sprite_renderer != null) sprite_renderer.sprite = idle_frames[index];
        }
        
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
            last_known_cell = state.position;
            UpdateSortingOrder();

            if (default_idle_frames != null && default_idle_frames.Length > 0)
            {
                SetIdleFrames(default_idle_frames, idle_frames_per_second, desynchronise: false);
            }
        }

        /// <summary>
        /// Face whichever way we last moved horizontally
        /// </summary>
        void UpdateFacing()
        {
            if (state.position == last_known_cell) return;

            int dx = state.position.x - last_known_cell.x;
            last_known_cell = state.position;

            if (dx == 0) return;
            // We don't want statues to flip when they're pushed.
            if (state.faction == Faction.Petrified || state.is_downed) return;

            bool facing_left = dx < 0;
            // flipX mirrors the artwork along the Y axis.
            if (sprite_renderer != null)
            {
                sprite_renderer.flipX = (facing_left == sprite_faces_right);
            }
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
            UpdateDowned();
            TickIdle();
            UpdateFacing();
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
            if (sprite == null) return;
            standing_sprite = sprite;
            if (sprite_renderer != null) sprite_renderer.sprite = sprite;
        }

        void UpdateDowned()
        {
            if (state.is_downed == showing_downed) return;
            showing_downed = state.is_downed;

            if (showing_downed)
            {
                if (downed_sprite != null && sprite_renderer != null) sprite_renderer.sprite = downed_sprite;
                return;
            }

            frame_index = 0;
            frame_timer = 0f;
            
            if (idle_frames != null && sprite_renderer != null) sprite_renderer.sprite = downed_sprite;
            else if (standing_sprite != null && sprite_renderer != null) sprite_renderer.sprite = standing_sprite;
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