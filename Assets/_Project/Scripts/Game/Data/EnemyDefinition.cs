using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Game
{
    [CreateAssetMenu(menuName = "Enemy Definition", fileName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string display_name = "lil guy";
        public Sprite sprite;

        public EnemyBehaviour behaviour = EnemyBehaviour.Wanderer;

        public int max_hp = 6;
        public int attack = 2;
        public int defence = 0;
        public int aggro_range = 8;
        public bool is_key = false;
        
        [Tooltip("Enemy will never appear on level shallower than this.")]
        public int minimum_depth = 1;

        [Tooltip("Maximum number of this enemy type allowed per level.")]
        public int max_per_level = 3;

        public int max_for_depth(int depth) => max_per_level * depth;

        public EntityState CreateEntity(Vector2Int position, int depth)
        {
            int hp_bonus = (depth - 1) / 2;

            return new EntityState
            {
                name = display_name,
                faction = Faction.Hostile,
                position = position,
                max_hp = this.max_hp + hp_bonus,
                hp = this.max_hp + hp_bonus,
                attack = this.attack,
                defence = this.defence,
                behaviour = this.behaviour,
                aggro_range = this.aggro_range,
                is_key = this.is_key
            };
        }
    }
}