namespace Fiero.Business
{
    public readonly struct QuaffPotionAction : IAction
    {
        public readonly Potion Potion;

        public QuaffPotionAction(Potion potion)
        {
            Potion = potion;
        }

        ActionName IAction.Name => ActionName.Quaff;
        int? IAction.Cost => 100;
    }
}
