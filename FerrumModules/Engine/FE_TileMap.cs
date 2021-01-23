using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledSharp;

namespace FerrumModules.Engine
{
    public class FE_TileMap : FE_Sprite
    {
        private readonly List<List<int>> mapValues = new List<List<int>>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        private static int ChunkWidth;
        private static int ChunkHeight;

        public FE_TileMap(string mapFilePath, Texture2D tileSetTexture, int chunkWidth = 16, int chunkHeight = 16) : base(tileSetTexture, 0, 0)
        {
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
            Centered = false;
            LoadTMX(mapFilePath);
        }

        public void LoadTMX(string mapFilePath)
        {
            var mapFile = new TmxMap(mapFilePath);
            TileWidth = mapFile.TileWidth;
            TileHeight = mapFile.TileHeight;
            Width = mapFile.Width;
            Height = mapFile.Height;

            var firstGid = mapFile.Tilesets[0].FirstGid;

            mapValues.Clear();
            for (int y = 0; y < mapFile.Height; y++)
            {
                mapValues.Add(new List<int>());
                for (int x = 0; x < mapFile.Width; x++)
                {
                    mapValues[y].Add(mapFile.TileLayers[0].Tiles[(y * mapFile.Width) + x].Gid - firstGid);
                }
            }
        }

        public override void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            Vector2 originalPosition = Position;

            for (int chunkY = 0; chunkY < Height / ChunkHeight + 1; chunkY++)
            {
                var scaledChunkPositionY = chunkY * ChunkHeight;
                for (int chunkX = 0; chunkX < Width / ChunkWidth + 1; chunkX++)
                {
                    var scaledChunkPositionX = chunkX * ChunkWidth;
                    if (FE_Collision.RectsCollide(Scene.Camera.BoundingBox, new Rectangle(
                        (int)((scaledChunkPositionX + Position.X) * Scale.X * TileWidth),
                        (int)((scaledChunkPositionY + Position.Y) * Scale.Y * TileHeight),
                        (int)(ChunkWidth * TileWidth * Scale.X),
                        (int)(ChunkHeight * TileHeight * Scale.Y)
                        ))) // Check collision if tile chunk is on screen
                    {
                        for (int tileY = 0; tileY < ChunkHeight; tileY++)
                        {
                            if (scaledChunkPositionY + tileY >= mapValues.Count) break;
                            for (int tileX = 0; tileX < ChunkWidth; tileX++)
                            {
                                if (scaledChunkPositionX + tileX >= mapValues[scaledChunkPositionY + tileY].Count) break;
                                Position = (
                                    new Vector2(tileX * TileWidth, tileY * TileHeight) +
                                    new Vector2(scaledChunkPositionX * TileWidth, scaledChunkPositionY * TileHeight)
                                    ) * Scale + originalPosition;

                                CurrentFrame = mapValues[scaledChunkPositionY + tileY][scaledChunkPositionX + tileX];
                                base.Render(spriteBatch, spriteBatchEffects);
                            }
                        }
                        Position = originalPosition;
                    }
                }
            }
        }
    }
}
