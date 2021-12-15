using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    
    [SerializeField] private LayerMask _impassable;
    [SerializeField] private LayerMask _ground;
    public LayerMask Ground => _ground;
    public LayerMask Impassable => _impassable;
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var realWorldPosition = RayCastFromMouseToPlane();
            var worldPosition = Globe.WrapAround(realWorldPosition);
            if (!MainManager.Instance.PathFinding.IsWalkable(worldPosition)) return;

            foreach (var actor in _playerActors)
                if (actor.IsInView(out var actorScreenPosition) && actor.Selectable)
                    actor.SetDestination(worldPosition, realWorldPosition);
        }
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

public enum Faction
{
    Player,
    Enemy,
}