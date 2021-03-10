using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledSharp;


namespace FerrumModules.Engine
{
    public class TileMap : Sprite
    {
        private struct TileSet
        {
            public Texture2D Texture;
            public int FirstGid;
            public int LastGid;
        }

        public override float GlobalAngle => 0.0f;
        public override bool Centered => false;

        public readonly List<List<int>> MapValues = new List<List<int>>();
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool InfiniteX = false;
        public bool InfiniteY = false;

        private readonly List<TileSet> tilesets = new List<TileSet>();

        public bool Infinite
        {
            get => InfiniteX || InfiniteY;
            set
            {
                InfiniteX = value;
                InfiniteY = value;
            }
        }

        public TileMap(string mapFilePath) : base(null, 0, 0)
        {
            Centered = false;
            LoadTMX(mapFilePath);
        }

        public void LoadTMX(string mapFilePath, int tileLayerID = 0, bool getNameFromFile = false)
        {
            var mapFile = new TmxMap("Content/Maps/" + mapFilePath + ".tmx");

            TileWidth = mapFile.TileWidth;
            TileHeight = mapFile.TileHeight;
            Width = mapFile.Width;
            Height = mapFile.Height;

            tilesets.Clear();
            foreach (var tmxTileset in mapFile.Tilesets)
            {
                var newTileset = new TileSet
                {
                    Texture = Assets.Textures[Path.GetFileNameWithoutExtension(tmxTileset.Image.Source)],
                    FirstGid = tmxTileset.FirstGid,
                    LastGid = tmxTileset.FirstGid + (int)tmxTileset.TileCount - 1
                };
                tilesets.Add(newTileset);
            }

            var tmxTileLayer = mapFile.TileLayers[tileLayerID];
            MapValues.Clear();
            for (int y = 0; y < mapFile.Height; y++)
            {
                MapValues.Add(new List<int>());
                for (int x = 0; x < mapFile.Width; x++)
                {
                    MapValues[y].Add(tmxTileLayer.Tiles[(y * mapFile.Width) + x].Gid);
                }
            }

            if (getNameFromFile) Name = tmxTileLayer.Name;
            Visible = tmxTileLayer.Visible;
            OpacityOffset = (float)tmxTileLayer.Opacity;
            PositionOffset = new Vector2((float)tmxTileLayer.OffsetX, (float)tmxTileLayer.OffsetY);
        }

        private int SignConsciousModulus(int value, int divisor)
        {
            if (value >= 0) return value % divisor;
            return (divisor - (-value % divisor)) % divisor;
        }

        public override void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            Vector2 originalPosition = PositionOffset;

            var cameraBox = Scene.Camera.BoundingBox;
            var cameraBoxPosition = new Vector2(cameraBox.X, cameraBox.Y);
            var cameraBoxSize = new Vector2(cameraBox.Width, cameraBox.Height);

            var globalTileSize = new Vector2(TileWidth, TileHeight);
            var tileFrameStart = (cameraBoxPosition - GlobalPosition) / globalTileSize / GlobalScale;
            var tileFrameEnd = cameraBoxSize / globalTileSize / GlobalScale + tileFrameStart;

            var tileFrameStartX = (int)tileFrameStart.X - 1;
            var tileFrameStartY = (int)tileFrameStart.Y - 1;
            var tileFrameEndX = (int)tileFrameEnd.X + 1;
            var tileFrameEndY = (int)tileFrameEnd.Y + 1;

            TileSet currentTileSet = TileSetForGid(MapValues[0][0]);

            for (var i = tileFrameStartY; i < tileFrameEndY; i++)
            {
                int tileRowIndex;
                if (InfiniteY) tileRowIndex = SignConsciousModulus(i, MapValues.Count);
                else
                {
                    if (i >= MapValues.Count) break;
                    if (i < 0) continue;
                    tileRowIndex = i;
                }
                for (var j = tileFrameStartX; j < tileFrameEndX; j++)
                {
                    int tileColumnIndex;
                    if (InfiniteX) tileColumnIndex = SignConsciousModulus(j, MapValues[tileRowIndex].Count);
                    else
                    {
                        if (j >= MapValues[tileRowIndex].Count) break;
                        if (j < 0) continue;
                        tileColumnIndex = j;
                    }

                    PositionOffset = new Vector2(j, i) * globalTileSize * GlobalScale + originalPosition;

                    var currentTileGid = MapValues[tileRowIndex][tileColumnIndex];
                    if ((currentTileGid < currentTileSet.FirstGid) || (currentTileGid > currentTileSet.LastGid))
                    {
                        currentTileSet = TileSetForGid(currentTileGid);
                    }

                    if (currentTileGid >= currentTileSet.FirstGid)
                    {
                        CurrentFrame = currentTileGid - currentTileSet.FirstGid;
                        base.Render(spriteBatch, spriteBatchEffects);
                    }
                }
            }

            PositionOffset = originalPosition;
        }

        private TileSet TileSetForGid(int tileGid)
        {
            TileSet tileSet = tilesets[0];
            foreach (var t in tilesets)
            {
                if ((tileGid > t.FirstGid) && (tileGid < t.LastGid))
                    tileSet = t;
            }

            Texture = tileSet.Texture;
            return tileSet;
        }
    }
}