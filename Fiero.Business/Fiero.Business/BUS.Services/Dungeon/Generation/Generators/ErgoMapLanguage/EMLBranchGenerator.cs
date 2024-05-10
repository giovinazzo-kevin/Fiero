namespace Fiero.Business
{
    [SingletonDependency]
    public class EMLBranchGenerator(GameScripts<ScriptName> scripts) : IBranchGenerator
    {
        public readonly GameScripts<ScriptName> Scripts = scripts;
        public DungeonTheme Theme { get; set; } = DungeonTheme.Default;
        public Floor GenerateFloor(FloorId id, FloorBuilder builder)
        {
            var script = (ErgoScript)Scripts.Get(ScriptName.Map_Test);
            var fieroLib = script.VM.KB.Scope.GetLibrary<FieroLib>(FieroLib.Modules.Fiero);
            var map = fieroLib.Maps[script.VM.KB.Scope.Entry];
            return builder
                .WithStep(ctx => ctx.FillBox(Coord.Zero, map.Size, Theme.RoomTile))
                .WithStep(ctx =>
                {
                    ctx.Theme = Theme;
                    var eml = EMLInterpreter.GenerateMap(script.VM, new(id, Coord.Zero, map.Size - Coord.PositiveOne));
                    if (!eml(script.VM, ctx))
                        return;
                })
                .WithStep(ctx =>
                {
                    // Remove temporary features used as markers by EML
                    ctx.RemoveObjects(o => o.Name == nameof(FeatureName.PrefabAnchor));
                })
                .Build(id, map.Size);
        }
    }
}
