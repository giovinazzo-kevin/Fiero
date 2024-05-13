using Ergo.Lang;
using Ergo.Lang.Ast;
using Ergo.Lang.Extensions;
using static Fiero.Business.FieroLib;

namespace Fiero.Business
{
    [SingletonDependency]
    public class EMLBranchGenerator(GameScripts<ScriptName> scripts) : IBranchGenerator
    {
        [Term(Marshalling = TermMarshalling.Named, Functor = "entity")]
        public readonly record struct GetEntityArgs(string Type, bool RandomType, ITerm Args);

        public readonly GameScripts<ScriptName> Scripts = scripts;
        public DungeonTheme Theme { get; set; } = DungeonTheme.Default;
        public Floor GenerateFloor(FloorId id, FloorBuilder builder)
        {
            var script = (ErgoScript)Scripts.Get(ScriptName.Map_Test);
            var fieroLib = script.VM.KB.Scope.GetLibrary<FieroLib>(FieroLib.Modules.Fiero);
            var map = fieroLib.Maps[script.VM.KB.Scope.Entry];
            return builder
                .WithStep(ctx => ctx.FillBox(Coord.Zero, map.Info.Size, Theme.RoomTile))
                .WithStep(ctx =>
                {
                    ctx.Theme = Theme;
                    var eml = EMLInterpreter.GenerateMap(map.Info.Size, id);
                    if (!eml(script.VM, ctx))
                        return;
                })
                .WithStep(ctx => ProcessMarkers(map, ctx))
                .Build(id, map.Info.Size);
        }

        protected virtual string GetRandomEntityKey(MapDef map, GetEntityArgs args, string type)
        {
            if (map.Info.Pools is not Dict dict)
                return "npc_rat";
            if (!dict.Dictionary.TryGetValue(new Atom(type), out var value)
                || value is not List list
                || list.Contents.Length <= 0)
                return "npc_rat";
            return Rng.Random.Choose(list.Contents).Explain();
        }

        protected virtual IEntityBuilder<PhysicalEntity> GetEntity(MapDef map, GetEntityArgs args, GameEntityBuilders e)
        {
            var key = args.Type.ToString().ToErgoCase();
            if (args.RandomType)
            {
                key = GetRandomEntityKey(map, args, key);
            }
            if (Spawn.BuilderMethods.TryGetValue(key, out var builder))
            {
                var extraArgs = args.Args is Dict d ? d : new Dict(WellKnown.Literals.Discard);
                if (Spawn.TryGetParams(builder, extraArgs, out var newParams))
                    return (IEntityBuilder<PhysicalEntity>)builder.Invoke(e, newParams);
                return (IEntityBuilder<PhysicalEntity>)builder.Invoke(e, []);
            }
            throw new NotSupportedException();
        }

        protected virtual void ProcessMarkers(MapDef map, FloorGenerationContext ctx)
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
                    case MapMarkerName.Entity when dict.Matches<GetEntityArgs>(out var args):
                        ctx.AddObject(nameof(MapMarkerName.Entity), def.Position, e => GetEntity(map, args, e));
                        break;
                }
            }
            ctx.RemoveObjects(markers.Contains);
        }
    }
}
