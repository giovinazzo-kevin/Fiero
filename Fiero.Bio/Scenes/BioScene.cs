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

        protected override bool CanChangeState(SceneState newState) => true;
        protected override void OnStateChanged(SceneState oldState) { }


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

        public override void Draw()
        {
        }

    }
}
