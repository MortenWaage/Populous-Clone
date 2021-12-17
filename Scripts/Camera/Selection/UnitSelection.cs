using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Faction
{
    Player,
    Enemy,
}

public class UnitSelection : MonoBehaviour
{
    [SerializeField] private LayerMask _ground;
    public LayerMask Ground => _ground;
    private List<Actor> _playerActors;

    void Awake()
    {
        _playerActors = new List<Actor>();
        StartCoroutine("AwaitEndOfFrame");
    }

    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.UnitManager = this;
        Debug.Log($"{Globe.RunOrder} - UnitSelection is set");
    }

    private void Start()
    {
        foreach(var actor in _playerActors)
            actor.gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            var realWorldPosition = RayCastFromMouseToPlane();
            var worldPosition = Globe.WrapAround(realWorldPosition);
            if (!MainManager.Instance.PathFinding.IsWalkable(worldPosition)) return;

            foreach (var actor in _playerActors)
                if (actor.IsInView(out var actorScreenPosition) && actor.Selectable)
                    actor.SetDestination(worldPosition, realWorldPosition);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (_hasGround)
                MainManager.Instance.Terrain.CalculateAffectedVertices(_cursorIndicator.transform.position);
            else
            {
                var realWorldPosition = RayCastFromMouseToPlane();
                MainManager.Instance.Terrain.CalculateAffectedVertices(realWorldPosition);
            }
        }

        MoveCursorIndicator();
    }
    [SerializeField] GameObject _cursorIndicator;
    private bool _hasGround;
    private void MoveCursorIndicator()
    {
        _hasGround = RayCastFromMouseToGround(out var worldPoint);
        if (_hasGround) _cursorIndicator.transform.position = worldPoint;
    }

    public bool RayCastFromMouseToGround(out Vector3 point)
    {
        point = Vector3.zero;
        if (!MainManager.Instance.Ready) return false;
        //--Raycast using Ground
        
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, MainManager.Instance.UnitManager.Ground))
        {
            point = Globe.WrapAround(hit.point);
            return true;
        }

        return false;
    }
    public Vector3 RayCastFromMouseToPlane()
    {
        //--Raycast using Plane
        var movementPlane = new Plane(Vector3.down, MapData.MIN_TERRAIN_HEIGHT);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (movementPlane.Raycast(ray, out var distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    public void AddToFaction(Faction faction, Actor actor)
    {
        _playerActors.Add(actor);
    }
}