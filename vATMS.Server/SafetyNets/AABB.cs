/// <summary>
/// Implementation of an AABB (Axis-aligned bounding box)
/// </summary>
internal class AABB
{
    private (double, double)[] _bounds;

    public string? Callsign;

    public AABB(string? callsign, params (double min, double max)[] bounds)
    {
        //(min, max)
        _bounds = bounds.Length > 0 ? bounds : Array.Empty<(double, double)>();
        Callsign = callsign;
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
        AABB newAABB = new(null, new (double, double)[box._bounds.Length]);

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

    /// <summary>
    /// Gets the geometric centre of an AABB
    /// The AABB is expected to have lat and lon stored in its first two indices ([0], [1]) respectively
    /// </summary>
    /// <returns>the centre coordinates</returns>
    public DecimalCoordinate GetCentre()
    {
        if (_bounds.Length < 2) throw new ArgumentOutOfRangeException("AABB size is smaller than expected, may be invalid");

        double lat = (_bounds[0].Item1 + _bounds[0].Item2) / 2;
        double lon = (_bounds[1].Item1 + _bounds[1].Item2) / 2;

        return new(lat, lon);
    }

    /// <summary>
    /// Gets starting time bounds of an AABB
    /// The AABB is expected to have times stored in its fourth index ([3])
    /// </summary>
    /// <returns>the start time</returns>
    public DateTime GetStartTime()
    {
        if (_bounds.Length < 4) throw new ArgumentOutOfRangeException("AABB size is smaller than expected, may be invalid");

        return DateTime.FromOADate(_bounds[3].Item1);
    }
}


/// <summary>
/// Implementation of an AABB tree
/// </summary>
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

    /// <summary>
    /// Adds a node to the AABB tree
    /// The cost of adding the node is based on the total bounding and overlap volume of the AABBS
    /// see https://github.com/kip-hart/AABBTree/blob/master/aabbtree.py
    /// </summary>
    /// <param name="box"></param>
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

        return GetUniquePairs(overlaps);
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
            treeBranches.Add(curTree);
        }
        else
        {
            if (curTree._left != null) treeBranches.Add(curTree._left);
            if (curTree._right != null) treeBranches.Add(curTree._right);
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

    /// <summary>
    /// Get unique AABB pairs which overlap, from a list of AABB pairs which overlap
    /// Note that this is not a filter of callsign pairs, only AABB pairs
    /// </summary>
    /// <param name="pairs"></param>
    /// <returns>the required list of AABBs</returns>
    private List<(AABB, AABB)> GetUniquePairs(List<(AABB, AABB)> pairs)
    {
        List<(AABB, AABB)> unique = new();
        HashSet<(AABB, AABB)> seen = new();
        foreach (var p in pairs)
        {
            if (seen.Contains(p) || seen.Contains((p.Item2, p.Item1)) || p.Item1 == p.Item2)
                continue;
            seen.Add(p);
            unique.Add(p);
        }
        return unique;
    }
}
