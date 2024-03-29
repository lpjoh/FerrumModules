﻿using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Crossfrog.Ferrum.Engine.Modules;
using Crossfrog.Ferrum.Engine.Physics;

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

        public bool CollisionActive = false;

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

        private Collider Collider;
        private readonly HashSet<Tuple<int, int>> ExistingColliderTiles = new HashSet<Tuple<int, int>>();
        private static Vector2 colliderCenter = new Vector2(0.5f, 0.5f);
        

        private void CreateHitbox()
        {
            Collider.AddChild(new CollisionShape().SetAsBox(1, 1));
        }
        public override void Init()
        {
            base.Init();
            Collider = AddChild(new StaticBody());
        }
        public override void PreCollision()
        {
            base.PreCollision();
            if (!CollisionActive) return;

            var hitboxCount = 0;
            Collider.ScaleOffset = TileSize;
            var shapesBefore = Scene.PhysicsWorld.Count;
            for (int s = 0; s < shapesBefore; s++)
            {
                var shape = Scene.PhysicsWorld[s];
                if (shape.Parent == Collider || typeof(StaticBody).IsAssignableFrom(shape.Parent?.GetType()))
                    continue;

                var shapeBox = shape.BoundingBox;

                var tileScale = TileSize * GlobalScale;
                var boxStart = (new Vector2(shapeBox.X - 1, shapeBox.Y - 1) - GlobalPosition) / tileScale;
                var boxEnd = boxStart + new Vector2(shapeBox.Width + 2, shapeBox.Height + 2) / tileScale;

                var rowStart = (int)boxStart.Y - 1;
                var rowEnd = (int)boxEnd.Y + 2;
                var columnStart = (int)boxStart.X - 1;
                var columnEnd = (int)boxEnd.X + 2;

                for (int i = rowStart; i < rowEnd; i++)
                {
                    int tileRowIndex;
                    if (i >= Height) break;
                    if (i < 0) continue;
                    tileRowIndex = i;

                    CollisionShape workingHitbox = null;
                    for (int j = columnStart; j < columnEnd; j++)
                    {
                        int tileColumnIndex;

                        if (j >= Width) break;
                        if (j < 0) continue;
                        tileColumnIndex = j;

                        var tilePositionTuple = new Tuple<int, int>(tileRowIndex, tileColumnIndex);
                        if (MapValues[tileRowIndex, tileColumnIndex] > 0 && (!ExistingColliderTiles.Contains(tilePositionTuple)))
                        {
                            ExistingColliderTiles.Add(tilePositionTuple);
                            if (workingHitbox == null)
                            {
                                hitboxCount++;
                                if (hitboxCount > Collider.CollisionShapes.Count)
                                    CreateHitbox();

                                workingHitbox = Collider.CollisionShapes[hitboxCount - 1];
                                workingHitbox.PositionOffset = new Vector2(tileColumnIndex, tileRowIndex) + colliderCenter;
                                workingHitbox.ScaleOffset.X = 1;
                            }
                            else
                            {
                                workingHitbox.ScaleOffset.X += 1;
                                workingHitbox.PositionOffset.X += 1 - colliderCenter.X;
                            }
                        }
                        else
                        {
                            workingHitbox = null;
                        }
                    }
                }
            }

            for (int i = hitboxCount; i < Collider.CollisionShapes.Count; i++)
                Collider.CollisionShapes[i].Exit();
            ExistingColliderTiles.Clear();
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
                        Texture = Assets.GetTexture(Path.GetFileNameWithoutExtension(tmxTileset.Image.Source)),
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
        public void SetCameraBounds()
        {
            var camera = Scene.Camera;
            camera.ScrollClampStart = GlobalPosition;
            camera.ScrollClampEnd = camera.ScrollClampStart + TileSize * new Vector2(Width, Height) * GlobalScale;
        }
        public override void Render(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            Vector2 originalPosition = PositionOffset;

            var cameraBox = Engine.VisibleAreaBox;
            var cameraBoxPosition = new Vector2(cameraBox.X, cameraBox.Y);
            var cameraBoxSize = new Vector2(cameraBox.Width, cameraBox.Height);

            var tileFrameStart = (cameraBoxPosition - GlobalPosition) / TileSize / GlobalScale;
            var tileFrameEnd = cameraBoxSize / TileSize / GlobalScale + tileFrameStart;

            var tileFrameStartX = (int)tileFrameStart.X - 1;
            var tileFrameStartY = (int)tileFrameStart.Y - 1;
            var tileFrameEndX = (int)tileFrameEnd.X + 2;
            var tileFrameEndY = (int)tileFrameEnd.Y + 2;

            TileSet currentTileSet = TileSetForGid(MapValues[0, 0]);

            for (int i = tileFrameStartY; i < tileFrameEndY; i++)
            {
                int tileRowIndex;
                if (InfiniteY)
                    tileRowIndex = Misc.SignConsciousModulus(i, Height);
                else
                {
                    if (i >= Height) break;
                    if (i < 0) continue;
                    tileRowIndex = i;
                }
                for (int j = tileFrameStartX; j < tileFrameEndX; j++)
                {
                    int tileColumnIndex;
                    if (InfiniteX)
                        tileColumnIndex = Misc.SignConsciousModulus(j, Width);
                    else
                    {
                        if (j >= Width) break;
                        if (j < 0) continue;
                        tileColumnIndex = j;
                    }

                    PositionOffset = new Vector2(j, i) * TileSize * GlobalScale + originalPosition;

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