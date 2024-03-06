namespace Fiero.Business
{
    public readonly record struct TileVariant(TileNeighborhood Matrix, string Variant, int Precedence);
}
