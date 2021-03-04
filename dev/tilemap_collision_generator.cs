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
                            boxPosition = new Vector2(x, y) * new Vector2(TileWidth, TileHeight) * ScaleOffset + PositionOffset;
                        }
                        
                        consecutiveTiles++;
                    }
                    if ((!IsTileSolid(x, y) || x == mapValues[0].Count - 1) && consecutiveTiles != 0)
                    {
                        var boxScale = new Vector2(consecutiveTiles * TileWidth * ScaleOffset.X, TileHeight * ScaleOffset.Y) / 2;
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