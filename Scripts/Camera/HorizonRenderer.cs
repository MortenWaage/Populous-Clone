using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HorizonRenderer : MonoBehaviour
{
    [SerializeField] Camera _cam;
    [SerializeField] CameraMovementController _cameraPivot;
    private readonly float _horizonDistance = MapData.HORIZON_RENDER_DISTANCE;
    [SerializeField] private Directions _direction;

    [Header("Main Camera Data")] 
    public string position_inspector;
    private Vector3 _directionToPoint;
    private string PositionText => $"{(_north ? "North" : "South")}-{(_east ? "East" : "West")}";
    private bool _north;
    private bool _east;
    public int X;
    public int Z;
    //-- End of Header

    private bool PointingEast => _east = _cam.transform.position.x > MapData.WIDTH * 0.5f;
    private bool PointingNorth => _north = _cam.transform.position.z > MapData.HEIGHT * 0.5f;

    void Awake()
    {
        StartCoroutine("AwaitEndOfFrame");
    }
    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.AddHorizonCam(this);
    }

    void Update()
    {
        transform.rotation = _cameraPivot.transform.rotation;
        transform.position = _cameraPivot.transform.position + _directionToPoint;

        #region Set Inspector Displayed Data
        position_inspector = PositionText;
        X = (int)_cam.transform.position.x;
        Z = (int)_cam.transform.position.z;
        #endregion
    }

    public void SetPosition(Vector3 position)
    {
        var pointingEast  =_east   = position.x > MapData.WIDTH * 0.5f;
        var pointingNorth = _north = position.z > MapData.HEIGHT * 0.5f;

        _directionToPoint = _horizonDistance * _direction switch
        {
            Directions.Diagonal   => new Vector3(pointingEast ? -1 : 1, 0, pointingNorth ? -1 : 1),
            Directions.NorthSouth => new Vector3(0, 0, pointingNorth ? -1 : 1),
            Directions.EastWest   => new Vector3(pointingEast ? -1 : 1, 0, 0),
            _ => Vector3.zero
        };
    }
}
