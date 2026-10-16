using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Owned by PhaseRun; rooms borrow textures and never dispose shared entries.
internal sealed class LdtkTextureCache : IDisposable
{
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.Ordinal);

    public Texture2D GetOrLoad(GraphicsDevice graphics, string path)
    {
        if (_textures.TryGetValue(path, out Texture2D texture))
        {
            using var timing = new LoadDiagnostics($"tileset {path}");
            timing.Mark("cache reutilizado");
            return texture;
        }
        texture = LoadTexture(graphics, path);
        _textures.Add(path, texture);
        return texture;
    }

    // Called only after the replacement map and all its textures validate successfully.
    public void Replace(string path, Texture2D texture)
    {
        if (_textures.TryGetValue(path, out Texture2D previous) && previous != texture)
            previous.Dispose();
        _textures[path] = texture;
    }

    public static Texture2D LoadTexture(GraphicsDevice graphics, string path)
    {
        using var timing = new LoadDiagnostics($"tileset {path}");
        using var stream = File.OpenRead(path);
        Texture2D texture = Texture2D.FromStream(graphics, stream);
        try
        {
            timing.Mark("abrir PNG e FromStream");
            timing.Texture(texture);
            var pixels = new Color[texture.Width * texture.Height];
            texture.GetData(pixels);
            timing.Mark("alocar pixels e GetData");
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.FromNonPremultiplied(
                    pixels[i].R, pixels[i].G, pixels[i].B, pixels[i].A);
            timing.Mark("converter alpha");
            texture.SetData(pixels);
            timing.Mark("SetData");
            return texture;
        }
        catch
        {
            texture.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        foreach (Texture2D texture in _textures.Values) texture.Dispose();
        _textures.Clear();
    }
}
