namespace Fiero.Business
{

    public class ActionComponent : EcsComponent
    {
        public ActionComponent()
        {
        }

        public IAction LastAction { get; set; }
        public ActionProvider ActionProvider { get; set; }
        public int TurnsSurvived { get; set; }
    }
}
