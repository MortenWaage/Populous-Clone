using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEditor.Experimental;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] [Range(0.05f, 5f)] private float _groundRaiseSpeed = 1f;
    [SerializeField] private Vector3 _testLocation = new Vector3(75, 1.5f, 10);
    [SerializeField] private Texture2D _heightMap;
    private Mesh _mesh;
    private int _worldX;
    private int _worldZ;
    private float[,] _terrainHeight;
    private int[] _triangles;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private bool _isOcean = true;  //-- Two MeshGenerators needs to be used until the curve shaders can be reworked to include multiple textures.
                            //-- This prevents us from modifying the ocean mesh accidentally

    public Vector3[] Vertices => _vertices;

    void Start()
    {
        _vertexModifiers = new List<VertexModifer>();

        MainManager.Instance.Terrain = this;
        Debug.Log($"{Globe.RunOrder} - MeshGenerator is Set");

        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        GenerateHeightData();

        GenerateMesh();
        GenerateUVs();
        UpdateMesh();

        if (TryGetComponent(out MeshCollider collider))
            collider.sharedMesh = _mesh;

        MainManager.Instance.MapLoaded = true;
        Debug.Log($"{Globe.RunOrder} - WorldMesh is Generated");
    }


    void Update()
    {
        if (_isOcean) return;
        
        SetVertexPositions();
    }

    void GenerateHeightData()
    {
        var hasMap = false;

        _worldX = MapData.WIDTH;
        _worldZ = MapData.HEIGHT;

        if (_heightMap != null)
        {
            _isOcean = false;
            Debug.Log(gameObject.name + " is Ocean");
            _worldX = _heightMap.width;
            _worldZ = _heightMap.height;
            hasMap = true;
        }

        Debug.Log($"{Globe.RunOrder} - Loading HeightMap of width {_worldX} and height {_worldZ}.");

        _terrainHeight = new float[_worldX, _worldZ];
        var heightMapData = Array.Empty<Color>();
        if (hasMap)
            heightMapData = _heightMap.GetPixels();
        

        Debug.Log($"{Globe.RunOrder} - Loading Colors of length { (hasMap ? heightMapData.Length.ToString() : "")}");

        for (int i = 0, z = 0; z < _worldZ; z++)
        for (var x = 0; x < _worldZ; x++, i++)
        {
            _terrainHeight[x, z] = hasMap ? WorldMesh.HeightFromColor(heightMapData[i]) : 0.9f;
        }

        Debug.Log($"{Globe.RunOrder} - Created Terrain Height Array of length {_terrainHeight.Length}");
    }

    void GenerateMesh()
    {
        _triangles = new int[_worldX * _worldZ * 6];
        _vertices  = new Vector3[(_worldX + 1) * (_worldZ + 1)];

        Debug.Log($"{Globe.RunOrder} - Creating {_triangles.Length} Triangles and {_vertices.Length} Vertices");

        for (int i = 0, z = 0; z <= _worldZ; z++)
        for (var x = 0; x <= _worldX; x++, i++)
        {
            var y = _terrainHeight[x % _worldX, z % _worldZ];
            y = WorldMesh.GetRandomHeight(x, y, z, _worldX, _worldZ);
            _vertices[i] = new Vector3(x, y, z);
        }

        var triangles = 0;
        var vertices = 0;

        for (var z = 0; z < _worldZ; z++) {
            for (var x = 0; x < _worldX; x++) {

                _triangles[triangles + 0] = vertices + 0;
                _triangles[triangles + 1] = vertices + _worldZ + 1;
                _triangles[triangles + 2] = vertices + 1;

                _triangles[triangles + 3] = vertices + 1;
                _triangles[triangles + 4] = vertices + _worldZ + 1;
                _triangles[triangles + 5] = vertices + _worldZ + 2;

                vertices++;
                triangles += 6;
            } 
            vertices++;
        }
    }

    void GenerateUVs()
    {
        _uvs = new Vector2[_vertices.Length];

        for (var i = 0; i < _uvs.Length; i++)
        {
            _uvs[i] = new Vector2(_vertices[i].x, _vertices[i].z);
        }
    }

    void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;

        _mesh.RecalculateNormals();

        if (TryGetComponent(out MeshCollider collider))
            collider.sharedMesh = _mesh;
    }

    [SerializeField] private bool _canModify;

    void SetVertexPositions()
    {
        if (!_canModify) return;




        if (_vertexModifiers.Count < 1) return;

        var modifiersToDelete = new List<VertexModifer>();

        foreach (var modifier in _vertexModifiers)
        {
            var modificationSpeed = modifier.Direction * Time.deltaTime * _groundRaiseSpeed;
            if (modifier.ModifyHeight(modificationSpeed, out int secondIndex))
            {
                _vertices[secondIndex].y = modifier.Position.y;
            }

            if (modifier.Delete) modifiersToDelete.Add(modifier);

            _vertices[modifier.Index] = modifier.Position;
        }

        foreach (var modifier in modifiersToDelete.Where(modifier => _vertexModifiers.Contains(modifier)))
        {
            _vertexModifiers.Remove(modifier);
        }

        UpdateMesh();

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (var modifiers in _vertexModifiers)
        {
            Gizmos.DrawSphere(modifiers.Position, 0.1f);
        }
    }

    private List<VertexModifer> _vertexModifiers;
    const int LAND_FLATTEN_SIZE = 2;

    public void CalculateAffectedVertices(Vector3 point)
    {
        if (!_canModify) _vertexModifiers.Clear();


        MainManager.Instance.PathFinding.RecalculateGround(); //-- Danger having this here.



        point = Globe.WrapAround(point);
        var wrappedPoint = new Vector3Int(Mathf.RoundToInt(point.x), 0, Mathf.RoundToInt(point.z));

        var modifier = new VertexModifer(WorldMesh.IndexFromPoint(wrappedPoint));
        modifier.GridPosition = wrappedPoint;
        _vertexModifiers.Add(modifier);

        var count = 0;
        for (var x = -LAND_FLATTEN_SIZE; x <= LAND_FLATTEN_SIZE; x++)
        for (var z = -LAND_FLATTEN_SIZE; z <= LAND_FLATTEN_SIZE; z++)
        {
            if (x == 0 && z == 0) continue;

            var adjacent = new Vector3Int(wrappedPoint.x + x, 0, wrappedPoint.z + z);
            adjacent = Globe.WrapVertex(adjacent);

            var adjacentModifier = new VertexModifer(WorldMesh.IndexFromPoint(adjacent))
            {
                DesiredHeight = modifier.DesiredHeight,
                GridPosition = adjacent,
                Parent = modifier,
            };
            _vertexModifiers.Add(adjacentModifier);
            count++;
        }

        Debug.Log($"Added {count} modifiers. Total modifiers in list is {_vertexModifiers.Count}");
    }
}

