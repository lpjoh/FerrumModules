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

        private const string mapFilesDirectory = "Content/Maps";

        private static readonly Dictionary<string, TmxMap> mapFileDict = new Dictionary<string, TmxMap>();
        private static readonly Dictionary<string, List<TileSet>> mapFileTilesetsDict = new Dictionary<string, List<TileSet>>();
        private List<TileSet> tilesets;

        public bool Infinite
        {
            get => InfiniteX || InfiniteY;
            set
            {
                InfiniteX = value;
                InfiniteY = value;
            }
        }



        public TileMap(string mapFilePath, int tileLayerID = 0, bool getNameFromFile = false) : base(null, 0, 0)
        {
            LoadMapLayer(mapFilePath, tileLayerID, getNameFromFile);
        }

        private static TmxMap LoadTMXFile(string filePath)
        {
            var fullFilePath = mapFilesDirectory + "/" + filePath + ".tmx";
            if (!mapFileDict.ContainsKey(fullFilePath))
                mapFileDict[fullFilePath] = new TmxMap(fullFilePath);

            return mapFileDict[fullFilePath];
        }

        public void LoadMapLayer(string mapFilePath, int tileLayerID = 0, bool getNameFromFile = false)
        {
            var mapFile = LoadTMXFile(mapFilePath);
            TileWidth = mapFile.TileWidth;
            TileHeight = mapFile.TileHeight;
            Width = mapFile.Width;
            Height = mapFile.Height;

            if (!mapFileTilesetsDict.ContainsKey(mapFilePath))
            {
                var newTileSets = mapFileTilesetsDict[mapFilePath] = new List<TileSet>();
                foreach (var tmxTileset in mapFile.Tilesets)
                {
                    var newTileset = new TileSet
                    {
                        Texture = Assets.Textures[Path.GetFileNameWithoutExtension(tmxTileset.Image.Source)],
                        FirstGid = tmxTileset.FirstGid,
                        LastGid = tmxTileset.FirstGid + (int)tmxTileset.TileCount - 1
                    };
                    newTileSets.Add(newTileset);
                }
            }
            tilesets = mapFileTilesetsDict[mapFilePath];

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

        public override void Render(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
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
                        base.Render(spriteBatch);
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

        public class FileLoadedScene
        {
            public List<TileMap> TileLayers = new List<TileMap>();
            public List<List<Entity>> ObjectGroups = new List<List<Entity>>();

            public List<Entity> GetAsEntityList()
            {
                var entityList = new List<Entity>();

                foreach (var tl in TileLayers)
                    entityList.Add(tl);
                foreach (var og in ObjectGroups)
                    foreach (var o in og) entityList.Add(o);

                return entityList;
            }
        }

        public static string ObjectNamespace = "";

        public static FileLoadedScene LoadSceneFromFile<EnumType>(string mapFilePath, EnumType startingRenderLayer) where EnumType : Enum
        {
            if (ObjectNamespace == "") throw new Exception("Please set the object namespace.");

            var mapFile = LoadTMXFile(mapFilePath);
            var fileLoadedScene = new FileLoadedScene();

            for (int i = 0; i < mapFile.TileLayers.Count; i++)
            {
                var newTileLayer = new TileMap(mapFilePath, i, true);
                newTileLayer.SetRenderLayer(startingRenderLayer);
                fileLoadedScene.TileLayers.Add(newTileLayer);
            }

            foreach (var objectGroup in mapFile.ObjectGroups)
            {
                var objectGroupOffset = new Vector2((float)objectGroup.OffsetX, (float)objectGroup.OffsetY);

                var newEntityList = new List<Entity>();
                fileLoadedScene.ObjectGroups.Add(newEntityList);

                foreach (var o in objectGroup.Objects)
                {
                    var entityType = Type.GetType(ObjectNamespace + "." + o.Type);
                    if (entityType == null || !entityType.IsSubclassOf(typeof(Entity)))
                        throw new Exception("Entity in map file \"" + mapFilePath + "\" had an object labelled as type \"" +
                            o.Type + "\", which is not a valid entity type in the namespace \"" + ObjectNamespace + "\".");

                    var newEntity = (Entity)Activator.CreateInstance(entityType);

                    newEntity.Name = o.Name;
                    newEntity.Visible = o.Visible;
                    newEntity.PositionOffset = new Vector2((float)o.X, (float)o.Y) + objectGroupOffset;
                    newEntity.AngleOffset = (float)o.Rotation;

                    newEntityList.Add(newEntity);
                }
            }

            return fileLoadedScene;
        }
    }
}