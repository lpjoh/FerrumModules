using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledSharp;
using Box2DSharp.Dynamics;
using Box2DSharp.Collision.Shapes;

namespace FerrumModules.Engine
{
    public class FE_TileMap : FE_Sprite
    {
        private readonly List<List<int>> mapValues = new List<List<int>>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        private static int ChunkWidth;
        private static int ChunkHeight;

        public FE_TileMap(string mapFilePath, int chunkWidth = 16, int chunkHeight = 16) : base(null, 0, 0)
        {
            ChunkWidth = chunkWidth;
            ChunkHeight = chunkHeight;
            Centered = false;
            LoadTMX(mapFilePath);
        }

        public override void Init()
        {
            //TODO: Abstract collision creation, update when scaled
            base.Init();
            var boxBodyDef = new BodyDef { BodyType = BodyType.StaticBody };

            for (int y = 0; y < mapValues.Count; y++)
            {
                int consecutiveTiles = 0;
                Vector2 boxPosition = Vector2.Zero;
                for (int x = 0; x < mapValues[0].Count; x++)
                {
                    if (IsTileSolid(x, y))
                    {
                        if (consecutiveTiles == 0)
                        {
                            boxPosition = new Vector2(x, y) * new Vector2(TileWidth, TileHeight) * Scale + Position;
                        }
                        
                        consecutiveTiles++;
                    }
                    if ((!IsTileSolid(x, y) || x == mapValues[0].Count - 1) && consecutiveTiles != 0)
                    {
                        var boxScale = new Vector2(consecutiveTiles * TileWidth * Scale.X, TileHeight * Scale.Y) / 2;
                        boxBodyDef.Position = new System.Numerics.Vector2(boxPosition.X, boxPosition.Y);
                        var boxBody = Scene.PhysicsWorld.CreateBody(boxBodyDef);
                        PolygonShape dynamicBox = new PolygonShape();
                        dynamicBox.SetAsBox(boxScale.X, boxScale.Y, new System.Numerics.Vector2(boxScale.X, boxScale.Y), 0.0f);
                        FixtureDef fixtureDef = new FixtureDef { Shape = dynamicBox };

                        boxBody.CreateFixture(fixtureDef);
                        consecutiveTiles = 0;
                    }
                }
            }
        }

        public void LoadTMX(string mapFilePath)
        {
            var mapFile = new TmxMap("Content/Maps/" + mapFilePath + ".tmx");

            Texture = FE_Assets.Textures[Path.GetFileNameWithoutExtension(mapFile.Tilesets[0].Image.Source)];

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
                                if (IsTileSolid(scaledChunkPositionX + tileX, scaledChunkPositionY + tileY))
                                {
                                    Position = (
                                        new Vector2(tileX * TileWidth, tileY * TileHeight) +
                                        new Vector2(scaledChunkPositionX * TileWidth, scaledChunkPositionY * TileHeight)
                                        ) * Scale + originalPosition;

                                    CurrentFrame = mapValues[scaledChunkPositionY + tileY][scaledChunkPositionX + tileX];
                                    base.Render(spriteBatch, spriteBatchEffects);
                                }
                            }
                        }
                        Position = originalPosition;
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
