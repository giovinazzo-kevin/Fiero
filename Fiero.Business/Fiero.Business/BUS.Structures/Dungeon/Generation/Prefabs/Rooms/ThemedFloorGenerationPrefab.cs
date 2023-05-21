namespace Fiero.Business
{
    public abstract class ThemedFloorGenerationPrefab : IFloorGenerationPrefab
    {

        private DungeonTheme _theme;
        public DungeonTheme Theme
        {
            get => _theme; set
            {
                _theme = CustomizeTheme(value);
            }
        }

        public ThemedFloorGenerationPrefab()
        {
            Theme = DungeonTheme.Default;
        }

        public abstract void Draw(FloorGenerationContext ctx);

        protected virtual DungeonTheme CustomizeTheme(DungeonTheme theme)
        {
            return theme;
        }
    }
}
