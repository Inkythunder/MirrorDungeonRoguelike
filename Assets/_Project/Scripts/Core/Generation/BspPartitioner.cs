using UnityEngine;

namespace Roguelike.Core
{
    public static class BspPartitioner
    {

        /// <summary>
        /// Recursively subdivide an area into a binary tree on non-overlapping partitions. 
        /// </summary>
        /// <returns></returns>
        public static BspNode Partition(RectInt area, int min_partition_size, int max_depth, Rng rng)
        {
            var root = new BspNode(area);
            Split(root, min_partition_size, max_depth, 0, rng);
            return root;
        }
        
        static void Split(BspNode node, int min_size, int max_depth, int depth, Rng rng)
        {
            if (depth >= max_depth) return;

            RectInt bounds = node.bounds;

            // We only split if both halves of the partition would still meet the minimum size.
            bool can_split_vertically = bounds.width >= min_size * 2;
            bool can_split_horizontally = bounds.height >= min_size * 2;

            if (!can_split_vertically && !can_split_horizontally) return;

            // If both splits are possible, randomly choose one. Otherwise, choose the only one that's possible.
            bool split_vertically;
            if (can_split_vertically && can_split_horizontally)
            {
                split_vertically = rng.Chance(0.5f);
            }
            else
            {
                split_vertically = can_split_vertically;
            }

            // Perform the split of the partition into two partitions making sure each new partition is at least
            // as big as the minimum room size.
            if (split_vertically)
            {
                int cut = rng.Range(bounds.xMin + min_size, bounds.xMax - min_size + 1);
                node.left = new BspNode(new RectInt(bounds.xMin, bounds.yMin, cut - bounds.xMin, bounds.height));
                node.right = new BspNode(new RectInt(cut, bounds.yMin,bounds.xMax - cut, bounds.height));
            }
            else
            {
                int cut = rng.Range(bounds.yMin + min_size, bounds.yMax - min_size + 1);
                node.left = new BspNode(new RectInt(bounds.xMin, bounds.yMin, bounds.width, cut - bounds.yMin));
                node.right = new BspNode(new RectInt(bounds.xMin,cut, bounds.width, bounds.yMax - cut));
            }
            
            // Recursive step to call split until we've reached max depth or we have no more legal splits.
            Split(node.left, min_size, max_depth, depth + 1, rng);
            Split(node.right, min_size, max_depth, depth + 1, rng);
        }
    }
}
