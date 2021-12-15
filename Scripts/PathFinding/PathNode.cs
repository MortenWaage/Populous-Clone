using UnityEngine;
public class PathNode
{
    public PathNode Parent;
    public bool Walkable;
    public Vector3Int Position;

    public int FCost => gCost + hCost;
    public int gCost;
    public int hCost;

    public PathNode(bool walkable, Vector3Int position)
    {
        Walkable = walkable;
        Position = position;
    }
}
