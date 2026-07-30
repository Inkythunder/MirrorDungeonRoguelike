using UnityEngine;

namespace Roguelike.Core
{
    [CreateAssetMenu(menuName = "Item Definition", fileName = "Item")]
    public class ItemDefinition : ScriptableObject
    {
        public string display_name = "potion";
        public Sprite sprite;
        public ItemType type = ItemType.Potion;

        [Tooltip("Weapon: added to attack. Armour: added to defence. Potions ignore it.")]
        public int tier = 1;

        [Tooltip("Item will never appear on level shallower than this.")]
        public int minimum_depth = 1;

        public ItemState CreateItem()
        {
            return new ItemState()
            {
                name = display_name,
                type = type,
                tier = tier
            };
        }
    }
}