namespace Fiero.Business
{
    public readonly record struct SpriteDef(TextureName Texture, string Sprite, ColorName Tint, Vec Offset, Vec Scale, float Alpha)
    {
    }
}
