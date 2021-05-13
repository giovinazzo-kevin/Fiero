using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System;
using System.Linq;

namespace Fiero.Business
{

    public class ActorDialogue : Paragraph
    {
        protected readonly int TileSize;
        protected readonly Func<string, Sprite> GetSprite;
        protected readonly Func<SoundName, Sound> GetSound;


        public Sprite Face { get; set; }
        protected readonly Sprite Cursor;


        public event Action<DialogueNode> NodeChanged;
        private DialogueNode _node;
        public DialogueNode Node {
            get => _node;
            set {
                _node = value;
                _selectedChoiceIndex = 0;
                Children.RemoveAll(x => x is Paragraph);
                if (_node != null) {
                    SelectedChoice = _node.Choices.Keys.FirstOrDefault();
                    Text.V = String.Join('\n', _node.Lines);
                    foreach (var label in Children.OfType<Label>()) {
                        label.Position.V = new(label.Position.V.X + TileSize * 5, label.Position.V.Y);
                        label.Scale.V = new(2, 2);
                    }
                    if (_node.Choices.Any()) {
                        var length = _node.Choices.Keys.Max(x => x.Length) * 2 + 2;
                        // var frame = BuildFrame(new((length + 1) * TileSize, _node.Choices.Count * TileSize * 2 + 1 * TileSize));
                        var p = new Paragraph(Input, GetText);
                        p.Scale.V = Scale.V;
                        p.Position.V = new(
                            Position.V.X + (MaxLength - length) * TileSize - TileSize,
                            Position.V.Y + MaxLines * TileSize + 3 * TileSize
                        );
                        p.Foreground.V = Foreground.V;
                        p.MaxLength.V = length - 2;
                        p.MaxLines.V = _node.Choices.Count;
                        p.Size.V = new(length * TileSize, _node.Choices.Count * TileSize + 2 * TileSize);
                        p.Text.V = String.Join('\n', _node.Choices.Keys.Select(k => k.PadLeft(k.Length + 1)));
                        Children.Add(p);
                    }
                    Face = GetSprite($"face-{_node.Face}");
                    if (Face != null) {
                        Face.Scale = new(4, 4);
                        Face.Position = new(Position.V.X + TileSize, Position.V.Y + TileSize);
                    }
                    NodeChanged?.Invoke(_node);
                }
                else {
                    SelectedChoice = null;
                    Text.V = String.Empty;
                    Face = null;
                }
            }
        }

        private int _selectedChoiceIndex;
        public string SelectedChoice { get; private set; }

        protected void PlayBlip() => new Sound(GetSound(SoundName.UIBlip)).Play();
        protected void PlayOk() => new Sound(GetSound(SoundName.UIOk)).Play();

        public ActorDialogue(GameInput input, int tileSize, Func<SoundName, Sound> getSound, Func<string, int, Text> getText, Func<string, Sprite> getSprite)
            : base(input, getText)
        {
            GetSound = getSound;
            GetSprite = getSprite;
            TileSize = tileSize;
            Cursor = GetSprite("hand-l");
        }

        public override void Update(float t, float dt)
        {
            base.Update(t, dt);
            if (Node == null)
                return;
            if (Node.Choices.Count > 0) {
                if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad2)) {
                    _selectedChoiceIndex = (++_selectedChoiceIndex % Node.Choices.Count);
                    SelectedChoice = Node.Choices.Keys.ElementAtOrDefault(_selectedChoiceIndex);
                    PlayBlip();
                }
                else if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad8)) {
                    _selectedChoiceIndex = ((--_selectedChoiceIndex % Node.Choices.Count + Node.Choices.Count) % Node.Choices.Count);
                    SelectedChoice = Node.Choices.Keys.ElementAtOrDefault(_selectedChoiceIndex);
                    PlayBlip();
                }
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Enter)) {
                Node = SelectedChoice == null ? Node.Next : Node.Choices[SelectedChoice];
                PlayOk();
            }

            if (SelectedChoice != null) {
                var p = Children.OfType<Paragraph>().Single();
                var lPos = p.Children.OfType<Label>()
                    .Single(l => l.Text.V.Trim().Equals(SelectedChoice, StringComparison.OrdinalIgnoreCase))
                    .Position;
                Cursor.Position = new(lPos.V.X, lPos.V.Y);
                Cursor.Scale = new(2, 2);
            }
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if (Face != null) {
                Face.Draw(target, states);
            }
            if (SelectedChoice != null) {
                Cursor.Draw(target, states);
            }
        }
    }
}
