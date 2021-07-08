using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{

    public class ActorDialogue : UIControl
    {
        private Picture<TextureName> _picture;
        private Paragraph _paragraph, _choices;
        private int _selectedChoiceIndex;

        protected readonly Func<SoundName, Sound> GetSound;
        protected readonly Func<ColorName, Color> GetColor;
        protected readonly GameDataStore Store;

        public readonly UIControlProperty<DialogueNode> Node = new(nameof(Node));
        public readonly UIControlProperty<string> SelectedChoice = new(nameof(SelectedChoice));

        public ActorDialogue(
            GameInput input, 
            GameDataStore store,
            GameUI ui, 
            Func<SoundName, Sound> getSound, 
            Func<ColorName, Color> getColor
        ) : base(input)
        {
            Store = store;
            GetSound = getSound;
            GetColor = getColor;
            var dialogue = ui.CreateLayout()
                .Build(Size, grid => grid
                    .Row()
                        .Style<Paragraph>(s => s.Apply(p => {
                            p.MaxLines.V = 4;
                            p.FontSize.V = 16;
                            p.CenterContentH.V = true;
                            p.Padding.V = new(8, 8);
                            p.Background.V = GetColor(ColorName.UIBackground);
                        }))
                        .Style<Picture<TextureName>>(s => s.Apply(p => {
                            p.HorizontalAlignment.V = HorizontalAlignment.Center;
                            p.Background.V = GetColor(ColorName.UIAccent);
                            p.TextureName.V = TextureName.UI; // Actor faces are found here
                            p.Scale.V = new(0.8f, 0.8f); // Leave some margin around the edges
                        }))
                        .Col(w: 0.25f)
                            .Cell<Picture<TextureName>>(p => _picture = p)
                        .End()
                        .Col(w: 1.75f)
                            .Cell<Paragraph>(p => _paragraph = p)
                        .End()
                    .End()
                    .Row(h: 1.5f)
                        .Style<Paragraph>(s => s.Apply(p => {
                            p.FontSize.V = 16;
                            p.Foreground.V = GetColor(ColorName.UIPrimary);
                            p.Background.V = Color.Transparent;
                            p.Padding.V = new(32, 32);
                            p.ConfigureText.V = t => {
                                t.OutlineColor = GetColor(ColorName.UIBackground);
                                t.OutlineThickness = 2f;
                            };
                        }))
                        .Col()
                            .Cell<Paragraph>(p => _choices = p)
                        .End()
                    .End());

            Children.Add(dialogue);

            Size.ValueChanged += (owner, old) => {
                dialogue.Size.V = Size.V;
            };
            Position.ValueChanged += (owner, old) => {
                dialogue.Position.V = Position.V;
            };

            Node.ValueChanged += (owner, old) => {
                if (Node.V == null) {
                    return;
                }

                _picture.SpriteName.V = $"face-{Node.V.Face}";
                _paragraph.Text.V = String.Join("\n", Node.V.Lines);
                _selectedChoiceIndex = 0;
                _choices.MaxLines.V = 1;
                SelectedChoice.V = Node.V.Choices.Count > 0 ? Node.V.Choices.Keys.First() : null;
                UpdateCursor();
            };
        }
        protected void PlayBlip() => GetSound(SoundName.UIBlip).Play();
        protected void PlayOk() => GetSound(SoundName.UIOk).Play();

        protected void UpdateCursor()
        {
            if (Node.V == null || Node.V.Choices.Count == 0)
                return;

            _choices.MaxLines.V = Node.V.Choices.Count;
            _choices.Text.V = String.Join("\n", Node.V.Choices.Keys);

            var choices = _choices.Children.OfType<Label>().ToList();
            for (int i = 0; i < choices.Count; i++) {
                choices[i].Foreground.V = i == _selectedChoiceIndex
                    ? GetColor(ColorName.UIAccent)
                    : GetColor(ColorName.UIPrimary);
            }
        }

        public override void Update(float t, float dt)
        {
            base.Update(t, dt);
            if (Node == null)
                return;
            if (Node.V.Choices.Count > 0) {
                if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad2)) {
                    _selectedChoiceIndex = (++_selectedChoiceIndex % Node.V.Choices.Count);
                    UpdateChoice();
                }
                else if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad8)) {
                    _selectedChoiceIndex = ((--_selectedChoiceIndex % Node.V.Choices.Count + Node.V.Choices.Count) % Node.V.Choices.Count);
                    UpdateChoice();
                }
            }
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Enter)) {
                _picture.SpriteName.V = String.Empty;
                _paragraph.Text.V = String.Empty;
                _choices.Text.V = String.Empty;
                if (Node.V.Choices.Count == 0) {
                    Node.V = Node.V.Next;
                }
                else {
                    Node.V = Node.V.Choices[SelectedChoice];
                }
                _selectedChoiceIndex = 0;
                SelectedChoice.V = null;
                PlayOk();
            }
            void UpdateChoice()
            {
                SelectedChoice.V = Node.V.Choices.Keys.ElementAtOrDefault(_selectedChoiceIndex);
                PlayBlip();
                UpdateCursor();
            }
        }
    }
}
