namespace Fiero.Business
{
    //[SingletonDependency(typeof(IUIControlResolver<Minimap>))]
    //public class MinimapResolver : UIControlResolver<Minimap>
    //{
    //    protected readonly GameSystems Systems;

    //    public MinimapResolver(GameUI ui, GameResources resources, GameSystems systems)
    //        : base(ui, resources)
    //    {
    //        Systems = systems;
    //    }

    //    public override Minimap Resolve(LayoutGrid dom)
    //    {
    //        var map = new Minimap(UI.Input, Systems.Dungeon, Systems.Faction, Resources.Colors);
    //        map.Background.V = Background;
    //        map.Foreground.V = Foreground;
    //        return map;
    //    }
    //}
}
