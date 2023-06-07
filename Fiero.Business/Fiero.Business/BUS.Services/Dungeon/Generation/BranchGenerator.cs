namespace Fiero.Business
{
    public interface IBranchGenerator
    {
        Floor GenerateFloor(FloorId id, FloorBuilder builder);
    }
}
