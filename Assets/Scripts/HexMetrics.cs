using UnityEngine;

public static class HexMetrics
{
    /// <summary>The distance to the peak of one of the hexagons points.</summary>
    public const float outerRadius = 10f;

    /// <summary>Divide the hexagon into 6 triangles. Divide one into two right triangles.
    /// Use the Pythagorean theorem to then determine the height of the triangle, the innerRadius. </summary>
    public const float innerRadius = outerRadius * 0.866025404f;

    /// <summary>The solid factor of the blend region, currently set to 75%.</summary>
    public const float solidFactor = 0.8f;

    /// <summary>Blend factor of the outerRadius is set by subtracting the solid factor from the whole and usingthe remainder.</summary>
    public const float blendFactor = 1f - solidFactor;

    /// <summary>The distance of each elevation step.</summary>
    public const float elevationStep = 3f;

    /// <summary>The number of terraces per slope.</summary>
    public const int terracesPerSlope = 2;

    /// <summary>The number of terraceSteps per slope.</summary>
    public const int terraceSteps = terracesPerSlope * 2 + 1;

    /// <summary>Determines the horizontal terrace step size based on the number of steps.</summary>
    public const float horizontalTerraceStepSize = 1f / terraceSteps;

    /// <summary>Determines the vertical terrace step size based on the number of terraces per slope. </summary>
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    /// <summary>Reference to the imported noise texture.</summary>
    public static Texture2D noiseSource;

    /// <summary>Stength value for cell perturbing.</summary>
    public const float cellPerturbStrength = 4f;

    /// <summary>Scale of the noise texture beign sampled.</summary>
    public const float noiseScale = 0.003f;

    /// <summary>Constant value of the strength value that vertices should be perturbed by.</summary>
    public const float elevationPerturbStrength = 1.5f;

    /// <summary>Constant value for the width and height of the chunk sizes.</summary>
    public const int chunkSizeX = 5, chunkSizeZ = 5;

    /// <summary>Static vector array for the corners on the XZ plane, oriented with the point up.</summary>
    static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    /// <summary>Getter function for the corners array. Gets the first corner.</summary>
    /// <param name="direction"></param>
    /// <returns>The first corner in a given direction.</returns>
    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    /// <summary>Getter function for the corners array. Gets the second corner.</summary>
    /// <param name="direction"></param>
    /// <returns>The second corner in a given direction.</returns>
    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    /// <summary>Determines the first solid corner of a given direction.</summary>
    /// <param name="direction">The given direction.</param>
    /// <returns>The solid factor of the first corner of a given direction.</returns>
    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    /// <summary>Determines the second solid corner of a given direction</summary>
    /// <param name="direction">The given direction.</param>
    /// <returns>The solid factor of the second corner of a given direction.</returns>
    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }

    /// <summary>Determines the rectangular bridge between cell.</summary>
    /// <param name="direction">The given direction.</param>
    /// <returns>Adjusted bridge between cells.</returns>
    public static Vector3 GetBridge(HexDirection direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
    }

    /// <summary>Interpolates between the values of a and b and adjusts the values to only change on instances where the Y-value is an odd number.</summary>
    /// <param name="a">Value a.</param>
    /// <param name="b">Value b.</param>
    /// <param name="step">The current step.</param>
    /// <returns>Interpolated value.</returns>
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;
        float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
        a.y += (b.y - a.y) * v;

        return a;
    }

    /// <summary>Interpolates for the colour values as if the connection was flat.</summary>
    /// <param name="a">Value a.</param>
    /// <param name="b">Value b.</param>
    /// <param name="step">The current step.</param>
    /// <returns>Interpolated colour.</returns>
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    /// <summary></summary>
    /// <param name="elevation1"></param>
    /// <param name="elevation2"></param>
    /// <returns></returns>
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }

        int delta = elevation2 - elevation1;

        if (delta == 1 || delta == -1)
        {
            return HexEdgeType.Slope;
        }

        return HexEdgeType.Cliff;
    }

    /// <summary></summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
    }
}