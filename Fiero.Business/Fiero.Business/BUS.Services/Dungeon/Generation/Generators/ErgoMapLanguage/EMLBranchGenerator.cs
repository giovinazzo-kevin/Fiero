using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

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
                .WithStep(ProcessMarkers)
                .Build(id, map.Size);
        }

        protected virtual void ProcessMarkers(FloorGenerationContext ctx)
        {
            // Use then remove temporary features used as markers by EML
            foreach (var def in ctx.GetObjects()
                .Where(obj => obj.Name == EMLInterpreter.CTX_MapMarker_Name))
            {
                var functor = ((Dict)def.Data).Functor;
                if (!functor.TryGetA(out var fun))
                    continue;
                var markerType = Enum.Parse<MapMarkerName>(fun.Explain().ToCSharpCase());
                switch (markerType)
                {
                    case MapMarkerName.SpawnPoint:
                        ctx.AddSpawnPoint(def.Position);
                        break;
                }
            }
            ctx.RemoveObjects(o => o.Name == EMLInterpreter.CTX_MapMarker_Name);
        }
    }
}
