using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public static class Globe
{
    public static bool Ready => MainManager.Instance.Ready;

    public static float Curvature {
        set => MainManager.Instance.GameController.SetCurvature(value);
    }

    public static int RunOrder => MainManager.Instance.RunOrder;
    public static void UpdateHorizonCameras(Vector3 position)
    {
        if (!Ready) return;
        MainManager.Instance.UpdateHorizon(position);
    }

    public static void UpdateHorizonTransform(Vector3 position, Quaternion rotation)
    {
        MainManager.Instance.Camera.UpdateHorizonTransform(position, rotation);
    }

    public static Vector3 WrapAround(Vector3 destination)
    {
        if (destination.x >= MapData.WIDTH) destination.x = destination.x - MapData.WIDTH;
        else if (destination.x < 0) destination.x = MapData.WIDTH + destination.x;

        if (destination.z >= MapData.HEIGHT) destination.z = destination.z - MapData.HEIGHT;
        else if (destination.z < 0) destination.z = MapData.HEIGHT + destination.z;

        return destination;
    }

    /*
        public static float EvaluateDistance(Vector3 position, Vector3 destination)
        {
            var dx = Mathf.Abs(destination.x - position.x);
            var dz = Mathf.Abs(destination.z - position.z);

            if (dx > 0.5f)
                dx = 1.0f - dx;
            if (dz > 0.5f)
                dz = 1.0f - dz;

            return Mathf.Sqrt(dx * dx + dz * dz);
            Debug.Log($"Distance To Destination is {Mathf.Sqrt(dx * dx + dz * dz)}");
        }

        float ToroidalDistance(float x1, float y1, float x2, float y2)
        {
            float dx = std::abs(x2 - x1);
            float dy = std::abs(y2 - y1);

            if (dx > 0.5f)
                dx = 1.0f - dx;

            if (dy > 0.5f)
                dy = 1.0f - dy;

            return std::sqrt(dx * dx + dy * dy);
        }
    */
    public static float Distance(Vector3 position, Vector3 destination)
    {
        var wrappedPosition = position;

        if (Mathf.Abs(destination.x - position.x) > MapData.WIDTH  * 0.5f) WrapX();
        if (Mathf.Abs(destination.z - position.z) > MapData.HEIGHT * 0.5f) WrapZ();

        void WrapX()
        {
            wrappedPosition.x = position.x > MapData.WIDTH * 0.5f
                ? position.x - MapData.WIDTH
                : MapData.WIDTH - position.x;
        }
        void WrapZ()
        {
            wrappedPosition.z = position.z > MapData.HEIGHT * 0.5f
                ? position.z - MapData.HEIGHT
                : MapData.HEIGHT - position.z;
        }

        var distance1 = Vector3.Distance(position, destination);
        var distance2 = Vector3.Distance(wrappedPosition, destination);

        return distance1 < distance2 ? distance1 : distance2;
    }
}