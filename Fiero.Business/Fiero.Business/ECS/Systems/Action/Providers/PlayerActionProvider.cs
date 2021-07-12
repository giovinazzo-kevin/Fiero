using Fiero.Core;

namespace Fiero.Business
{
    public class PlayerActionProvider : ActionProvider
    {
        protected readonly GameInput Input;
        protected readonly GameDataStore Store;

        public PlayerActionProvider(GameInput input, GameDataStore store)
        {
            Input = input;
            Store = store;
        }

        public override IAction GetIntent(Actor a)
        {
            var moveIntent = Input.IsKeyDown(SFML.Window.Keyboard.Key.LControl)
                ? ActionName.Attack
                : ActionName.Move;
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad7)) {
                return new MoveRelativeAction(new(-1, -1));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad8)) {
                return new MoveRelativeAction(new(0, -1));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad9)) {
                return new MoveRelativeAction(new(1, -1));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad4)) {
                return new MoveRelativeAction(new(-1, 0));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad5)) {
                return new MoveRelativeAction(new(0, 0));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad6)) {
                return new MoveRelativeAction(new(1, 0));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad1)) {
                return new MoveRelativeAction(new(-1, 1));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad2)) {
                return new MoveRelativeAction(new(0, 1));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad3)) {
                return new MoveRelativeAction(new(1, 1));
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.G)) {
                return new InteractRelativeAction();
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.I)) {

            }
            return new NoAction();
        }
    }
}
