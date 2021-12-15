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

    #region Legacy Code
    #region Old Direction To Point Method
    private Vector3 DirectionToPointOLD_SWITCH()
    {
        return _direction switch
        {
            Directions.Diagonal => Diagonal(),
            Directions.NorthSouth => NorthSouth(),
            Directions.EastWest => EastWest(),
            _ => Vector3.zero
        };

        Vector3 Diagonal()
        {
            var directionToPoint = new Vector3(PointingEast ? -1 : 1, 0, 1);
            if (PointingEast) directionToPoint.x = -1;
            if (PointingNorth) directionToPoint.z = -1;
            return directionToPoint;
        }

        Vector3 NorthSouth()
        {
            var directionToPoint = new Vector3(0, 0, 1);
            if (PointingNorth) directionToPoint.z = -1;
            return directionToPoint;
        }
        Vector3 EastWest()
        {
            var directionToPoint = new Vector3(1, 0, 0);
            if (PointingEast) directionToPoint.x = -1;
            return directionToPoint;
        }
    }

    #endregion
    #region How not to write Code - DO NOT OPEN
    private Vector3 DirectionToPointOLD()
    {
        bool East = _cameraPivot.transform.position.x > MapData.WIDTH * 0.5f;
        bool North = _cameraPivot.transform.position.z > MapData.HEIGHT * 0.5f;


        var LookingUp = Vector3.Dot(_cameraPivot.transform.forward, Vector3.forward) >= 0;
        var LookingRight = Vector3.Dot(_cameraPivot.transform.forward, Vector3.right) >= 0;
        var directionToPoint = Vector3.zero;

        if (_direction == Directions.Diagonal)
        {
            if (East && North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, -1);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, -1);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, -1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, -1);
                }
            }
            if (!East && !North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 1);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 1);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 1);
                }
            }
            if (!East && North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, -1);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, -1);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, -1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, -1);
                }
            }
            if (East && !North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, 1);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, 1);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, 1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, 1);
                }
            }
        }
        if (_direction == Directions.NorthSouth)
        {
            if (East && North)
            {
                if (LookingUp && LookingRight)
                {
                    return new Vector3(0, 0, -1);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, 0);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, 0);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(-1, 0, 0);
                }
            }
            if (!East && !North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 0);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 0);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 0);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 0);
                }
            }
            if (!East && North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 0);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(1, 0, 0);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, -1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, -1);
                }

            }
            if (East && !North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
            }
        }
        if (_direction == Directions.EastWest)
        {
            if (East && North)
            {
                if (LookingUp && LookingRight)
                {
                    return new Vector3(-1, 0, 0);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, -1);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, -1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, -1);
                }
            }
            if (!East && !North)
            {
                if (LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
                if (LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
                if (!LookingUp && !LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
                if (!LookingUp && LookingRight)
                {
                    directionToPoint = new Vector3(0, 0, 1);
                }
            }
            if (!East && North)
            {
                if (LookingUp && LookingRight)
                {
                    return new Vector3(0, 0, -1);
                }
                if (LookingUp && !LookingRight)
                {
                    return new Vector3(0, 0, -1);
                }
                if (!LookingUp && LookingRight)
                {
                    return new Vector3(1, 0, 0);
                }
                if (!LookingUp && !LookingRight)
                {
                    return new Vector3(1, 0, 0);
                }
            }
            if (East && !North)
            {
                if (LookingUp && LookingRight)
                {
                    return new Vector3(-1, 0, 0);
                }
                if (LookingUp && !LookingRight)
                {
                    return new Vector3(-1, 0, 0);
                }
                if (!LookingUp && LookingRight)
                {
                    return new Vector3(-1, 0, 0);
                }
                if (!LookingUp && !LookingRight)
                {
                    return new Vector3(-1, 0, 0);
                }
            }
        }
        return directionToPoint;
    }
    #endregion
    #endregion
}
