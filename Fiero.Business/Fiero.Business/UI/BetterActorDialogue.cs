using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class BetterActorDialogue : Layout
    {
        private Picture<TextureName> _picture, _cursor;
        private Paragraph _paragraph, _choices;
        private int _selectedChoiceIndex;

        protected readonly Func<SoundName, Sound> GetSound;

        public readonly UIControlProperty<DialogueNode> Node = new(nameof(Node));
        public readonly UIControlProperty<string> SelectedChoice = new(nameof(SelectedChoice));

        public BetterActorDialogue(GameInput input, GameUI ui, Func<SoundName, Sound> getSound)
            : base(input)
        {
            GetSound = getSound;
            var dialogue = ui.CreateLayout()
                .Build(Size, grid => grid
                    .Row(h: 1.25f)
                        .Style<Paragraph>(p => {
                            p.MaxLines.V = 4;
                            p.FontSize.V = 16;
                            p.Background.V = Color.Red;
                        })
                        .Style<Picture<TextureName>>(p => {
                            p.Center.V = true;
                            p.TextureName.V = TextureName.UI; // Actor faces are found here
                            p.Scale.V = new(0.8f, 0.8f); // Leave some margin around the edges
                        })
                        .Col(w: 0.25f)
                            .Cell<Picture<TextureName>>(p => _picture = p)
                        .End()
                        .Col(w: 1.75f)
                            .Cell<Paragraph>(p => _paragraph = p)
                        .End()
                    .End()
                    .Row(h: 0.75f)
                        .Style<Paragraph>(p => {
                            p.MaxLines.V = 4;
                            p.FontSize.V = 12;
                            p.Background.V = Color.Cyan;
                        })
                        .Style<Picture<TextureName>>(p => {
                            p.TextureName.V = TextureName.UI; 
                        })
                        .Col(w: 0.1f)
                            .Col(h: 0.25f)
                                .Cell<Picture<TextureName>>(p => _cursor = p)
                            .End()
                            .Col(h: 1.75f).End()
                        .End()
                        .Col(w: 1.9f)
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
                    _picture.SpriteName.V = String.Empty;
                    _cursor.SpriteName.V = String.Empty;
                    _paragraph.Text.V = String.Empty;
                    _choices.Text.V = String.Empty;
                    _selectedChoiceIndex = 0;
                    SelectedChoice.V = null;
                    return;
                }

                _picture.SpriteName.V = $"face-{Node.V.Face}";
                _paragraph.Text.V = String.Join("\n", Node.V.Lines);
                _selectedChoiceIndex = 0;
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

            _choices.Text.V = String.Join("\n", Node.V.Choices.Keys);
            _cursor.SpriteName.V = "hand-l";
            _cursor.Position.V = new(
                _cursor.Position.V.X,
                _choices.Children.OfType<Label>().ElementAt(_selectedChoiceIndex).Position.V.Y);
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
                if (Node.V.Choices.Count == 0) {
                    Node.V = Node.V.Next;
                }
                else {
                    Node.V = Node.V.Choices[SelectedChoice];
                }
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
