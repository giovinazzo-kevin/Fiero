using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;
using static SFML.Window.Keyboard;

namespace Fiero.Business
{
    /// <summary>
    /// A cell that stores a TrackerNote and a byte for the octave and represents it as a user-readable string like C#4 that can be typed in and shifted by the user.
    /// </summary>
    public class TrackerNoteCell : Label
    {
        protected readonly Key[] NoteKeys = new[] { Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G };
        protected readonly Key[] OctaveKeys = new[] { Key.Num0,Key.Num1,Key.Num2,Key.Num3,Key.Num4,Key.Num5,Key.Num6,Key.Num7,Key.Num8,Key.Num9,
            Key.Numpad0,Key.Numpad1,Key.Numpad2,Key.Numpad3,Key.Numpad4,Key.Numpad5,Key.Numpad6,Key.Numpad7,Key.Numpad8,Key.Numpad9 };

        public UIControlProperty<byte> MinOctave { get; private set; } = new(nameof(MinOctave), 1);
        public UIControlProperty<byte> MaxOctave { get; private set; } = new(nameof(MaxOctave), 15);
        public UIControlProperty<byte> Octave { get; private set; } = new(nameof(Octave));
        public UIControlProperty<TrackerNote> Note { get; private set; } = new(nameof(Note));

        public TrackerNoteCell(GameInput input, Func<string, BitmapText> getText)
            : base(input, getText)
        {
            IsInteractive.V = true;
            Octave.ValueChanged += (_, __) => {
                var clamped = Math.Clamp(Octave, MinOctave, MaxOctave);
                if (Octave.V != clamped) {
                    Octave.V = clamped;
                }
                else {
                    UpdateText();
                }
            };
            Note.ValueChanged += (_, __) => {
                UpdateText();
            };
            Text.ValueChanged += (_, __) => {
                // TODO
            };

            void UpdateText()
            {
                Text.V = Note.V switch {
                    TrackerNote.None => "   ",
                    TrackerNote.Stop => "---",
                    _ => $"{Note.V.ToString().Replace("s", "#").PadRight(2, '-')}{Octave.V:X1}"
                };
            }
        }

        public override void Update()
        {
            base.Update();
            if (!IsActive) {
                return;
            }
            var noteKeystrokes = Input.KeysPressed()
                .Intersect(NoteKeys);
            foreach (var k in noteKeystrokes) {
                Note.V = Enum.Parse<TrackerNote>(k.ToString());
            }
            var octaveKeystrokes = Input.KeysPressed()
                .Intersect(OctaveKeys);
            foreach (var k in octaveKeystrokes) {
                Octave.V = byte.Parse(k.ToString().Replace("Numpad", "").Replace("Num", ""));
            }
            // Toggle flat/sharp
            if (Input.IsKeyPressed(Key.S)) {
                Note.V = Note.V switch {
                    TrackerNote.A => TrackerNote.As,
                    TrackerNote.C => TrackerNote.Cs,
                    TrackerNote.D => TrackerNote.Ds,
                    TrackerNote.F => TrackerNote.Fs,
                    TrackerNote.G => TrackerNote.Gs,
                    TrackerNote.As => TrackerNote.A,
                    TrackerNote.Cs => TrackerNote.C,
                    TrackerNote.Ds => TrackerNote.D,
                    TrackerNote.Fs => TrackerNote.F,
                    TrackerNote.Gs => TrackerNote.G,
                    var x => x
                };
            }
            // Clear cell
            if (Input.IsKeyPressed(Key.Delete)) {
                Note.V = TrackerNote.None;
            }
            // Shift+up/down changes octave, up/down changes semitone
            if (Input.IsKeyPressed(Key.Up)) {
                if(Input.IsKeyDown(Key.LShift)) {
                    Octave.V += 1;
                }
                else {
                    if((int)Note.V >= (int)TrackerNote.C && (int)Note.V < (int)TrackerNote.B) {
                        Note.V += 1;
                    }
                    else if(Note.V == TrackerNote.B) {
                        Note.V = TrackerNote.C;
                        Octave.V += 1;
                    }
                }
            }
            if (Input.IsKeyPressed(Key.Down)) {
                if (Input.IsKeyDown(Key.LShift)) {
                    Octave.V -= 1;
                }
                else {
                    if ((int)Note.V > (int)TrackerNote.C && (int)Note.V <= (int)TrackerNote.B) {
                        Note.V -= 1;
                    }
                    else if (Note.V == TrackerNote.C) {
                        Note.V = TrackerNote.B;
                        Octave.V -= 1;
                    }
                }
            }
        }
    }
}
