namespace Fiero.Core
{

    public record struct LayoutUnit(float AbsolutePart, float RelativePart)
    {
        public static readonly LayoutUnit Zero = new(0, 0);
        public static readonly LayoutUnit RelativeOne = new(0, 1);
        public static readonly LayoutUnit AbsoluteOne = new(1, 0);

        public static LayoutUnit FromBool(float value, bool px) =>
            px switch
            {
                true => new(value, 0),
                false => new(0, value)
            };

        public override string ToString() => $"{AbsolutePart}px + {RelativePart}*";

        public static LayoutUnit operator +(LayoutUnit self, LayoutUnit other)
            => new LayoutUnit(self.AbsolutePart + other.AbsolutePart, self.RelativePart + other.RelativePart);
        public static LayoutUnit operator -(LayoutUnit self, LayoutUnit other)
            => new LayoutUnit(self.AbsolutePart - other.AbsolutePart, self.RelativePart - other.RelativePart);
        public static LayoutUnit operator *(LayoutUnit self, float scale)
            => new LayoutUnit(self.AbsolutePart * scale, self.RelativePart * scale);
        public static LayoutUnit operator /(LayoutUnit self, float scale)
            => new LayoutUnit(self.AbsolutePart / scale, self.RelativePart / scale);
    }

    public record struct LayoutPoint(LayoutUnit X, LayoutUnit Y)
    {
        public static readonly LayoutPoint Zero = new(LayoutUnit.Zero, LayoutUnit.Zero);
        public static readonly LayoutPoint RelativeOne = new(LayoutUnit.RelativeOne, LayoutUnit.RelativeOne);

        public static LayoutPoint FromAbsolute(Coord px) => new(new(px.X, 0), new(px.Y, 0));
        public static LayoutPoint FromRelative(Vec rel) => new(new(0, rel.X), new(0, rel.Y));

        public Vec RelativePart => new(X.RelativePart, Y.RelativePart);
        public Vec AbsolutePart => new(X.AbsolutePart, Y.AbsolutePart);

        public static LayoutPoint operator +(LayoutPoint self, LayoutPoint other)
            => new LayoutPoint(self.X + other.X, self.Y + other.Y);
        public static LayoutPoint operator -(LayoutPoint self, LayoutPoint other)
            => new LayoutPoint(self.X - other.X, self.Y - other.Y);
        public static LayoutPoint operator *(LayoutPoint self, float scale)
            => new LayoutPoint(self.X * scale, self.Y * scale);
        public static LayoutPoint operator /(LayoutPoint self, float scale)
            => new LayoutPoint(self.X / scale, self.Y / scale);
        public static LayoutPoint operator *(LayoutPoint self, Vec scale)
            => new LayoutPoint(self.X * scale.X, self.Y * scale.Y);
        public static LayoutPoint operator /(LayoutPoint self, Vec scale)
            => new LayoutPoint(self.X / scale.X, self.Y / scale.Y);
    }
}
