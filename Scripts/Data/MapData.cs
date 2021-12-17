using UnityEngine;

public static class MapData
{
    private const int DEFAULT = 80;
    public static int HORIZON_RENDER_DISTANCE => DEFAULT;
    public static int WIDTH => DEFAULT;
    public static int HEIGHT => DEFAULT;

    public const float MIN_TERRAIN_HEIGHT = 1f;
    public const float MAX_TERRAIN_HEIGHT = 30f;
}

public static class GlobeData
{
    //-- 5 planet, 35 in curvature
    public const float PLANET_CURVATURE = 5;
    public const float OVERVIEW_CURVATURE = 35;

    public static readonly Vector3 DEFAULT_PLANET_POSITION = new Vector3(0, 8, -8);
    public static readonly Vector3 DEFAULT_PLANET_ROTATION = new Vector3(35, 0, 0);

    public static readonly Vector3 DEFAULT_OVERVIEW_POSITION = new Vector3(0, 35, 0);
    public static readonly Vector3 DEFAULT_OVERVIEW_ROTATION = new Vector3(90, 0, 0);
}