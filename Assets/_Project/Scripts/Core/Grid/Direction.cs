using UnityEngine;

namespace Roguelike.Core
{
    public static class Direction
    {
        public static readonly Vector2Int Up = new Vector2Int(0, 1);
        public static readonly Vector2Int Up_Right = new Vector2Int(1, 1);
        public static readonly Vector2Int Right = new Vector2Int(1, 0);
        public static readonly Vector2Int Down_Right = new Vector2Int(1, -1);
        public static readonly Vector2Int Down = new Vector2Int(0, -1);
        public static readonly Vector2Int Down_Left = new Vector2Int(-1, -1);
        public static readonly Vector2Int Left = new Vector2Int(-1, 0);
        public static readonly Vector2Int Up_Left = new Vector2Int(-1, 1);
        
        /// <summary>
        /// Four cardinal directions: Clockwise from Up.
        /// </summary>
        public static readonly Vector2Int[] Cardinals4 = { Up, Right, Down, Left };

        /// <summary>
        /// Eight cardinal directions: Clockwise from Up.
        /// </summary>
        public static readonly Vector2Int[] Cardinals8 =
            { Up, Up_Right, Right, Down_Right, Down, Down_Left, Left, Up_Left };
    }
}
