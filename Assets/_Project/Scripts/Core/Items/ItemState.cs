namespace Roguelike.Core
{
    public enum ItemType { Potion, Weapon, Armour }

    public sealed class ItemState
    {
        public string name = "Item";
        public ItemType type;
        public int tier;
    }
}


