using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledSharp;


namespace FerrumModules.Engine
{
    public class TileMap : Sprite
    {
        public override float GlobalAngle => 0.0f;

        private readonly List<List<int>> mapValues = new List<List<int>>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool Infinite = false;

        public TileMap(string mapFilePath) : base(null, 0, 0)
        {
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

            var cameraBox = Scene.Camera.BoundingBox;
            var cameraBoxPosition = new Vector2(cameraBox.X, cameraBox.Y);
            var cameraBoxSize = new Vector2(cameraBox.Width, cameraBox.Height);

            var globalTileSize = new Vector2(TileWidth, TileHeight) * GlobalScale;
            var tileScaledGlobalPosition = GlobalPosition / globalTileSize;

            var tileFrameStart = cameraBoxPosition / globalTileSize;
            var tileFrameEnd = (cameraBoxPosition + cameraBoxSize) / globalTileSize;

            var tileFrameStartX = (int)tileFrameStart.X - 1;
            var tileFrameStartY = (int)tileFrameStart.Y - 1;
            var tileFrameEndX = (int)tileFrameEnd.X + 1;
            var tileFrameEndY = (int)tileFrameEnd.Y + 1;

            for (var i = tileFrameStartY; i < tileFrameEndY; i++)
            {
                if (!Infinite)
                {
                    if (i >= mapValues.Count) break;
                    if (i < tileScaledGlobalPosition.Y) continue;
                }
                for (var j = tileFrameStartX; j < tileFrameEndX; j++)
                {
                    int tileRowIndex, tileColumnIndex;
                    if (!Infinite)
                    {
                        if (j >= mapValues[i].Count) break;
                        if (j < tileScaledGlobalPosition.X) continue;
                        tileRowIndex = i;
                        tileColumnIndex = j;
                    }
                    else
                    {
                        tileRowIndex = Math.Abs(i) % mapValues.Count;
                        tileColumnIndex = Math.Abs(j) % mapValues[tileRowIndex].Count;
                    }
                    PositionOffset = new Vector2(j, i) * globalTileSize;
                    if (mapValues[tileRowIndex][tileColumnIndex] >= 0)
                    {
                        CurrentFrame = mapValues[tileRowIndex][tileColumnIndex];
                        base.Render(spriteBatch, spriteBatchEffects);
                    }
                }
            }

            PositionOffset = originalPosition;
        }
    }
}
