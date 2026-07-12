using System;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Game
{
    public class EntityView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sprite_renderer;
        [SerializeField] float move_speed = 14f;
        
        public EntityState State { get; private set; }

        public void Bind(EntityState state)
        {
            State = state;
            transform.position = CellToWorld(state.position);
            UpdateSortingOrder();
        }

        public void Update()
        {
            if (State == null) return;

            Vector3 target = CellToWorld(State.position);
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
                sprite_renderer.sortingOrder = -(State.position.y);
            }
        }

        public static Vector3 CellToWorld(Vector2Int cell) => new Vector3(cell.x + 0.5f, cell.y, 0f);
    }
}