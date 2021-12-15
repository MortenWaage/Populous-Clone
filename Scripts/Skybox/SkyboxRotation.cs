using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyboxRotation : MonoBehaviour
{
    [SerializeField] float _rotationSpeed = 1f;
    [SerializeField] private Vector3 _screen_right;
    [SerializeField] private Vector3 _screen_left;
    [SerializeField] [Range(0, 20f)] private float _hardCodedRotationSpeed = 5;

    [SerializeField] RawImage _stars;
    [SerializeField] RawImage _atmosphere;

    void Start()
    {
        _stars.enabled = false;
        _atmosphere.enabled = false;
    }
    void Update()
    { var rotation = 1 * _rotationSpeed * Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(rotation, transform.up);
    }

    public void RotateSkybox(float rotation)
    {
        transform.rotation *= Quaternion.AngleAxis(rotation, transform.up);
    }

    public void ToggleStars()
    {
        _stars.enabled = !_stars.enabled;
    }
    public void ToggleAtmosphere()
    {
        _atmosphere.enabled = !_atmosphere.enabled;
    }
}