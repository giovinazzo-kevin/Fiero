using Fiero.Core;
using SFML.Graphics;
using static SFML.Window.Keyboard;

namespace Fiero.Business.Scenes
{
    public class TrackerScene : GameScene<TrackerScene.SceneState>
    {
        public enum SceneState
        {
            Main,
            Exit
        }

        protected readonly Mixer Mixer;
        protected readonly Instrument Ins;

        protected readonly GameInput Input;
        protected readonly GameUI UI;

        protected Layout Layout { get; private set; }

        public TrackerScene(GameInput input, GameUI ui)
        {
            Input = input;
            UI = ui;
            Mixer = new Mixer(0, 44100);
            Ins = new Instrument(() => new Oscillator(OscillatorShape.Triangle));
            Mixer.Master.Attach(Ins);
            Mixer.Unbuffered.V = true;
        }

        public override void Initialize()
        {
            Layout = UI.CreateLayout()
                .Build(new(), grid => grid
                    .Row(id: "navbar", h: 0.1f)
                        .Col(w: 0.5f)
                            .Cell<Label>(x => {
                                x.Background.V = new(255, 0, 0);
                                x.Text.V = "Tracker";
                            })
                        .End()
                        .Col(w: 0.5f)
                            .Cell<Label>(x => {
                                x.Background.V = new(0, 255, 0);
                                x.Text.V = "Tracker";
                            })
                        .End()
                    .End()
                    .Row(id: "rows", h: 1.9f)
                    .End()
                );
            Data.UI.WindowSize.ValueChanged += e => {
                Layout.Size.V = e.NewValue;
            };
        }


        protected override bool CanChangeState(SceneState newState) => true;
        protected override void OnStateChanged(SceneState oldState)
        {
            Mixer.Stop();
            if(State == SceneState.Main) {
                Mixer.Play();
            }
        }

        public override void Update(RenderWindow win, float t, float dt)
        {
            Layout.Update(t, dt);
            if (Input.IsKeyPressed(Key.Q)) {
                Ins.Play(Note.C, 3, 0.25f);
            }
            if (Input.IsKeyPressed(Key.W)) {
                Ins.Play(Note.D, 3, 0.25f);
            }
            if (Input.IsKeyPressed(Key.E)) {
                Ins.Play(Note.E, 3, 0.25f);
            }
            if (Input.IsKeyPressed(Key.R)) {
                Ins.Play(Note.F, 3, 0.25f);
            }
            if (Input.IsKeyPressed(Key.T)) {
                Ins.Play(Note.G, 3, 0.25f);
            }
            if (Input.IsKeyPressed(Key.Y)) {
                Ins.Play(Note.A, 3, 0.25f);
            }
            if (Input.IsKeyPressed(Key.U)) {
                Ins.Play(Note.B, 3, 0.25f);
            }
            if (Input.IsKeyPressed(Key.I)) {
                Ins.Play(Note.C, 4, 0.25f);
            }
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            win.Clear(Color.Black);
            win.Draw(Layout);
        }

    }
}
