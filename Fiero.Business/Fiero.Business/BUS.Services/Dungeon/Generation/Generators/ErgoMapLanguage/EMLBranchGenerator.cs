using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;

namespace Fiero.Business
{
    [SingletonDependency]
    public class EMLBranchGenerator(GameScripts<ScriptName> scripts) : IBranchGenerator
    {
        [Term(Marshalling = TermMarshalling.Named, Functor = "npc")]
        public readonly record struct GetNpcName(NpcName Type);
        [Term(Marshalling = TermMarshalling.Named, Functor = "item")]
        public readonly record struct GetItemArgs(string Type);
        [Term(Marshalling = TermMarshalling.Named, Functor = "feature")]
        public readonly record struct GetFeatureArgs(FeatureName Type);

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

        protected virtual IEntityBuilder<Actor> GetRandomEnemy(GetNpcName args, GameEntityBuilders e)
        {
            return e.NPC_Rat();
        }

        protected virtual IEntityBuilder<Item> GetRandomItem(GetItemArgs args, GameEntityBuilders e)
        {
            return e.Resource_Gold(500);
        }

        protected virtual IEntityBuilder<Feature> GetRandomFeature(GetFeatureArgs args, GameEntityBuilders e)
        {
            return args.Type switch
            {
                FeatureName.Door => e.Feature_Door(),
                FeatureName.Statue => e.Feature_Statue(),
                FeatureName.Trap => e.Feature_Trap(),
                FeatureName.Shrine => e.Feature_Shrine(),
                FeatureName.Chest => e.Feature_Chest(),
                _ => throw new NotSupportedException()
            };
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
                    case MapMarkerName.Npc when dict.Matches<GetNpcName>(out var args):
                        ctx.AddObject(nameof(MapMarkerName.Npc), def.Position, e => GetRandomEnemy(args, e));
                        break;
                    case MapMarkerName.Feature when dict.Matches<GetFeatureArgs>(out var args):
                        ctx.TryAddFeature(nameof(MapMarkerName.Feature), def.Position, e => GetRandomFeature(args, e));
                        break;
                    case MapMarkerName.Item when dict.Matches<GetItemArgs>(out var args):
                        ctx.AddObject(nameof(MapMarkerName.Item), def.Position, e => GetRandomItem(args, e));
                        break;
                }
            }
            ctx.RemoveObjects(markers.Contains);
        }
    }
}
