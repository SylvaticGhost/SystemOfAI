namespace Lab2.Entities;

public record Vector(int LeftMissionaries, int LeftCannibals, int BoatSide)
{
    public static Vector operator +(Vector a, Vector b) =>
        new(a.LeftMissionaries + b.LeftMissionaries, a.LeftCannibals + b.LeftCannibals, a.BoatSide + b.BoatSide);
}