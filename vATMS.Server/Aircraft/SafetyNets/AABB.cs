internal class AABB
{
    public (double, double)[] _bounds;

    public AABB(params (double min, double max)[] bounds)
    {
        //(min, max)
        _bounds = bounds.Length > 0 ? bounds : Array.Empty<(double, double)>();
    }

    public bool IsOverlapping(AABB box)
    {
        // all AABBs will? should? have the same dimensions, so this should be safe

        for (int i = 0; i < box._bounds.Length; i++)
        {
            (double min1, double max1) = _bounds[i];
            (double min2, double max2) = box._bounds[i];

            if (min1 >= max2 || min2 >= max1) return false;
        }


        return true;
    }

    public double GetOverlapVolume(AABB box)
    {
        double volume = 1;

        for (int i = 0; i < box._bounds.Length; i++)
        {
            (double min1, double max1) = _bounds[i];
            (double min2, double max2) = box._bounds[i];

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
            (double min, double max) = _bounds[i];

            volume *= max - min;
        }

        return volume;
    }

    public AABB Merge(AABB box)
    {
        AABB newAABB = new(new (double, double)[box._bounds.Length]);

        for (int i = 0; i < box._bounds.Length; i++)
        {
            (double min1, double max1) = _bounds[i];
            (double min2, double max2) = box._bounds[i];

            double newMin = Math.Min(min1, min2);
            double newMax = Math.Max(max1, max2);
            newAABB._bounds[i] = (newMin, newMax);
        }

        return newAABB;
    }
}

internal class AABBTree
{
    AABB _node;
    AABBTree? _left;
    AABBTree? _right;

    public AABBTree(
        AABB aabb,
        AABBTree? left = null,
        AABBTree? right = null
    )
    {
        _node = aabb;
        _left = left;
        _right = right;
    }

    bool IsLeaf()
    {
        return (_left == null && _right == null) ? true : false;
    }

    public void AddNode(AABB box)
    {
        if (IsLeaf())
        {
            _left = new AABBTree(_node, _left, _right);
            _right = new AABBTree(box);

            _node = _node.Merge(box);
        }
        else
        {
            if (_left == null)
                _left = new AABBTree(_node);

            if (_right == null)
                _right = new AABBTree(_node);

            AABB branchMerge = _node.Merge(box);
            double branchCost = branchMerge.GetVolume();
            double branchOverlapCost = _node.GetOverlapVolume(box);
            branchCost += branchOverlapCost;

            AABB leftMerge = _left!._node.Merge(box);
            double leftCost = branchMerge.GetVolume() - _node.GetVolume();
            leftCost += leftMerge.GetVolume() - _left._node.GetVolume();
            double leftOverlapCost = _right == null ? 0 : leftMerge.GetOverlapVolume(_right._node!);
            leftCost += leftOverlapCost;

            AABB rightMerge = _right!._node.Merge(box);
            double rightCost = branchMerge.GetVolume() - _node.GetVolume();
            rightCost += rightMerge.GetVolume() - _right._node.GetVolume();
            double rightOverlapCost = _left == null ? 0 : rightMerge.GetOverlapVolume(_left._node!);
            rightCost += rightOverlapCost;


            if (branchCost < leftCost && branchCost < rightCost)
            {
                _left = new AABBTree(_node, _left, _right);
                _right = new AABBTree(box);
            }
            else if (leftCost < rightCost)
            {
                _left!.AddNode(box);
            }
            else
            {
                _right!.AddNode(box);
            }

            _node = _left!._node.Merge(_right._node);
        }
    }

    public List<(AABB, AABB)> GetOverlappingPairs()
    {
        return GetOverlaps(this, this);
    }

    List<(AABB, AABB)> GetOverlaps(AABBTree tree1, AABBTree tree2)
    {
        List<(AABB, AABB)> overlaps = new();

        if (!tree1._node.IsOverlapping(tree2._node))
        {
            return overlaps;
        }

        if (tree1.IsLeaf() && tree2.IsLeaf())
        {
            overlaps.Add((tree1._node, tree2._node));
            return overlaps;
        }

        if (tree1.IsLeaf())
        {
            if (tree2._left != null) overlaps.AddRange(GetOverlaps(tree1, tree2._left));
            if (tree2._right != null) overlaps.AddRange(GetOverlaps(tree1, tree2._right));
        }
        else if (tree2.IsLeaf())
        {
            if (tree1._left != null) overlaps.AddRange(GetOverlaps(tree2, tree1._left));
            if (tree1._right != null) overlaps.AddRange(GetOverlaps(tree2, tree1._right));
        }
        else
        {
            if (tree1._left != null && tree2._left != null) overlaps.AddRange(GetOverlaps(tree1._left, tree2._left));
            if (tree1._right != null && tree2._right != null) overlaps.AddRange(GetOverlaps(tree1._right, tree2._right));
            if (tree1._right != null && tree2._left != null) overlaps.AddRange(GetOverlaps(tree1._right, tree2._left));
            if (tree1._left != null && tree2._right != null) overlaps.AddRange(GetOverlaps(tree1._left, tree2._right));
        }

        return overlaps;
    }

    List<AABB> OverlapDFS(AABBTree inTree, AABBTree curTree)
    {
        List<AABB> overlappingAABB = new();
        List<AABBTree> inBranches = new();
        List<AABBTree> treeBranches = new();

        if (inTree.IsLeaf())
        {
            inBranches.Add(inTree);
        }
        else
        {
            if (inTree._left != null) inBranches.Add(inTree._left);
            if (inTree._right != null) inBranches.Add(inTree._right);
        }

        if (curTree.IsLeaf())
        {
            inBranches.Add(curTree);
        }
        else
        {
            if (curTree._left != null) inBranches.Add(curTree._left);
            if (curTree._right != null) inBranches.Add(curTree._right);
        }

        if (!inTree._node.IsOverlapping(curTree._node))
            return overlappingAABB;

        if (inTree.IsLeaf() && curTree.IsLeaf())
        {
            overlappingAABB.Add(inTree._node);
            return overlappingAABB;
        }

        foreach (AABBTree inBranch in inBranches)
        {
            foreach (AABBTree treeBranch in treeBranches)
            {
                List<AABB> overlaps = OverlapDFS(inBranch, treeBranch);
                overlappingAABB.AddRange(overlaps);
            }
        }

        return overlappingAABB;
    }

}