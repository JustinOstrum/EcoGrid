public enum HexDirection
{
	NE, E, SE, SW, W, NW
}

public static class HexDirectionExtensions
{
	/// <summary>Determines the opposite of the given direction by adding or subtracting 3.</summary>
	/// <param name="direction">The given direction.</param>
	/// <returns>The given direction plus or minus 3.</returns>
	public static HexDirection Opposite(this HexDirection direction)
	{
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}

	/// <summary>Determines the previous direction by subtracting 1 from the current direction.</summary>
	/// <param name="direction">The given direction.</param>
	/// <returns>The given direction minus 1.</returns>
	public static HexDirection Previous(this HexDirection direction)
	{
		return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
	}

	/// <summary>Determines the next direction by adding 1 to the current direction.</summary>
	/// <param name="direction">The given direction.</param>
	/// <returns>The given direction plus 1.</returns>
	public static HexDirection Next(this HexDirection direction)
	{
		return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
	}
}