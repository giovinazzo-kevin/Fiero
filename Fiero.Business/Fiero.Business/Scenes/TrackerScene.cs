//using Fiero.Core;
//using SFML.Graphics;
//using SFML.Window;
//using System;
//using System.Collections;
//using System.Linq;
//using System.Security.Cryptography.X509Certificates;
//using static SFML.Window.Keyboard;

//namespace Fiero.Business.Scenes
//{
//    public class TrackerScene : GameScene<TrackerScene.SceneState>
//    {
//        public enum SceneState
//        {
//            Main,
//            Exit
//        }

//        protected readonly Mixer Mixer;
//        protected readonly Tracker Tracker;
//        protected readonly Instrument Ins1;
//        protected readonly Instrument Ins2;
//        protected readonly Instrument Ins3;
//        protected readonly Instrument Ins4;

//        protected readonly GameInput Input;
//        protected readonly GameUI UI;

//        protected Layout Layout { get; private set; }
//        protected LayoutGrid CurrentRow { get; private set; }

//        public TrackerScene(GameInput input, GameUI ui)
//        {
//            Input = input;
//            UI = ui;
//            Mixer = new Mixer(4, 44100);
//            Ins1 = new Instrument(() => new Oscillator(OscillatorShape.Saw));
//            Ins2 = new Instrument(() => new Oscillator(OscillatorShape.Triangle));
//            Ins3 = new Instrument(() => new Oscillator(OscillatorShape.Sine));
//            Ins4 = new Instrument(() => new Oscillator(OscillatorShape.Square));
//            Tracker = new Tracker(4, Mixer);
//            Tracker.Tempo.V = 180;
//        }

//        public override void Initialize()
//        {
//            Tracker.Instruments.Add(Ins1);
//            Tracker.Instruments.Add(Ins2);
//            Tracker.Instruments.Add(Ins3);
//            Tracker.Instruments.Add(Ins4);
//            Mixer.Tracks[0].Attach(Ins1);
//            Mixer.Tracks[1].Attach(Ins2);
//            Mixer.Tracks[2].Attach(Ins3);
//            Mixer.Tracks[3].Attach(Ins4);
//            Mixer.Tracks[3].Effects.Add(new AmplitudeModulator());
//            Mixer.Unbuffered.V = false;

