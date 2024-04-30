namespace Fiero.Business
{
    public class EmptyRoom : Room
    {
        public EmptyRoom()
        {
            AllowMonsters = true;
            AllowFeatures = true;
            AllowItems = true;
        }
    }
}
