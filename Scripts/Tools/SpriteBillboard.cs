using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] Camera _cam;

    void Update()
    {
        Quaternion billBoardTransform = _cam.transform.rotation;
        transform.rotation = billBoardTransform;
    }
}