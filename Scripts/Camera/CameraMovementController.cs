using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CameraMovementController : MonoBehaviour
{
    private Camera _cam;

    [SerializeField] GameObject _mesh;
    [SerializeField] private SkyboxRotation _skyBox;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _rotationSpeed = 5;

    public Camera MainCamera => _cam;
    public Transform MainCameraPivot => _mesh.transform;

    void Awake()
    {
        _cameraMode = CameraMode.Manual;
        StartCoroutine("AwaitEndOfFrame");
    }

    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.CameraController = this;
        Debug.Log($"{Globe.RunOrder} - CameraController is set");
    }
    IEnumerator InitializeCameraPosition()
    {
        yield return new WaitForEndOfFrame();
        Globe.UpdateHorizonCameras(transform.position);
        Debug.Log($"{Globe.RunOrder}Initializing Horizon Camera Positions");
    }
    void Start()
    {
        _cam = Camera.main;
        StartCoroutine("InitializeCameraPosition");
    }

    enum CameraMode
    {
        Auto,
        Manual,
    }

    private CameraMode _cameraMode;
    void Update()
    {
        switch (_cameraMode)
        {
            case CameraMode.Auto:   UpdateAuto();   break;
            case CameraMode.Manual: UpdateManual(); break;
            default: break;
        }
    }

    void LateUpdate()
    {

    }

    private void UpdateManual()
    {
        var velocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            velocity += transform.forward;

        if (Input.GetKey(KeyCode.Q))
            velocity -= transform.right;

        if (Input.GetKey(KeyCode.S))
            velocity -= transform.forward;

        if (Input.GetKey(KeyCode.E))
            velocity += transform.right;

        if (Input.GetKey(KeyCode.A))
            Rotate(Directions.Left);

        if (Input.GetKey(KeyCode.D))
            Rotate(Directions.Right);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeView();
            ChangeCameraState();
        }

        transform.position += velocity.normalized * _speed * Time.deltaTime;

        WrapAroundMap();
    }


    private bool _changedView = true;
    private bool _planetView = true;
    private float _zoomFactor = 0f;
    private Vector3 _savedPlanetPosition;
    private Vector3 _savedOverviewPosition;

    private void UpdateAuto()
    {
        var curvature = 0f;
        var position = Vector3.zero;
        var rotation = Vector3.zero;
        var cameraPivotPosition = Vector3.zero;

        if (!_planetView) ZoomIn();
        else ZoomOut();

        MainManager.Instance.CameraController.transform.position = cameraPivotPosition;
        var eulerRotation = Quaternion.Euler(rotation);
        MainManager.Instance.Camera.SkyBoxCam.clearFlags = _planetView ? CameraClearFlags.Skybox : CameraClearFlags.Color;
        Globe.UpdateHorizonTransform(position, eulerRotation);
        Globe.Curvature = curvature;

        WrapAroundMap();

        if (_changedView) ChangeCameraState();
        if (_changedView && !_planetView) _skyBox.ToggleAtmosphere();

        void ZoomIn()
        {
            _zoomFactor = _zoomFactor < 1 ? _zoomFactor + 1 * Time.deltaTime : 1;

            curvature  = Mathf.Lerp(GlobeData.PLANET_CURVATURE, GlobeData.OVERVIEW_CURVATURE, _zoomFactor);
            position.y = Mathf.Lerp(GlobeData.DEFAULT_PLANET_POSITION.y, GlobeData.DEFAULT_OVERVIEW_POSITION.y, _zoomFactor);
            position.z = Mathf.Lerp(GlobeData.DEFAULT_PLANET_POSITION.z, GlobeData.DEFAULT_OVERVIEW_POSITION.z, _zoomFactor);
            rotation.x = Mathf.Lerp(GlobeData.DEFAULT_PLANET_ROTATION.x, GlobeData.DEFAULT_OVERVIEW_ROTATION.x, _zoomFactor);

            cameraPivotPosition.x = Mathf.Lerp(_savedPlanetPosition.x, MapData.EQUATOR.x, _zoomFactor);
            cameraPivotPosition.z = Mathf.Lerp(_savedPlanetPosition.z, MapData.EQUATOR.z, _zoomFactor);

            if (_zoomFactor >= 1) _changedView = true;
        }
        void ZoomOut()
        {
            _zoomFactor = _zoomFactor > 0 ? _zoomFactor - 1 * Time.deltaTime : 0;

            curvature  = Mathf.Lerp(GlobeData.PLANET_CURVATURE, GlobeData.OVERVIEW_CURVATURE, _zoomFactor);
            position.y = Mathf.Lerp(GlobeData.DEFAULT_PLANET_POSITION.y, GlobeData.DEFAULT_OVERVIEW_POSITION.y, _zoomFactor);
            position.z = Mathf.Lerp(GlobeData.DEFAULT_PLANET_POSITION.z, GlobeData.DEFAULT_OVERVIEW_POSITION.z, _zoomFactor);
            rotation.x = Mathf.Lerp(GlobeData.DEFAULT_PLANET_ROTATION.x, GlobeData.DEFAULT_OVERVIEW_ROTATION.x, _zoomFactor);

            cameraPivotPosition.x = Mathf.Lerp(_savedPlanetPosition.x, _savedOverviewPosition.x, _zoomFactor);
            cameraPivotPosition.z = Mathf.Lerp(_savedPlanetPosition.z, _savedOverviewPosition.z, _zoomFactor);

            if (_zoomFactor <= 0) _changedView = true;
        }
    }

    private void ChangeCameraState()
    {
        _cameraMode = _cameraMode switch
        {
            CameraMode.Auto   => CameraMode.Manual,
            CameraMode.Manual => CameraMode.Auto,
            _ => _cameraMode
        };
    }
    private void ChangeView()
    {
        _changedView = false;

        if (_planetView) _savedPlanetPosition = MainManager.Instance.CameraController.transform.position;
        else
        {
            _savedOverviewPosition = MainManager.Instance.CameraController.transform.position;
            _skyBox.ToggleAtmosphere();
        }
        _planetView = !_planetView;
        _skyBox.ToggleStars();
    }

    private void Rotate(Directions direction)
    {
        var rotation = (float) direction * _rotationSpeed * Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(rotation, transform.up);
        _skyBox.RotateSkybox(rotation);
    }

    /*
 *  lexacutable:
    regarding A), this is very very very simple
    if your grid is 64x64, and your unit is at x=63 and y=5, and moves 4 units up and 4 units right, your unit is now at x=3 (67%64) and y=9
    you just loop around
    to zero
    this looping goes for everything, including the area that buildings or other things occupy, pathfinding, visibility etc
    rendering
 */

    private void WrapAroundMap()
    {
        var camPos = _cam.transform.position;
        var pivotPos = transform.position;

        if      (camPos.x > MapData.WIDTH)  pivotPos.x = pivotPos.x     - MapData.WIDTH;
        else if (camPos.x < 0)              pivotPos.x = MapData.WIDTH  + pivotPos.x;

        if      (camPos.z > MapData.HEIGHT) pivotPos.z = pivotPos.z     - MapData.HEIGHT;
        else if (camPos.z < 0)              pivotPos.z = MapData.HEIGHT + pivotPos.z;

        Globe.UpdateHorizonCameras(pivotPos);
        transform.position = pivotPos;
    }

    void OnDrawGizmos()
    {
        return;
        if (!Application.isPlaying) return;

        var distance = 10f;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * distance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * distance);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * distance);
    }
}
