using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	//This editor handles the modification of the Hex Grid. 

	public Color[] colors;

	public HexGrid hexGrid;

	private Color activeColor;

	int activeElevation;

	void Awake()
	{
		SelectColor(0);
	}

	void Update()
	{
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			HandleInput();
		}
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(inputRay, out hit))
		{
			EditCell(hexGrid.GetCell(hit.point));
		}
	}

	/// <summary>Takes in the cell at the ray's hitpoint. Sets the colour of that cell. Changes the elevation of the cell.
	/// Triangulates the mesh again.</summary>
	/// <param name="cell"></param>
	void EditCell(HexCell cell)
	{
		cell.Colour = activeColor;
		cell.Elevation = activeElevation;		
	}

	public void SelectColor(int index)
	{
		activeColor = colors[index];
	}

	public void SetElevation(float elevation)
	{
		activeElevation = (int)elevation;
	}
}