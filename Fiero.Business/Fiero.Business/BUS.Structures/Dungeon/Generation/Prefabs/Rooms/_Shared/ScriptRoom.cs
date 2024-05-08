namespace Fiero.Business
{
    public class ScriptRoom : Room
    {
        public readonly ErgoScript Script;

        public ScriptRoom(ErgoScript script)
        {
            Script = script;
        }

        public override void Draw(FloorGenerationContext ctx)
        {
            DrawRects(ctx);
            DrawConnectors(ctx);



            OnDrawn(ctx);
        }
    }
}
