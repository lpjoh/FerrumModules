using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Crossfrog.Ferrum.Engine.Modules
{
    public static class Assets
    {
        public static FE_Engine Engine;
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        // To add textures to a game, use the MonoGame Pipeline Tool and open the Content.mcgb file.
        public static Texture2D GetTexture(string name)
        {
            if (!Textures.ContainsKey(name))
                Textures.Add(name, Engine.LoadAsset<Texture2D>("Textures/" + name));

            return Textures[name];
        }
    }
}
