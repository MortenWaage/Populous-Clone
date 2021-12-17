using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private Transform _flyByTarget;

    [SerializeField] Camera _camMain;
    [SerializeField] Camera _camDiagonal;
    [SerializeField] Camera _camNorthSouth;
    [SerializeField] Camera _camEastWest;
    [SerializeField] public Camera SkyBoxCam;

    [SerializeField] private Camera[] _gameCameras;
    [SerializeField] private CameraMovementController _cameraPivot;

    [Header("Unused")]
    [SerializeField] private float _camAngle = 45f;
    public bool _north;
    public bool _east;
    //-- End of Unused

    private bool East => _east = _camMain.transform.position.x < 0f;
    private bool North => _north = _camMain.transform.position.z < 0f;

    void Awake()
    {
        StartCoroutine("AwaitEndOfFrame");
        _gameCameras = new Camera[]
        {
            _camDiagonal,
            _camNorthSouth,
            _camEastWest,
            _camMain,
        };
    }

    private const int HORIZON_CAMERAS = 4;
    public void UpdateHorizonTransform(Vector3 position, Quaternion rotation)
    {
        for (var i = 0; i < HORIZON_CAMERAS; i++)
        {
            _gameCameras[i].transform.localPosition = position;
            _gameCameras[i].transform.localRotation = rotation;
        }
    }
    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.Camera = this;
        Debug.Log($"{Globe.RunOrder} - Main Camera is set");
    }
    void Start()
    {
        _camDiagonal.depth = 1;
        _camNorthSouth.depth = 2;
        _camEastWest.depth = 2;
        _camMain.depth = 3;
    }
    void Update()
    {
        if (_flyByTarget == null) return;
        
        transform.position = _flyByTarget.transform.position;
        transform.rotation = _flyByTarget.transform.rotation;

        return;
    }
}
