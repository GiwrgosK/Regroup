using System;

public readonly struct GridPosition : IEquatable<GridPosition> {
	public readonly int x;
	public readonly int z;
	
	public GridPosition(int x, int z) {
		this.x = x;
		this.z = z;
	}

	public static bool operator ==(GridPosition a, GridPosition b) => a.x == b.x && a.z == b.z;
	public static bool operator !=(GridPosition a, GridPosition b) => !(a == b);

	public static GridPosition operator +(GridPosition a, GridPosition b) => new GridPosition(a.x + b.x, a.z + b.z);
	public static GridPosition operator -(GridPosition a, GridPosition b) => new GridPosition(a.x - b.x, a.z - b.z);
	
	public override bool Equals(object obj) {
		return obj is GridPosition position && x == position.x && z == position.z;
	}

	public bool Equals(GridPosition other) {
		return this == other;
	}
	
	public override int GetHashCode() {
		return HashCode.Combine(x, z);
	}

	public override string ToString() {
		return "x: " + x + "; z: " + z;
	}
}