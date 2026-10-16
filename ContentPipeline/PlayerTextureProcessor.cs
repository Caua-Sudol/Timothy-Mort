using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace DontLikePoetry.ContentPipeline;

// Only changes compiled content. Source PNGs and animation canvas alignment stay intact.
[ContentProcessor(DisplayName = "Player texture (nearest reduction)")]
public sealed class PlayerTextureProcessor : TextureProcessor
{
    public int ResolutionDivisor { get; set; } = 1;

    public override TextureContent Process(TextureContent input, ContentProcessorContext context)
    {
        if (ResolutionDivisor < 1)
            throw new InvalidContentException("ResolutionDivisor must be at least 1.");

        if (ResolutionDivisor == 1)
            return base.Process(input, context);

        input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
        foreach (MipmapChain face in input.Faces)
        {
            var source = (PixelBitmapContent<Color>)face[0];
            if (source.Width % ResolutionDivisor != 0 || source.Height % ResolutionDivisor != 0)
                throw new InvalidContentException("Player canvas dimensions must be divisible by ResolutionDivisor.");

            var reduced = new PixelBitmapContent<Color>(
                source.Width / ResolutionDivisor, source.Height / ResolutionDivisor);
            int offset = ResolutionDivisor / 2;
            for (int y = 0; y < reduced.Height; y++)
            for (int x = 0; x < reduced.Width; x++)
                reduced.SetPixel(x, y, source.GetPixel(
                    x * ResolutionDivisor + offset, y * ResolutionDivisor + offset));

            face.Clear();
            face.Add(reduced);
        }

        // Keep the usual alpha premultiplication and output format handling.
        return base.Process(input, context);
    }
}
