using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Actor : MonoBehaviour
{
    [SerializeField] bool _selectable;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private Vector3 _destination;
    private Vector3 _finalDestination; //--Remove Serialization Later
    private float _minimumMoveDistance = 1.4f;


    void Awake()
    {
        _path = new List<PathNode>();
    }
    void Start()
    {
        _destination = transform.position;
        StartCoroutine("AwaitFrame");
    }

    IEnumerator AwaitFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.UnitManager.AddToFaction(Faction.Player, this);
    }

    void Update()
    {
        MoveToDestination();
    }

    public bool Selectable => _selectable;
    private List<PathNode> _path;
    public void SetDestination(Vector3 destination, Vector3 unwrappedDestination)
    {
        _finalDestination = destination;
        if (Vector3.Distance(transform.position, unwrappedDestination) > MapData.WIDTH * 0.5f)
        {

            if (transform.position.z < MapData.HEIGHT * 0.5f)
                 unwrappedDestination.z = 0 - MapData.HEIGHT - unwrappedDestination.z;
            else unwrappedDestination.z = MapData.HEIGHT + unwrappedDestination.z;


            if (transform.position.x < MapData.WIDTH * 0.5f)
                 unwrappedDestination.x = 0 - MapData.WIDTH - unwrappedDestination.x;
            else unwrappedDestination.x = MapData.WIDTH + unwrappedDestination.x;

        }
        MainManager.Instance.PathFinding.FindPath(transform.position, destination, unwrappedDestination, out var path);
        _path = path;
        if (_path.Count < 1) return;
        _destination = _path[0].Position;
    }

    private void MoveToDestination()
    {
        if (_path.Count < 1) _destination = _finalDestination;

        var destinationNoY = new Vector3(_destination.x, 0, _destination.z);
        var positionNoY = new Vector3(transform.position.x, 0, transform.position.z);
        var distanceToNode = Globe.Distance(positionNoY, destinationNoY);

        MainManager.Instance.UI.Text(0, $"{(_path.Count > 0 ? $"Next Node Position: {_path[0].Position}" : "No Destination Given")}");
        MainManager.Instance.UI.Text(1, $"{(_path.Count > 0 ? $"Distance to next node: {distanceToNode:#.##}" : "Reached Destination")}");
        MainManager.Instance.UI.Text(2, $"{(_path.Count > 0 ? $"Nodes Remaining on Path: {_path.Count}" : "")}");

        if (distanceToNode < _minimumMoveDistance && _path.Count > 0)
        {
            _path.RemoveAt(0);
            if (_path.Count < 1) return;
            _destination = _path[0].Position;
        }
        
        var destination = CalculateShortestPath();
        var direction = (destination - transform.position).normalized;
        var finalPosition = transform.position + direction * _moveSpeed * Time.deltaTime;

        finalPosition      = Globe.WrapAround(finalPosition);
        finalPosition.y    = GetGroundHeight(finalPosition);
        transform.position = finalPosition;
    }

    Vector3 CalculateShortestPath()
    {
        var xDistance = (_destination.x - transform.position.x);
        var yDistance = (_destination.z - transform.position.z);
        var destination = _destination;

        if (xDistance > MapData.WIDTH * 0.5f && transform.position.x < MapData.WIDTH * 0.5f) {
            destination.x = (MapData.WIDTH - (transform.position.x + xDistance)) * -1;
        }
        else if (Mathf.Abs(xDistance) > MapData.WIDTH * 0.5f && transform.position.x >= MapData.WIDTH * 0.5f) {
            destination.x = MapData.WIDTH + _destination.x;
        }

        if (yDistance > MapData.HEIGHT * 0.5f) {
            destination.z = (MapData.HEIGHT - (transform.position.z + yDistance)) * -1;
        }
        else if (Mathf.Abs(yDistance) > MapData.HEIGHT * 0.5f && transform.position.z >= MapData.HEIGHT * 0.5f) {
            destination.z = MapData.HEIGHT + _destination.z;
        }

        return destination;
    }

    float GetGroundHeight(Vector3 position)
    {
        Physics.Raycast(position + Vector3.up * MapData.MAX_TERRAIN_HEIGHT, Vector3.down, out RaycastHit hit,
            Mathf.Infinity, MainManager.Instance.UnitManager.Ground);

        return hit.point.y;
    }

    [Obsolete("Ground Height is gotten with GetGroundHeight(Vector3 position) method instead")]
    private bool GetGroundHeight(Vector3 position, out float groundHeight)
    {
        Physics.Raycast(Globe.WrapAround(position) + Vector3.up * MapData.MAX_TERRAIN_HEIGHT, Vector3.down, out RaycastHit hit,
            Mathf.Infinity, MainManager.Instance.UnitManager.Ground);
        if (hit.point.y > 0.99f)
        {
            groundHeight = hit.point.y;
            return true;
        }

        groundHeight = position.y;
        Debug.Log(Globe.WrapAround(position));
        return false;
    }


    public bool IsInView(out Vector3 pointOnScreen)
    {
        pointOnScreen = MainManager.Instance.CameraController.MainCamera.WorldToScreenPoint(transform.position);
        return !(pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
               (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height);
    }
}
