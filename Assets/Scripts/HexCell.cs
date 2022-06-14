using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;

    Color colour;

    [SerializeField]
    HexCell[] neighbours;

    /// <summary>The elevation of this gameObject's cell.</summary>
    int elevation = int.MinValue;

    public RectTransform uiRect;

    public HexGridChunk chunk;

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    public int Elevation
    {
        get
        {
            return elevation;
        }

        //Sets the cells localPosition based on the given value.
        set
        {
            if (elevation == value)
            {
                return;
            }

            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            Refresh();
        }
    }

    public Color Colour
    {
        get
        {
            return colour;
        }

        set
        {
            if (colour == value)
            {
                return;
            }

            colour = value;

            Refresh();
        }
    }

    /// <summary>Gets the neighbour in the given direction.</summary>
    /// <param name="direction">The given direction.</param>
    /// <returns>The neighbour in the given direction.</returns>
    public HexCell GetNeighbour(HexDirection direction)
    {
        return neighbours[(int)direction];
    }

    /// <summary>Sets the neighbour in the given direction to the current cell.
    /// Using the HexDirectionExtension Opposite, it determines the bi-directional neighbour of the current cell if there is one,
    /// and sets the current cell as it's equivalent neighbour.
    /// </summary>
    /// <param name="direction">The given direction.</param>
    /// <param name="cell">They neighbour in the given direction as the current cell.</param>
    public void SetNeighbour(HexDirection direction, HexCell cell)
    {
        neighbours[(int)direction] = cell;
        cell.neighbours[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbours[(int)direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();

            for (int i = 0; i < neighbours.Length; i++)
            {
                HexCell neighbour = neighbours[i];

                if (neighbour != null && neighbour.chunk != chunk)
                {
                    neighbour.chunk.Refresh();
                }
            }
        }
    }
}