using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HexGrid : MonoBehaviour
{
	/// <summary>The number of grid chunks on the X-axis.</summary>
	[Tooltip("The chunk count for the X-axis.")]
	public int chunkCountX = 4;

	/// <summary>The number of grid chunks on the Z-axis.</summary>
	[Tooltip("The chunk count for the Z-axis.")]
	public int chunkCountZ = 3;

	int cellCountX, cellCountZ;

	/// <summary>The Hex Cell prefab.</summary>
	[Tooltip("The Hex Cell prefab goes here.")]
	public HexCell cellPrefab;

	/// <summary>Array of cells in the grid based on the assigned width and height.</summary>
	HexCell[] cells;

	/// <summary>The Hex Cell Label prefab.</summary>
	[Tooltip("The Hex Cell prefab goes here.")]
	public TMP_Text cellLabelPrefab;

	/// <summary>The colour for an untouched cell.</summary>
	[Tooltip("Untouched cell colour.")]
	public Color defaultColour = Color.white;

	/// <summary>The imported noise texture.</summary>
	public Texture2D noiseSource;

	/// <summary>The hex grid chunk prefab.</summary>
	public HexGridChunk chunkPrefab;

	/// <summary>Array of hex grid chunks to create.</summary>
	HexGridChunk[] chunks;

	void Awake()
	{
		HexMetrics.noiseSource = noiseSource;
		
		cellCountX = chunkCountX * HexMetrics.chunkSizeX;
		cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

		CreateChunks();
		CreateCells();
	}

	void CreateChunks()
	{
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++)
		{
			for (int x = 0; x < chunkCountX; x++)
			{
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	void CreateCells()
    {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

	void OnEnable()
	{
		HexMetrics.noiseSource = noiseSource;
	}

	/// <summary>Takes in the given position and determines what cell was hit.</summary>
	/// <param name="position">The position of the ray hitpoint with the mesh.</param>
	/// <returns>The cells array index at the given position.</returns>
	public HexCell GetCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		return cells[index];
	}

	/// <summary>Instantiates a Hex Cell prefab at each point, offset by each cells size.
	/// These are stored in an local variable array to keep track of them.
	/// These are parented to this grid object, without changing their world position.
	/// Instantiates a Hex Cell Label prefab with each cell's position, and parents them to the Hex Grid Canvas
	/// without changing their world position.
	/// </summary>
	/// <param name="x">Represents the assigned width of the grid.</param>
	/// <param name="z">Represents the assigned height of the grid.</param>
	/// <param name="i">Increments the index of the "cells" array so the local variable array "cell" matches it's size.
	/// </param>
	void CreateCell(int x, int z, int i)
	{
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.Colour = defaultColour;

		//Sets the East-West neighbour relations between cells.
		if (x > 0)
		{
            cell.SetNeighbour(HexDirection.W, cells[i - 1]);
		}

		//Sets the NorthWest/NorthEast-SouthWest/SouthEast neighbour relations between cells.
		if (z > 0)
		{
			if ((z & 1) == 0)
			{
				cell.SetNeighbour(HexDirection.SE, cells[i - cellCountX]);
				
				if (x > 0)
				{
					cell.SetNeighbour(HexDirection.SW, cells[i - cellCountX - 1]);
				}				
			}

			else
			{
				cell.SetNeighbour(HexDirection.SW, cells[i - cellCountX]);

				if (x < cellCountX - 1)
				{
					cell.SetNeighbour(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}


		}

		TMP_Text label = Instantiate<TMP_Text>(cellLabelPrefab);
		
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
	}

	void AddCellToChunk(int x, int z, HexCell cell)
	{
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}
}