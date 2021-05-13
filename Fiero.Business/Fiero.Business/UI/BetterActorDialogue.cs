using Fiero.Core;
using SFML.Audio;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class BetterActorDialogue : Layout
    {
        private Picture<TextureName> _picture;
        private Paragraph _paragraph;
        private LayoutGrid _choicesDom;
        private Layout _choices;
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
                    .Style<Picture<TextureName>>(p => {
                        p.Center.V = true;
                        p.TextureName.V = TextureName.UI; // Actor faces are found here
                        p.Scale.V = new(0.8f, 0.8f); // Leave some margin around the edges
                    })
                    .Style<Paragraph>(p => {
                        p.MaxLines.V = 4;
                        p.FontSize.V = 16;
                        p.Background.V = Color.Red;
                    })
                    .Row()
                        .Col(w: 0.25f)
                            .Cell<Picture<TextureName>>(p => _picture = p)
                        .End()
                        .Col(w: 1.75f)
                            .Cell<Paragraph>(p => _paragraph = p)
                        .End()
                    .End());

            Children.Add(_picture);
            Children.Add(_paragraph);

            Size.ValueChanged += (owner, old) => {
                dialogue.Size.V = Size.V;
            };
            Position.ValueChanged += (owner, old) => {
                dialogue.Position.V = Position.V;
            };

            Node.ValueChanged += (owner, old) => {
                if (_choicesDom != null) {
                    foreach (var node in _choicesDom.Query(x => x.IsCell && x.HasAnyClass("cursor", "label"))) {
                        Children.Remove(node.ControlInstance);
                    }
                }

                if (Node.V == null) {
                    _picture.SpriteName.V = String.Empty;
                    _paragraph.Text.V = String.Empty;
                    _selectedChoiceIndex = 0;
                    SelectedChoice.V = null;
                    _choicesDom = null;
                    _choices = null;
                    return;
                }

                _picture.SpriteName.V = $"face-{Node.V.Face}";
                _paragraph.Text.V = String.Join("\n", Node.V.Lines);
                _selectedChoiceIndex = 0;
                SelectedChoice.V = Node.V.Choices.Count > 0 ? Node.V.Choices.Keys.First() : null;

                _choices = ui.CreateLayout()
                    .Build(Size, grid => {
                        grid = grid
                            .Style<Picture<TextureName>>(p => {
                                p.TextureName.V = TextureName.UI; // Images are used for a pointer icon
                            })
                            .Style<Label>(p => {
                                p.FontSize.V = 12;
                                p.Background.V = Color.Cyan;
                            });

                        var i = 0;
                        foreach (var c in Node.V.Choices) {
                            grid = grid
                                .Row()
                                    .Col(w: 0.25f, @class: "cursor", id: $"{i++}")
                                        .Cell<Picture<TextureName>>()
                                    .End()
                                    .Col(w: 1.75f, @class: "label")
                                        .Cell<Label>(l => l.Text.V = c.Key)
                                    .End()
                                .End();
                        }

                        return _choicesDom = grid;
                    });

                foreach (var node in _choicesDom.Query(x => x.IsCell && x.HasAnyClass("cursor", "label"))) {
                    Children.Add(node.ControlInstance);
                }
                UpdateCursor();
            };
        }
        protected void PlayBlip() => GetSound(SoundName.UIBlip).Play();
        protected void PlayOk() => GetSound(SoundName.UIOk).Play();
        protected void UpdateCursor()
        {
            if (Node.V == null || Node.V.Choices.Count == 0)
                return;
            var cursor = _choicesDom.Query(x => x.Id == $"{_selectedChoiceIndex}")
                .Select(x => x.ControlInstance)
                .Cast<Picture<TextureName>>()
                .First();
            cursor.SpriteName.V = "hand-l";
            foreach (var nonCursor in _choicesDom.Query(x => x.HasClass("cursor") && x.Id != $"{_selectedChoiceIndex}")
                .Select(x => x.ControlInstance)
                .Cast<Picture<TextureName>>()) {
                nonCursor.SpriteName.V = String.Empty;
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
