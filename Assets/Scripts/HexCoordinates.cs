using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int x, z;

    public int X
    {
        get { return x; }
    }

    public int Z
    {
        get { return z; }
    }

    public int Y
    {
        get
        {
            return -X - Z;
        }
    }

    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    /// <summary>Generates HexCoordinates based off of offset coordinates. </summary>
    /// <param name="x">The width of the grid.</param>
    /// <param name="z">The height of the grid.</param>
    /// <returns>Offset HexCoordinates.</returns>
    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    /// <summary>Calculates the position of a coordinate. X is divided by the horizontal width of a hexagon. Y is a mirror of X and therefore equal to -X.
    /// Shifting by an offset for the Z-axis of one unit to the left for every two rows. The values are then rounded to the closest int.
    /// Due to rounding errors, iX and iZ need to be reconstructed before being returned.
    /// </summary>
    /// <param name="position">The position of the raycast from TouchCell.</param>
    /// <returns></returns>
    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;

        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }

    /// <summary>Overrides the default ToString to return the struct values, not just it's type.</summary>
    /// <returns>Returns the values, not the type.</returns>
    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    /// <summary>Matches matches the current setup of having the coordinates on split lines.</summary>
    /// <returns></returns>
    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }
}