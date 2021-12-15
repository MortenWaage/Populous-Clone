using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    public MainCamera Camera { get; set; }
    public HorizonRenderer[] HorizonCameras { get; set; }
    public GameController GameController { get; set; }
    public CameraMovementController CameraController { get; set; }
    public EventsLog EventsLog { get; set; }
    public UserInterface UI { get; set; }
    public UnitSelection UnitManager { get; set; }
    public PathFindingGrid PathFinding { get; set; }
    public bool Ready { get; private set; }

    private int _runOrder;

    public int RunOrder {
        get {
            _runOrder++; return _runOrder;
        }
    }

    void Awake()
    {
        Debug.Log($"{RunOrder} - MainManager.Instance is set");
        if (Instance == null) Instance = this;
        HorizonCameras = new HorizonRenderer[HORIZON_CAMERAS];
        Debug.Log($"{Globe.RunOrder} - Instantiated HorizonRenderer[{HORIZON_CAMERAS}]");
    }

    void Start()
    {
        StartCoroutine("AwaitEndOfFrame");
    }

    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Ready = true;
        Debug.Log($"{Globe.RunOrder} - GAME IS READY");
    }

    private const int HORIZON_CAMERAS = 3;
    private int _horizonCamerasIndex = 0;
    public void AddHorizonCam(HorizonRenderer hRend)
    {
        Debug.Log($"{RunOrder} - Horizon Camera ({_horizonCamerasIndex}) is set");
        if (_horizonCamerasIndex >= HORIZON_CAMERAS) return;
        HorizonCameras[_horizonCamerasIndex] = hRend;
        _horizonCamerasIndex++;
    }

    public void UpdateHorizon(Vector3 position)
    {
        if (!GameController.UseCameras) return;
        for (var i = 0; i < HORIZON_CAMERAS; i++)
            HorizonCameras[i].SetPosition(position);
    }
}
