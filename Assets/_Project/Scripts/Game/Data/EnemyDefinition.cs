using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Game
{
    [CreateAssetMenu(menuName = "Enemy Definition", fileName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string display_name = "namen't";
        public Sprite sprite;
        
        [Header("Idle animation")]
        [Tooltip("Frames of idle cycle in order. Leave empty to use single sprite.")]
        public Sprite[] idle_frames;

        [Tooltip("Frames per second for the idle animation.")]
        public int idle_frames_per_second = 6;

        public EnemyBehaviour behaviour = EnemyBehaviour.Wanderer;

        public int max_hp = 6;
        public int attack = 2;
        public int defence = 0;
        public int aggro_range = 8;
        public bool is_key = false;
        
        [Tooltip("This enemy is a guardian of the mirror world.")]
        public bool is_guardian = false;
        
        [Header("Knock down")]
        [Tooltip("Turns spent knocked down instead of dying. 0 means they die normally.")]
        [SerializeField] public int downed_duration = 0;

        [Tooltip("Sprite shown while knocked down:")] 
        [SerializeField] public Sprite downed_sprite;
        
        [Tooltip("Enemy will never appear on level shallower than this.")]
        public int minimum_depth = 1;

        [Tooltip("Maximum number of this enemy type allowed per level.")]
        public int max_per_level = 3;

        [Tooltip("Multiplied with world tint. White = normal sprite colour.")]
        public Color tint = Color.white;

        public int max_for_depth(int depth) => max_per_level * depth;
        
        public EntityState CreateEntity(Vector2Int position, int depth)
        {
            int hp_bonus = (depth - 1) / 2;

            return new EntityState
            {
                name = display_name,
                // Guardians start as statues and wake up in the mirror world.
                faction = is_guardian ? Faction.Petrified: Faction.Hostile,
                position = position,
                max_hp = this.max_hp + hp_bonus,
                hp = this.max_hp + hp_bonus,
                attack = this.attack,
                defence = this.defence,
                behaviour = this.behaviour,
                aggro_range = this.aggro_range,
                is_key = this.is_key,
                is_guardian = this.is_guardian,
                downed_duration = this.downed_duration
            };
        }
    }
}