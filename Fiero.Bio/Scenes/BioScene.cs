using Fiero.Core;
using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fiero.Bio
{
    public struct Reagent
    {
        public string Name { get; set; }
        public float DiffusionRate { get; set; }
    }

    public struct Reaction
    {
        public (int N, Reagent Reagent) A { get; set; }
        public (int N, Reagent Reagent) B { get; set; }
    }

    public static class Reagents
    {
        public static readonly Reagent A = new() {
            Name = nameof(A),
            DiffusionRate = 1f
        };
        public static readonly Reagent B = new() {
            Name = nameof(B),
            DiffusionRate = 1f
        };
    }

    public class BioScene : GameScene<BioScene.SceneState>
    {
        public enum SceneState
        {
            Main
        }

        protected override bool CanChangeStateAsync(SceneState newState) => true;
        protected override void OnStateChangedAsync(SceneState oldState) { }


        public readonly IReadOnlyList<Reaction> Reactions;
        public readonly HardwareGrid Grid;
        protected readonly GameInput Input;

        public BioScene(GameInput input)
        {
            Input = input;
            Reactions = new List<Reaction>() {
                { new Reaction() { A = (1, Reagents.A), B = (2, Reagents.B) } }
            };
            Grid = new HardwareGrid(512, 512);
        }

        public override void Draw(RenderWindow win, float t, float dt)
        {
            win.Clear();
            if (Input.IsKeyDown(Keyboard.Key.R)) {
                Grid.SetPixels(c => {
                    var rng = new Random();
                    var A = (byte)255;
                    var B = (byte)(rng.Next(0, 2) * 255);
                    if (c.Dist(new(256, 256)) < 100) {
                        return new(0, 0, B, 255);
                    }
                    return new(A, 0, 0, 255);
                }, maxDegreeOfParallelism: 8);
            }

            Grid.ReactionDiffusion(1f, 0.5f, 0.0367f, 0.0569f, 1f);
            Grid.Flow(t * 100, 0.1f, 1f);
            win.Draw(Grid);
            //using var img = new HardwareGrid(Grid);
            //img.SetPixels(p => {
            //    var c = img.GetPixel(p);
            //    var X = c.B;
            //    return new(X, X, X, 255);
            //});
            //win.Draw(img);
        }

    }
}
