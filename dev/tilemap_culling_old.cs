using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledSharp;


namespace FerrumModules.Engine
{
    public class TileMap : Sprite
    {
        private readonly List<List<int>> mapValues = new List<List<int>>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        private static int ChunkWidth;
        private static int ChunkHeight;

        public TileMap(string mapFilePath, int chunkWidth = 16, int chunkHeight = 16) : base(null, 0, 0)
        {
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
            Centered = false;
            LoadTMX(mapFilePath);
        }

        public void LoadTMX(string mapFilePath)
        {
            var mapFile = new TmxMap("Content/Maps/" + mapFilePath + ".tmx");

            Texture = Assets.Textures[Path.GetFileNameWithoutExtension(mapFile.Tilesets[0].Image.Source)];

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
            Vector2 originalPosition = PositionOffset;

            var chunkScaleFactorX = ScaleOffset.X * TileWidth;
            var chunkScaleFactorY = ScaleOffset.Y * TileHeight;

            for (int chunkY = 0; chunkY < Height / ChunkHeight + 1; chunkY++)
            {
                var scaledChunkPositionY = chunkY * ChunkHeight;
                for (int chunkX = 0; chunkX < Width / ChunkWidth + 1; chunkX++)
                {
                    var scaledChunkPositionX = chunkX * ChunkWidth;
                    var chunkBoundingBox = Rotation.RotatedRectAABB(new Rectangle
                        (
                            (int)(scaledChunkPositionX * chunkScaleFactorX),
                            (int)(scaledChunkPositionY * chunkScaleFactorY),
                            (int)(ChunkWidth * chunkScaleFactorX),
                            (int)(ChunkHeight * chunkScaleFactorY)
                        ), GlobalAngle);

                    chunkBoundingBox.X += (int)(PositionOffset.X * chunkScaleFactorX);
                    chunkBoundingBox.Y += (int)(PositionOffset.Y * chunkScaleFactorY);

                    if (Collision.RectsCollide(Scene.Camera.BoundingBox, chunkBoundingBox)) // Check collision if tile chunk is on screen
                    {
                        for (int tileY = 0; tileY < ChunkHeight; tileY++)
                        {
                            if (scaledChunkPositionY + tileY >= mapValues.Count) break;
                            for (int tileX = 0; tileX < ChunkWidth; tileX++)
                            {
                                if (scaledChunkPositionX + tileX >= mapValues[scaledChunkPositionY + tileY].Count) break;
                                if (IsTileSolid(scaledChunkPositionX + tileX, scaledChunkPositionY + tileY))
                                {
                                    PositionOffset = (
                                        new Vector2(tileX * TileWidth, tileY * TileHeight) +
                                        new Vector2(scaledChunkPositionX * TileWidth, scaledChunkPositionY * TileHeight)
                                        + originalPosition) * GlobalScale;

                                    CurrentFrame = mapValues[scaledChunkPositionY + tileY][scaledChunkPositionX + tileX];
                                    base.Render(spriteBatch, spriteBatchEffects);
                                }
                            }
                        }
                        PositionOffset = originalPosition;
                    }
                }
            }
        }

        private bool IsTileSolid(int x, int y)
        {
            if (x > mapValues[0].Count || y > mapValues.Count) return false;
            if (x < 0 || y < 0) return false;
            return mapValues[y][x] >= 0;
        }
    }
}
