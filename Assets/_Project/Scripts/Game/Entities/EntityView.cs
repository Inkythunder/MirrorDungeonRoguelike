using Roguelike.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Roguelike.Game
{
    public class EntityView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sprite_renderer;
        [SerializeField] float move_speed = 14f;
        [SerializeField] private Light2D glow;

        /// <summary>
        /// The key enemy is lit while player is in the mirror world.
        /// </summary>
        public void SetGlow(bool on)
        {
            if (glow != null) glow.enabled = on;
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

        public void SetTint(Color tint)
        {
            if (sprite_renderer != null) sprite_renderer.color = tint;
        }
    }
}