public class VertexModifer
{
    public VertexModifer Parent;
    public bool Delete;
    public int Index;
    public float DesiredHeight;
    public Vector3 Position;
    public Vector3Int GridPosition;

    public int Direction => DesiredHeight > Position.y ? 1 : -1;

    public VertexModifer(int index)
    {
        Parent = this;
        Index = index;
        Delete = false;
        Position = MainManager.Instance.Terrain.Vertices[index];

        DesiredHeight = Position.y;
    }
    public bool ModifyHeight(float adjustment, out int secondIndex)
    {
        Position.y += adjustment;
        Delete = Mathf.Abs(DesiredHeight - Position.y) < 0.01f;
        Position.y = Delete ? Position.y = DesiredHeight : Position.y;

        //var totalVertexArrayLength = (MapData.WIDTH + 1) * (MapData.HEIGHT + 1);

        Vector3Int thisPosition = new Vector3Int(Mathf.RoundToInt(Position.x), 0, Mathf.RoundToInt(Position.z));
        var secondGridPosition = thisPosition;//GridPosition;

        if      (GridPosition.x == 0)             secondGridPosition.x = MapData.WIDTH;
        else if (GridPosition.x == MapData.WIDTH) secondGridPosition.x = 0;


        if      (GridPosition.z == 0)              secondGridPosition.z = MapData.HEIGHT;
        else if (GridPosition.z == MapData.HEIGHT) secondGridPosition.z = 0;


        if (secondGridPosition != GridPosition)
        {
            secondIndex = WorldMesh.IndexFromPoint(secondGridPosition);
            Debug.Log($"Node indexed {Index} at position {GridPosition}, Outputting second modifier at index {secondIndex} and position {secondGridPosition}");
            return true;
        }

        //if (Index % MapData.WIDTH+1 == 0)
        //{
        //    secondIndex = Index - MapData.WIDTH+1;
        //    if (Index == 0) secondIndex = totalVertexArrayLength - MapData.WIDTH+1;
        //    return true;
        //}
        secondIndex = 0;
        return false;
    }
}
public static class WorldMesh
{
    //-- Should probably be using greyscale and lerp values between a Minimum and Maximum height value from the combined RGB values instead of dipping into if/else-hell
    public static float HeightFromColor(Color color)
    {
        if (color.b > 0.75f && color.r < 0.5f && color.g < 0.5f) return 0.5f;
        else if (color.r > 0.75f && color.g > 0.75f && color.b > 0.75f) return 3.50f;
        else if (color.g > 0.11f && color.b < 0.1f) return 1.5f;
        else if (color.r > 0.75f) return 2.75f;
        else if (color.r > 0.1f && color.g > 0.1f && color.b > 0.1f) return 2.75f;
        return 2.25f;
    }

    public static float GetRandomHeight(float x, float y, float z, int width, int height)
    {
        if (x != 0 && x >= width && z != 0 && z >= height)
        {
            if (y >= 1.45f && y < 2f)
                y = Random.Range(1.40f, 1.60f);
            if (y >= 2.70f && y < 3f)
                y = Random.Range(2.45f, 3f);
            if (y >= 3.0f)
                y = Random.Range(3f, 3.75f);
        }

        return y;
    }

    public static int IndexFromPoint(Vector3Int point)
    {
        return point.z * MapData.WIDTH + point.x + (point.z * 1);
    }
}