namespace Fiero.Business
{
    //[SingletonDependency(typeof(IUIControlResolver<Minimap>))]
    //public class MinimapResolver : UIControlResolver<Minimap>
    //{
    //    protected readonly MetaSystem Systems;

    //    public MinimapResolver(GameUI ui, GameResources resources, MetaSystem systems)
    //        : base(ui, resources)
    //    {
    //        Systems = systems;
    //    }

    //    public override Minimap Resolve(LayoutGrid dom)
    //    {
    //        var map = new Minimap(UI.Input, Systems.Get<DungeonSystem>(), Systems.Get<FactionSystem>(), Resources.Colors);
    //        map.Background.V = Background;
    //        map.Foreground.V = Foreground;
    //        return map;
    //    }
    //}
}
