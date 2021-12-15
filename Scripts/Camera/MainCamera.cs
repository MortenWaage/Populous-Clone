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
        Debug.Log($"{Globe.RunOrder} - Camera is set");
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

        #region Legacy Code
        #region How not to write code - PROOF OF CONCEPT:

        var LookingUp = Vector3.Dot(_cameraPivot.transform.forward, Vector3.forward) >= 0;
        var LookingRight = Vector3.Dot(_cameraPivot.transform.forward, Vector3.right) >= 0;

        /*Inverse values for camera position for South West and North East. 
        For instance, !lookingUp & !lookingRight is 3221 instead of 1223.
        The same applies to South East and North West. Camera is angled precisely in the mirror direcion.*/

        if (!East && !North) //--SouthWest
        {
            if (!LookingUp)
            {
                if (!LookingRight)  //-- Check logic for this one
                {
                    _camDiagonal.depth = 1;
                    _camNorthSouth.depth = 4;
                    _camEastWest.depth = 3;
                    _camMain.depth = 4;
                }
                else
                {
                    _camDiagonal.depth = 2;
                    _camNorthSouth.depth = 4;
                    _camEastWest.depth = 1;
                    _camMain.depth = 3;
                }
            }
            if (LookingUp)
            {
                if (!LookingRight)
                {
                    _camDiagonal.depth = 2;
                    _camNorthSouth.depth = 1;
                    _camEastWest.depth = 3;
                    _camMain.depth = 4;
                }
                else  //-- Check logic for this one
                {
                    _camDiagonal.depth = 4;
                    _camNorthSouth.depth = 2;
                    _camEastWest.depth = 3;
                    _camMain.depth = 1;
                }
            }
        }
        if (East && North) //--NorthEast
        {
            if (!LookingUp)
            {
                if (!LookingRight)
                {
                    _camDiagonal.depth = 4;
                    _camNorthSouth.depth = 2;
                    _camEastWest.depth = 3;
                    _camMain.depth = 1;
                }
                else
                {
                    _camDiagonal.depth = 2;
                    _camNorthSouth.depth = 1;
                    _camEastWest.depth = 4;
                    _camMain.depth = 2;
                }
            }
            if (LookingUp)
            {
                if (!LookingRight) //-- Check logic for this one
                {
                    _camDiagonal.depth = 1;
                    _camNorthSouth.depth = 3;
                    _camEastWest.depth = 2;
                    _camMain.depth = 2;
                }
                else //-- Check logic for this one
                {
                    _camDiagonal.depth = 1;
                    _camNorthSouth.depth = 2;
                    _camEastWest.depth = 2;
                    _camMain.depth = 3;
                }
            }
        }
        if (!East && North) //-- NorthWest
        {
            if (!LookingUp)
            {
                if (!LookingRight)
                {
                    _camDiagonal.depth = 2;
                    _camNorthSouth.depth = 4;
                    _camEastWest.depth = 2;
                    _camMain.depth = 3;
                }
                else //-- Check Logic for this one
                {
                    _camDiagonal.depth = 3;
                    _camNorthSouth.depth = 2;
                    _camEastWest.depth = 2;
                    _camMain.depth = 1;
                }
            }
            if (LookingUp)
            {
                if (!LookingRight)  //-- Check logic for this one
                {
                    _camDiagonal.depth = 1;
                    _camNorthSouth.depth = 2;
                    _camEastWest.depth = 2;
                    _camMain.depth = 3;
                }
                else
                {
                    _camDiagonal.depth = 3;
                    _camNorthSouth.depth = 4;
                    _camEastWest.depth = 1;
                    _camMain.depth = 2;
                }
            }
        }
        if (East && !North) //-- SouthEast
        {
            if (LookingUp)
            {
                if (!LookingRight) //-- Check logic for this one
                {
                    _camDiagonal.depth = 3;
                    _camNorthSouth.depth = 2;
                    _camEastWest.depth = 2;
                    _camMain.depth = 1;
                }
                else
                {
                    _camDiagonal.depth = 1;
                    _camNorthSouth.depth = 4;
                    _camEastWest.depth = 2;
                    _camMain.depth = 3;
                }
            }
            if (!LookingUp)
            {
                if (!LookingRight)
                {
                    _camDiagonal.depth = 1;
                    _camNorthSouth.depth = 2;
                    _camEastWest.depth = 3;
                    _camMain.depth = 4;
                }
                else
                {
                    _camDiagonal.depth = 1;
                    _camNorthSouth.depth = 3;
                    _camEastWest.depth = 2;
                    _camMain.depth = 4;
                }
            }
        }

        #endregion
        #endregion
    }
}
