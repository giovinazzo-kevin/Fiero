using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Business
{
    [SingletonDependency]
    public class EMLBranchGenerator(GameScripts<ScriptName> scripts) : IBranchGenerator
    {
        [Term(Marshalling = TermMarshalling.Named, Functor = "enemy")]
        public readonly record struct GetRandomEnemyArgs(int Dummy);
        [Term(Marshalling = TermMarshalling.Named, Functor = "item")]
        public readonly record struct GetRandomItemArgs(int Dummy);

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

        protected virtual IEntityBuilder<Actor> GetRandomEnemy(GetRandomEnemyArgs args, GameEntityBuilders e)
        {
            return e.NPC_Rat();
        }

        protected virtual IEntityBuilder<Item> GetRandomItem(GetRandomItemArgs args, GameEntityBuilders e)
        {
            return e.Resource_Gold(500);
        }

        protected virtual void ProcessMarkers(FloorGenerationContext ctx)
        {
            // Use then remove temporary features used as markers by EML
            var markers = ctx.GetObjects()
                .Where(obj => obj.Name == EMLInterpreter.CTX_MapMarker_Name)
                .ToList();
            foreach (var def in markers)
            {
                var dict = (Dict)def.Data;
                if (!dict.Functor.TryGetA(out var functor))
                    continue;
                var markerType = Enum.Parse<MapMarkerName>(functor.Explain().ToCSharpCase());
                switch (markerType)
                {
                    case MapMarkerName.SpawnPoint:
                        ctx.AddSpawnPoint(def.Position);
                        break;
                    case MapMarkerName.Enemy when dict.Matches<GetRandomEnemyArgs>(out var args):
                        ctx.AddObject(nameof(MapMarkerName.Enemy), def.Position, e => GetRandomEnemy(args, e));
                        break;
                    case MapMarkerName.Item when dict.Matches<GetRandomItemArgs>(out var args):
                        ctx.AddObject(nameof(MapMarkerName.Item), def.Position, e => GetRandomItem(args, e));
                        break;
                }
            }
            ctx.RemoveObjects(markers.Contains);
        }
    }
}
