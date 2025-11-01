
internal class AABB
{
    (double, double)[] _bounds;
    bool isLeaf;

    public AABB(params (double min, double max)[] bounds)
    {
        //(min, max)
        _bounds = bounds;
    }

    public bool IsOverlapping(AABB box)
    {
        // all AABBs will? should? have the same dimensions, so this should be safe
        for (int i = 0; i < _bounds.Length; i++)
        {
            (double min1, double max1) = _bounds[0];
            (double min2, double max2) = box._bounds[0];

            if (min1 > max2) return false;
            if (min2 > max1) return false;
        }

        return true;
    }

    public double GetOverlapVolume(AABB box)
    {
        double volume = 1;
        for (int i = 0; i < _bounds.Length; i++)
        {
            (double min1, double max1) = _bounds[0];
            (double min2, double max2) = box._bounds[0];

            double overlapMin = Math.Max(min1, min2);
            double overlapMax = Math.Min(max1, max2);

            volume *= overlapMax - overlapMin;
        }

        return volume;
    }

    public double GetVolume()
    {
        double volume = 1;
        for (int i = 0; i < _bounds.Length; i++)
        {
            (double min, double max) = _bounds[0];

            volume *= max - min;
        }

        return volume;
    }

    public AABB Merge(AABB box)
    {
        AABB newAABB = new(new (double, double)[3]);

        for (int i = 0; i < _bounds.Length; i++)
        {
            (double min1, double max1) = _bounds[0];
            (double min2, double max2) = box._bounds[0];

            double newMin = Math.Min(min1, min2);
            double newMax = Math.Max(max1, max2);
            newAABB._bounds[i] = (newMin, newMax);
        }

        return newAABB;
    }
}

internal class AABBTree
{
    AABB? _node;
    AABBTree? _left;
    AABBTree? _right;

    public AABBTree(
        AABB? aabb,
        AABBTree? left,
        AABBTree? right
    )
    {
        _node = aabb;
        _left = left != null ? left : new(null, null, null);
        _right = right != null ? right : new(null, null, null);
    }

    bool IsLeaf()
    {
        return (_left == null && _right == null) ? true : false;
    }

    void AddNode(AABB box)
    {
        // if this is a new empty node
        if (_node == null)
        {
        }
        else if (IsLeaf())
        {
            _left = new AABBTree(_node, _left, _right);
            _right = new AABBTree(_node, null, null);

            _node = _node.Merge(box);
        }
        else
        {
            AABB branchMerge = _node.Merge(box);
            double branchCost = branchMerge.GetVolume();
            double branchOverlapCost = _node.GetOverlapVolume(box);
            branchCost += branchOverlapCost;

            double leftCost;
            double rightCost;

            if (_left != null)
            {
                AABB leftMerge = _left._node!.Merge(box);
                leftCost = branchMerge.GetVolume() - _node.GetVolume();
                leftCost += leftMerge.GetVolume() - _left._node!.GetVolume();
                double leftOverlapCost = _right == null ? 0 : leftMerge.GetOverlapVolume(_right._node!);
                leftCost += leftOverlapCost;
            }
            else
            {
                leftCost = 0;
            }

            if (_right != null)
            {
                AABB rightMerge = _right._node!.Merge(box);
                rightCost = branchMerge.GetVolume() - _node.GetVolume();
                rightCost += rightMerge.GetVolume() - _right._node!.GetVolume();
                double rightOverlapCost = _left == null ? 0 : rightMerge.GetOverlapVolume(_left._node!);
                rightCost += rightOverlapCost;
            }
            else
            {
                rightCost = 0;
            }

            if (branchCost < leftCost && branchCost < rightCost)
            {
                _left = new AABBTree(_node, _left, _right);
                _right = new AABBTree(box, null, null);
            }
            else if (leftCost < rightCost)
            {
                _left.AddNode(box);
            }
            else
            {
                _right.AddNode(box);
            }

            _node = _left._node.Merge(_right._node);
        }
    }

}