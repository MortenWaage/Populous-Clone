using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Actor : MonoBehaviour
{
    [SerializeField] private bool _selectable;
    [SerializeField] private float _moveSpeed = 1.5f;
    private Vector3 _destination;
    private Vector3 _finalDestination;
    private float _minimumMoveDistance = 1.4f;
    private List<PathNode> _path;
    public bool Selectable => _selectable;
    
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
        if (!MainManager.Instance.Ready) return;

        MoveToDestination();

        MoveTheFakeGizmo();
    }

    public void SetDestination(Vector3 destination, Vector3 unwrappedDestination)
    {
        _finalDestination = destination;
        unwrappedDestination = Globe.WrapDestination(transform.position, unwrappedDestination);

        MainManager.Instance.PathFinding.FindPath(transform.position, destination, unwrappedDestination, out var path);
        _path = path;
        if (_path.Count < 1) return;
        _destination = _path[0].Position;

        ActivateFakeGizmo();
    }

    private void MoveToDestination()
    {
        if (_path.Count < 1)
        {
            _destination = _finalDestination;
            _fakeGizmo.gameObject.SetActive(false);
            _fakeGizmoEnd.gameObject.SetActive(false);
        }

        var destinationNoY = new Vector3(_destination.x, 0, _destination.z);
        var positionNoY = new Vector3(transform.position.x, 0, transform.position.z);
        var distanceToNode = Globe.CompareDistance(positionNoY, destinationNoY);

        UIAddMovementData();

        if (distanceToNode < _minimumMoveDistance && _path.Count > 0)
        {
            _path.RemoveAt(0);
            if (_path.Count < 1) return;
            _destination = _path[0].Position;
        }
        
        var destination = Globe.CalculateShortestPath(transform.position, _destination);
        var direction = (destination - transform.position).normalized;
        var finalPosition = transform.position + direction * _moveSpeed * Time.deltaTime;

        finalPosition      = Globe.WrapAround(finalPosition);
        finalPosition.y    = Globe.GetGroundHeight(finalPosition);
        transform.position = finalPosition;

        void UIAddMovementData()
        {
            MainManager.Instance.UI.Text(0, $"{(_path.Count > 0 ? $"Next Node Position: {_path[0].Position}" : "No Destination Given")}");
            MainManager.Instance.UI.Text(1, $"{(_path.Count > 0 ? $"Distance to next node: {distanceToNode:#.##}" : "Reached Destination")}");
            MainManager.Instance.UI.Text(2, $"{(_path.Count > 0 ? $"Nodes Remaining on Path: {_path.Count}" : "")}");
        }
    }



    public bool IsInView(out Vector3 pointOnScreen)
    {
        pointOnScreen = Vector3.zero;
        return true;    //--Always return true until a proper unity manager class and drag selection can be implemented
                        //--Keep the below code. We'll need it later

        pointOnScreen = MainManager.Instance.CameraController.MainCamera.WorldToScreenPoint(transform.position);
        return (pointOnScreen.x >= 0) && (pointOnScreen.x <= Screen.width) ||
               (pointOnScreen.y >= 0) && (pointOnScreen.y <= Screen.height);
    }


    #region Fake Gizmo - Temporary
    //-- Fake Gizmo Visualize the actors current path through each of the node in the actors current path list.
    //-- Will give visual feedback on the actors path outside of the Unity Editor.

    private int _currentGizmoNode = 0;
    [SerializeField] private float _fakeGizmoSpeed = 2.5f;
    [SerializeField] private GameObject _fakeGizmo;
    [SerializeField] private GameObject _fakeGizmoEnd;
    private float _gizmoTimer = 1;

    private void ActivateFakeGizmo()
    {
        _currentGizmoNode = 0;
        _fakeGizmo.gameObject.SetActive(true);
        _fakeGizmoEnd.gameObject.SetActive(true);
        _fakeGizmo.transform.position = transform.position;

        Vector3 finalPosition = _path[_path.Count - 1].Position;
        finalPosition.y = Globe.GetGroundHeight(finalPosition) + 0.5f;
        _fakeGizmoEnd.transform.position = finalPosition;
    }
    private void MoveTheFakeGizmo()
    {
        if (_path.Count < 1) return;

        _gizmoTimer -= _fakeGizmoSpeed * Time.deltaTime;
        if (_gizmoTimer > 0) return;

        _gizmoTimer = 1;
        _currentGizmoNode++;

        if (_currentGizmoNode >= _path.Count)
            _currentGizmoNode = 0;

        MainManager.Instance.UI.Text(3, $"_currentGizmoNode {_currentGizmoNode}::_path.Count {_path.Count}");

        Vector3 finalPosition = _path[_currentGizmoNode].Position;
        finalPosition.y = Globe.GetGroundHeight(finalPosition) + 0.5f;
        _fakeGizmo.transform.position = finalPosition;
    }
    #endregion
}

