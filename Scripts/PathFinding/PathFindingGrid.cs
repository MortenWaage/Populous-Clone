using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathFindingGrid : MonoBehaviour
{
    private bool[,] _worldPaths;
    private bool _hasCalculatedPaths = false;

    private PathNode[,] _worldNodes;

    void Awake()
    {
        StartCoroutine("AwaitEndOfFrame");
    }
    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.PathFinding = this;
        Debug.Log($"{Globe.RunOrder} - PathFindingGrid is set");
    }
    void Start()
    {
        _worldPaths = new bool[MapData.WIDTH,MapData.HEIGHT];
        _worldNodes = new PathNode[MapData.WIDTH, MapData.HEIGHT];
        StartCoroutine("CalculateWalkableGround");
    }

    IEnumerator CalculateWalkableGround()
    {
        for (var z = 0; z < MapData.HEIGHT; z++)
        for (var x = 0; x < MapData.WIDTH; x++)
        {
            _worldPaths[x, z] = TestGroundForWalkability(x, z);
        }

        for (var z = 0; z < MapData.HEIGHT; z++)
        for (var x = 0; x < MapData.WIDTH; x++)
        {
            _worldNodes[x, z] = new PathNode(_worldPaths[x, z], new Vector3Int(x, 0, z));
        }
        
        yield return new WaitForSeconds(1);
        _hasCalculatedPaths = true;
    }

    private bool TestGroundForWalkability(int x, int z)
    {
        var position = new Vector3(x, 0, z) + (Vector3.up * MapData.MAX_TERRAIN_HEIGHT);
        Physics.Raycast(position, Vector3.down, out RaycastHit hit, Mathf.Infinity);
        return hit.point.y > MapData.MIN_TERRAIN_HEIGHT && hit.point.y < 2f;
    }


    public bool IsWalkable(Vector3 position)
    {
        var x = Mathf.FloorToInt(position.x);
        var z = Mathf.FloorToInt(position.z);

        return _worldPaths[x, z];
    }

    private PathNode NodeFromWorldPoint(Vector3 position)
    {
        var x = Mathf.FloorToInt(position.x);
        var z = Mathf.FloorToInt(position.z);

        return _worldNodes[x, z];
    }

    #region Pathfinding Methods
    public void FindPath(Vector3 startPos, Vector3 targetPos, Vector3 unwrappedTargetPos, out List<PathNode> path) //-- Checked Method 
    {
        PathNode startNode = NodeFromWorldPoint(startPos);
        PathNode targetNode = NodeFromWorldPoint(targetPos);
        path = new List<PathNode>();

        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet[0];

            for (var i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost > currentNode.FCost) continue;
                if (openSet[i].hCost < currentNode.hCost)
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                path = ReTracePath(startNode, targetNode);
                return;
            }

            foreach (PathNode adjacentNode in GetNeighbors(currentNode))
            {
                if (!adjacentNode.Walkable || closedSet.Contains(adjacentNode)) continue;

                var newMovementCostToNode = currentNode.gCost + GetDistance(currentNode, adjacentNode, unwrappedTargetPos);
                if (newMovementCostToNode >= adjacentNode.gCost && openSet.Contains(adjacentNode)) continue;

                adjacentNode.gCost = newMovementCostToNode;
                adjacentNode.hCost = GetDistance(adjacentNode, targetNode, unwrappedTargetPos);
                adjacentNode.Parent = currentNode;

                if (!openSet.Contains(adjacentNode))
                {
                    openSet.Add(adjacentNode);
                }
            }
        }
    }

    int GetDistance(PathNode nodeA, PathNode nodeB, Vector3 unwrappedTargetPos)
    {
        var origin = new Vector3(nodeA.Position.x, 0, nodeA.Position.z);
        var destination = new Vector3(nodeB.Position.x, 0, nodeB.Position.z);
        var distanceLessThanHalfMapSize = Vector3.Distance(origin, destination) < MapData.HEIGHT * 0.5f;
        
        if (distanceLessThanHalfMapSize)
        {
            var distanceX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
            var distanceZ = Mathf.Abs(nodeA.Position.z - nodeB.Position.z);

            return distanceX > distanceZ
                ? 14 * distanceZ + 10 * (distanceX - distanceZ)
                : 14 * distanceX + 10 * (distanceZ - distanceX);
        }
        else
        {
            var distanceX = Mathf.Abs(nodeA.Position.x - (int) unwrappedTargetPos.x);
            var distanceZ = Mathf.Abs(nodeA.Position.z - (int) unwrappedTargetPos.z);

            return distanceX > distanceZ
                ? 14 * distanceZ + 10 * (distanceX - distanceZ)
                : 14 * distanceX + 10 * (distanceZ - distanceX);
        }
    }

    List<PathNode> ReTracePath(PathNode startingNode, PathNode endPathNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endPathNode;

        while (currentNode != startingNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        GizmoDisplayedPath = path;

        return path;
    }

    public List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> adjacentNodes = new List<PathNode>();

        for (var x = -1; x <= 1; x++)
        for (var z = -1; z <= 1; z++)
        {
            if (x == 0 & z == 0) continue;
            var position = Globe.WrapAround(node.Position + new Vector3(x, 0, z));
            adjacentNodes.Add(NodeFromWorldPoint(position));
        }

        return adjacentNodes;
    }

    #endregion
    
    private PathNode _selectedNode;
    private Vector3 _selectedNodePosition = new Vector3(50, 0, 2); //approximately at spawn

    void Update()
    {
        //--Used for moving the Gizmo
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selectedNodePosition.z += 1;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selectedNodePosition.z -= 1;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _selectedNodePosition.x -= 1;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _selectedNodePosition.x += 1;
        }

        var position = Globe.WrapAround(_selectedNodePosition);
        _selectedNode = NodeFromWorldPoint(position);
        _selectedNodePosition = position;

        int x = (int) _selectedNodePosition.x, z = (int) _selectedNodePosition.z;

        _selectedNode = _worldNodes[x, z];
    }

    private List<PathNode> GizmoDisplayedPath;
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!_hasCalculatedPaths) return;

        #region Draw Walkable Area
        var origin = MainManager.Instance.CameraController.MainCameraPivot.position;
        var distance = 8;

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(MainManager.Instance.CameraController.MainCameraPivot.position, 0.2f);
        Gizmos.color = Color.red;

        for (var z = (int)origin.z - distance; z < (int)origin.z + distance; z++)
        for (var x = (int)origin.x - distance; x < (int)origin.x + distance; x++)
        {
            var worldPosition = Globe.WrapAround(new Vector3Int(x, 2, z));

            var gridX = (int)worldPosition.x;
            var gridZ = (int)worldPosition.z;
            if (!_worldPaths[gridX, gridZ]) continue;
            Gizmos.DrawSphere(worldPosition, 0.1f);
        }
        #endregion
        #region Draw Shortest Path
        Gizmos.color = Color.green;
        if (GizmoDisplayedPath != null)
        {
            foreach (PathNode node in GizmoDisplayedPath)
            {
                var position = node.Position;
                position.y = 2;
                Gizmos.DrawSphere(position, 0.2f);
            }
        }
        #endregion
        #region Draw Active Node
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_selectedNodePosition + Vector3.up * 2f, 0.25f);

        Gizmos.color = Color.blue;
        foreach(var node in GetNeighbors(_selectedNode))
            Gizmos.DrawSphere(node.Position + Vector3.up * 2f, 0.22f);
        #endregion
    }
}
