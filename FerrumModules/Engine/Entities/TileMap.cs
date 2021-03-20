using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Crossfrog.Ferrum.Engine.Modules;

using TiledSharp;

namespace Crossfrog.Ferrum.Engine.Entities
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

        public readonly int[,] MapValues;
        public readonly int Width;
        public readonly int Height;

        public bool InfiniteX = false;
        public bool InfiniteY = false;

        private const string mapFilesDirectory = "Content/Maps";

        private static readonly Dictionary<string, TmxMap> mapFileDict = new Dictionary<string, TmxMap>();
        private static readonly Dictionary<string, TileSet[]> mapFileTilesetsDict = new Dictionary<string, TileSet[]>();
        private TileSet[] tilesets;

        public bool Infinite
        {
            get => InfiniteX || InfiniteY;
            set
            {
                InfiniteX = value;
                InfiniteY = value;
            }
        }

        public TileMap(string mapFilePath, int tileLayerID = 0, bool getNameFromFile = false)
        {
            var mapFile = LoadTMXFile(mapFilePath);

            Width = mapFile.Width;
            Height = mapFile.Height;
            MapValues = new int[mapFile.Height, mapFile.Width];
            LoadMapLayer(mapFilePath, tileLayerID, getNameFromFile);
        }

        private static TmxMap LoadTMXFile(string filePath)
        {
            var fullFilePath = mapFilesDirectory + "/" + filePath + ".tmx";
            if (!mapFileDict.ContainsKey(fullFilePath))
                mapFileDict[fullFilePath] = new TmxMap(fullFilePath);

            return mapFileDict[fullFilePath];
        }

        private void LoadMapLayer(string mapFilePath, int tileLayerID = 0, bool getNameFromFile = false)
        {
            var mapFile = LoadTMXFile(mapFilePath);
            TileWidth = mapFile.TileWidth;
            TileHeight = mapFile.TileHeight;

            if (!mapFileTilesetsDict.ContainsKey(mapFilePath))
            {
                var newTileSets = mapFileTilesetsDict[mapFilePath] = new TileSet[mapFile.Tilesets.Count];
                for (int i = 0; i < mapFile.Tilesets.Count; i++)
                {
                    var tmxTileset = mapFile.Tilesets[i];
                    var newTileset = new TileSet
                    {
                        Texture = Assets.Textures[Path.GetFileNameWithoutExtension(tmxTileset.Image.Source)],
                        FirstGid = tmxTileset.FirstGid,
                        LastGid = tmxTileset.FirstGid + (int)tmxTileset.TileCount - 1
                    };
                    newTileSets[i] = newTileset;
                }
            }
            tilesets = mapFileTilesetsDict[mapFilePath];

            var tmxTileLayer = mapFile.TileLayers[tileLayerID];
            for (int i = 0; i < mapFile.Height; i++)
            {
                for (int j = 0; j < mapFile.Width; j++)
                {
                    MapValues[i, j] = tmxTileLayer.Tiles[(i * mapFile.Width) + j].Gid;
                }
            }

            if (getNameFromFile) Name = tmxTileLayer.Name;
            Visible = tmxTileLayer.Visible;
            OpacityOffset = (float)tmxTileLayer.Opacity;
            PositionOffset = new Vector2((float)tmxTileLayer.OffsetX, (float)tmxTileLayer.OffsetY);
            SpawnProperties = tmxTileLayer.Properties;
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

            var cameraBox = Engine.VisibleAreaBox;
            var cameraBoxPosition = new Vector2(cameraBox.X, cameraBox.Y);
            var cameraBoxSize = new Vector2(cameraBox.Width, cameraBox.Height);

            var globalTileSize = new Vector2(TileWidth, TileHeight);
            var tileFrameStart = (cameraBoxPosition - GlobalPosition) / globalTileSize / GlobalScale;
            var tileFrameEnd = cameraBoxSize / globalTileSize / GlobalScale + tileFrameStart;

            var tileFrameStartX = (int)tileFrameStart.X - 1;
            var tileFrameStartY = (int)tileFrameStart.Y - 1;
            var tileFrameEndX = (int)tileFrameEnd.X + 1;
            var tileFrameEndY = (int)tileFrameEnd.Y + 1;

            TileSet currentTileSet = TileSetForGid(MapValues[0, 0]);

            for (int i = tileFrameStartY; i < tileFrameEndY; i++)
            {
                int tileRowIndex;
                if (InfiniteY) tileRowIndex = SignConsciousModulus(i, Height);
                else
                {
                    if (i >= MapValues.Length) break;
                    if (i < 0) continue;
                    tileRowIndex = i;
                }
                for (int j = tileFrameStartX; j < tileFrameEndX; j++)
                {
                    int tileColumnIndex;
                    if (InfiniteX) tileColumnIndex = SignConsciousModulus(j, Width);
                    else
                    {
                        if (j >= MapValues.Length) break;
                        if (j < 0) continue;
                        tileColumnIndex = j;
                    }

                    PositionOffset = new Vector2(j, i) * globalTileSize * GlobalScale + originalPosition;

                    var currentTileGid = MapValues[tileRowIndex, tileColumnIndex];
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

        public class FileLoadedScene<EnumType> where EnumType : Enum
        {
            public readonly TileMap[] TileLayers;
            public readonly List<Entity[]> ObjectGroups = new List<Entity[]>();

            public FileLoadedScene(string mapFilePath, EnumType startingRenderLayer)
            {
                var mapFile = LoadTMXFile(mapFilePath);
                TileLayers = new TileMap[mapFile.TileLayers.Count];
                for (int i = 0; i < TileLayers.Length; i++)
                {
                    var newTileLayer = new TileMap(mapFilePath, i, true);
                    newTileLayer.SetRenderLayer(startingRenderLayer);
                    TileLayers[i] = newTileLayer;
                }

                foreach (var objectGroup in mapFile.ObjectGroups)
                {
                    var objectGroupOffset = new Vector2((float)objectGroup.OffsetX, (float)objectGroup.OffsetY);

                    var newEntityList = new Entity[objectGroup.Objects.Count];
                    ObjectGroups.Add(newEntityList);

                    for (int i = 0; i < newEntityList.Length; i++)
                    {
                        var o = objectGroup.Objects[i];
                        var entityType = Type.GetType(ObjectNamespace + "." + o.Type);

#if DEBUG
                        if (entityType == null || !entityType.IsSubclassOf(typeof(Entity)))
                            throw new Exception("Entity in map file \"" + mapFilePath + "\" had an object labelled as type \"" +
                                o.Type + "\", which is not a valid entity type in the namespace \"" + ObjectNamespace + "\".");
#endif

                        var newEntity = (Entity)Activator.CreateInstance(entityType);

                        newEntity.Name = o.Name;
                        newEntity.Visible = o.Visible;
                        newEntity.PositionOffset = new Vector2((float)o.X, (float)o.Y) + objectGroupOffset;
                        newEntity.AngleOffset = (float)o.Rotation;
                        newEntity.SpawnProperties = o.Properties;
                        newEntityList[i] = newEntity;
                    }
                }
            }

            public Entity[] GetEntities()
            {
                int entityCount = TileLayers.Length;

                foreach (var og in ObjectGroups)
                    entityCount += og.Length;

                var entities = new Entity[entityCount];

                for (int i = 0; i < TileLayers.Length; i++)
                    entities[i] = TileLayers[i];

                foreach (var og in ObjectGroups)
                    for (int i = 0; i < og.Length; i++)
                        entities[i + TileLayers.Length] = og[i];

                return entities;
            }
        }

        public static string ObjectNamespace = "";

        public static FileLoadedScene<EnumType> LoadSceneFromFile<EnumType>(string mapFilePath, EnumType startingRenderLayer) where EnumType : Enum
        {
#if DEBUG
            if (ObjectNamespace == "") throw new Exception("Please set the object namespace.");
#endif

            LoadTMXFile(mapFilePath);
            var fileLoadedScene = new FileLoadedScene<EnumType>(mapFilePath, startingRenderLayer);
            return fileLoadedScene;
        }
    }
}