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
        private static int ChunkSizeX;
        private static int ChunkSizeY;

        public FE_TileMap(string mapFilePath, Texture2D tileSetTexture, int chunkSizeX = 8, int chunkSizeY = 8) : base(tileSetTexture, 0, 0)
        {
            ChunkSizeX = chunkSizeX;
            ChunkSizeY = chunkSizeY;
            Centered = false;
            LoadTMX(mapFilePath, tileSetTexture);
        }

        public void LoadTMX(string mapFilePath, Texture2D tileSetTexture)
        {
            var mapFile = new TmxMap(mapFilePath);
            TileSizeX = mapFile.TileWidth;
            TileSizeY = mapFile.TileHeight;
            Width = mapFile.Width;
            Height = mapFile.Height;

            var totalTiles = tileSetTexture.Width / TileSizeX * tileSetTexture.Height / TileSizeY;
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

            for (int chunkY = 0; chunkY < Height / ChunkSizeY + 1; chunkY++)
            {
                var scaledChunkPositionY = chunkY * ChunkSizeY;
                for (int chunkX = 0; chunkX < Width / ChunkSizeX + 1; chunkX++)
                {
                    var scaledChunkPositionX = chunkX * ChunkSizeX;
                    if (FE_Collision.RectsCollide(Scene.Camera.BoundingBox, new Rectangle(
                        (int)((scaledChunkPositionX + Position.X) * Scale.X * TileSizeX),
                        (int)((scaledChunkPositionY + Position.Y) * Scale.Y * TileSizeY),
                        (int)(ChunkSizeX * TileSizeX * Scale.X),
                        (int)(ChunkSizeY * TileSizeY * Scale.Y)
                        ))) // Check collision if tile chunk is on screen
                    {
                        for (int tileY = 0; tileY < ChunkSizeY; tileY++)
                        {
                            if (scaledChunkPositionY + tileY >= mapValues.Count) break;
                            for (int tileX = 0; tileX < ChunkSizeX; tileX++)
                            {
                                if (scaledChunkPositionX + tileX >= mapValues[scaledChunkPositionY + tileY].Count) break;
                                Position = (
                                    new Vector2(tileX * TileSizeX, tileY * TileSizeY) +
                                    new Vector2(scaledChunkPositionX * TileSizeX, scaledChunkPositionY * TileSizeY)
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