//            Layout = UI.CreateLayout()
//                .Build(new(), grid => grid
//                    .Row(id: "navbar", h: 0.1f)
//                        .Col(w: 1f)
//                            .Cell<Label>(x => {
//                                x.FontSize.V = 16;
//                                x.Text.V = "FieroTracker";
//                            })
//                        .End()
//                        .Col(w: 0.2f)
//                            .Cell<Checkbox>(x => {
//                                x.Checked.V = Mixer.Unbuffered.V;
//                                x.Checked.ValueChanged += (_, __) => {
//                                    Mixer.Unbuffered.V = !x.Checked.V;
//                                };
//                            })
//                        .End()
//                        .Col(w: 1.8f)
//                            .Cell<Label>(x => {
//                                x.FontSize.V = 8;
//                                x.Text.V = "Unbuffered Mixer";
//                                x.CenterContentH.V = false;
//                            })
//                        .End()
//                    .End()
//                    .Row(id: "controls", h: 0.1f)
//                        .Col(@class: "control play pause").Cell<Button>(x => {
//                            x.Text.V = GetText(Tracker.State);
//                            Tracker.StateChanged += (_, __) => {
//                                x.Text.V = GetText(Tracker.State);
//                            };
//                            x.Clicked += (_, __, ___) => {
//                                if (Tracker.State == TrackerState.Playing) {
//                                    Tracker.Pause();
//                                }
//                                else {
//                                    Tracker.Play();
//                                }
//                                return false;
//                            };
//                            static string GetText(TrackerState s) => s switch {
//                                TrackerState.Playing => "PAUSE",
//                                _ => "PLAY",
//                            };
//                        })
//                        .End()
//                        .Col(@class: "control stop").Cell<Button>(x => {
//                            x.Text.V = "STOP";
//                            x.Clicked += (_, __, ___) => {
//                                Tracker.Stop();
//                                return false;
//                            };
//                        })
//                        .End()
//                    .End()
//                    .Row(id: "header", h: 0.1f)
//                        .Style<Label>(s => s.Apply(x => { x.FontSize.V = 8; }))
//                        .Col().Cell<Label>(x => x.Text.V = "#").End()
//                        .Repeat(Tracker.Channels.Count, (j, grid) => grid
//                        .Col(@class: "row separator").End()
//                        .Col(@class: "row note octave")
//                            .Cell<Label>(x => {
//                                x.Text.V = "Note/Oct";
//                                x.Clicked += (_, __, button) => {
//                                    if (button == Mouse.Button.Right) {
//                                        foreach (var item in Layout.Dom.Query(x => x.HasAllClasses($"col_{j}", "note"))
//                                            .SelectMany(x => x.GetAllControlInstances())) {
//                                            item.IsActive.V = true;
//                                        }
//                                        return true;
//                                    }
//                                    return false;
//                                };
//                            })
//                        .End()
//                        .Col(@class: "row instrument")
//                            .Cell<Label>(x => {
//                                x.Text.V = "Inst";
//                                x.Clicked += (_, __, button) => {
//                                    if (button == Mouse.Button.Right) {
//                                        foreach (var item in Layout.Dom.Query(x => x.HasAllClasses($"col_{j}", "instrument"))
//                                            .SelectMany(x => x.GetAllControlInstances())) {
//                                            item.IsActive.V = true;
//                                        }
//                                        return true;
//                                    }
//                                    return false;
//                                };
//                            })
//                        .End()
//                        .Col(@class: "row volume")
//                            .Cell<Label>(x => {
//                                x.Text.V = "Vol";
//                                x.Clicked += (_, __, button) => {
//                                    if (button == Mouse.Button.Right) {
//                                        foreach (var item in Layout.Dom.Query(x => x.HasAllClasses($"col_{j}", "volume"))
//                                            .SelectMany(x => x.GetAllControlInstances())) {
//                                            item.IsActive.V = true;
//                                        }
//                                        return true;
//                                    }
//                                    return false;
//                                };
//                            })
//                        .End())
//                    .End()
//                    .Row(id: "rows", h: 2.7f)
//                        .Repeat(Tracker.RowsPerPattern, (i, grid) => grid
//                        .Row(@class: "row", id: $"row_{i}", h: 0.5f)
//                            .Style<Label>(s => s.Apply(x => {
//                                x.FontSize.V = 9;
//                                x.Background.V = i % 2 == 0 ? new(0, 0, 0) : new(30, 30, 30);
//                                Tracker.Step += (t, r) => {
//                                    if (r == i) {
//                                        x.Background.V = new(255, 255, 0);
//                                    }
//                                    else if (r == (i + 1) % Tracker.RowsPerPattern) {
//                                        x.Background.V = i % 2 == 0 ? new(0, 0, 0) : new(30, 30, 30);
//                                    }
//                                };
//                            }))
//                            .Col(@class: "row channel")
//                                .Col(@class: "row channel_id")
//                                    .Cell<Label>(x => x.Text.V = $"{i:X2}")
//                                .End()
//                                .Repeat(Tracker.Channels.Count, (j, grid) => grid
//                                .Col(@class: "row separator").End()
//                                .Col(@class: $"row col_{j} note octave")
//                                    .Cell<TrackerNoteCell>(x => {
//                                        x.Note.V = Tracker.Channels[j].GetRow(i).Note;
//                                        x.Octave.V = Tracker.Channels[j].GetRow(i).Octave;
//                                        x.Text.ValueChanged += (_, __) => {
//                                            var row = Tracker.Channels[j].GetRow(i)
//                                                .WithNote(x.Note.V)
//                                                .WithOctave(x.Octave.V);
//                                            Tracker.Channels[j].SetRow(i, row);
//                                            Tracker.PlayRow(row);
//                                        };
//                                    })
//                                .End()
//                                .Col(@class: $"row col_{j} instrument")
//                                    .Cell<TrackerHexCell>(x => {
//                                        x.MinValue.V = 1;
//                                        x.MaxValue.V = 4;
//                                        x.Value.V = Tracker.Channels[j].GetRow(i).Instrument;
//                                        x.Value.ValueChanged += (_, __) => {
//                                            var row = Tracker.Channels[j].GetRow(i)
//                                                .WithInstrument(x.Value.V);
//                                            Tracker.Channels[j].SetRow(i, row);
//                                        };
//                                    })
//                                .End()
//                                .Col(@class: $"row col_{j} volume")
//                                    .Cell<TrackerHexCell>(x => {
//                                        x.MinValue.V = 0;
//                                        x.MaxValue.V = 255;
//                                        x.Value.V = Tracker.Channels[j].GetRow(i).Volume;
//                                        x.Value.ValueChanged += (_, __) => {
//                                            var row = Tracker.Channels[j].GetRow(i)
//                                                .WithVolume(x.Value.V);
//                                            Tracker.Channels[j].SetRow(i, row);
//                                        };
//                                    })
//                                .End())
//                            .End()
//                        .End())
//                    .End()
//                );
//            Data.UI.WindowSize.ValueChanged += e => {
//                if(State == SceneState.Main) {
//                    //Layout.Size.V = e.NewValue;
//                }
//            };
//        }


//        protected override bool CanChangeState(SceneState newState) => true;
//        protected override void OnStateChanged(SceneState oldState)
//        {
//            Mixer.Stop();
//            if(State == SceneState.Main) {
//                Mixer.Play();
//            }
//        }

//        public override void Update(RenderWindow win, float t, float dt)
//        {
//            Layout.Update(t, dt);
//            if(Input.IsKeyPressed(Key.Space)) {
//                if(Tracker.State == TrackerState.Playing) {
//                    Tracker.Pause();
//                }
//                else {
//                    Tracker.Play();
//                }
//            }
//            Tracker.Update(dt, out var row);
//        }

//        public override void Draw(RenderWindow win, float t, float dt)
//        {
//            win.Clear(Color.Black);
//            win.Draw(Layout);
//        }

//    }
//}
