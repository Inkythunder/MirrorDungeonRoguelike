using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    /// <summary>
    /// One node in the BSP tree. Each node has two child nodes and each node becomes a room.
    /// </summary>
    public sealed class BspNode
    {
        public RectInt bounds;
        public BspNode left;
        public BspNode right;

        public BspNode(RectInt bounds)
        {
            this.bounds = bounds;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public List<BspNode> Leaves()
        {
            var list = new List<BspNode>();
            Collect(list);
            return list;
        }

        void Collect(List<BspNode> into)
        {
            if (IsLeaf())
            {
                into.Add(this);
                return;
            }
            if (left != null) left.Collect(into);
            if (right != null) right.Collect(into);
        }
    }
}
