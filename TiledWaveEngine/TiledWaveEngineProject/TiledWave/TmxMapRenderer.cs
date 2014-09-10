using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;
using WaveEngine.Framework.Physics2D;

namespace TiledWaveEngineProject.TiledWave
{
    public class TmxMapRenderer
    {
        string _path;
        TmxMap _map;
        EntityManager _entityManager;

        public TmxMapRenderer(string path, EntityManager entityManager)
        {
            _path = path;
            _map = new TmxMap(_path);
            _entityManager = entityManager;
        }

        //TODO: respect the overall layer order
        public void Render()
        {
            #region Object Layers

            for (int objectGroupIndex = _map.ObjectGroups.Count - 1; objectGroupIndex >= 0; objectGroupIndex--)
            {
                for (int objectIndex = _map.ObjectGroups[objectGroupIndex].Objects.Count - 1; objectIndex >= 0; objectIndex--)
                {
                    TmxObjectGroup.TmxObject _tiledObject = _map.ObjectGroups[objectGroupIndex].Objects[objectIndex];

                    //Add the tiles
                    if (_tiledObject.ObjectType == TmxObjectGroup.TmxObjectType.Tile)
                    {
                        for (int tileSetIndex = 0; tileSetIndex < _map.Tilesets.Count; tileSetIndex++)
                        {
                            TmxTileset tileSet = _map.Tilesets[tileSetIndex];

                            int numXTiles = (int)tileSet.Image.Width / tileSet.TileWidth;
                            int numYTiles = (int)tileSet.Image.Height / tileSet.TileHeight;

                            //Get the tile to draw
                            TmxLayerTile currentTile = _tiledObject.Tile;
                            int currentTileID = currentTile.Gid;

                            //if there's nothing to draw in this tile or our rect is in a later tileset, then skip the loop
                            if (currentTileID <= 0 || currentTileID > (tileSet.FirstGid + numXTiles * numYTiles))
                                continue;

                            //Determine where to crop the texture compensating for FirstGid of each tileSet                        
                            int rectangleX = (currentTileID - tileSet.FirstGid) % numYTiles * tileSet.TileWidth;
                            int rectangleY = ((currentTileID - tileSet.FirstGid) - ((currentTileID - tileSet.FirstGid) % numYTiles)) / numYTiles * tileSet.TileHeight;

                            //Get the cropped sprite for this tile
                            Sprite tileSetSprite = new Sprite(tileSet.Image.Source);
                            tileSetSprite.SourceRectangle = new Rectangle(rectangleX, rectangleY, tileSet.TileWidth, tileSet.TileHeight);

                            //Used for both position and collision
                            Transform2D rectTransform = new Transform2D()
                            {
                                X = _tiledObject.Tile.X, //Something doesn't seem right here that I only need to subtract the height...
                                Y = _tiledObject.Tile.Y - tileSet.TileHeight
                            };

                            //Create the renderable entity
                            var currentEntity = new Entity()
                                .AddComponent(tileSetSprite)
                                .AddComponent(new RectangleCollider() //adding collision allows for off-screen culling as well
                                {
                                    Transform2D = rectTransform
                                })
                                .AddComponent(new SpriteRenderer(DefaultLayers.Alpha))
                                .AddComponent(rectTransform);

                            _entityManager.Add(currentEntity);
                        }                        
                    }
                    else
                    {
                        //Add the tiled objects except for the tiles
                        Entity tiledWaveObjEntity = new Entity()
                            .AddComponent(new TiledWaveObjectEntity(_map, _tiledObject));

                        _entityManager.Add(tiledWaveObjEntity);
                    }
                }
            }

            #endregion

            #region Tile Layers

            //Tileset for user in drawing tiles on tile layers
            for (int layerIndex = _map.Layers.Count - 1; layerIndex >= 0; layerIndex--)
            {
                //Loop through the tiles and add them to the entity manager to be rendered
                for (int tileIndex = 0; tileIndex < _map.Layers[layerIndex].Tiles.Count; tileIndex++)
                {   
                    //Get the clipped sprite from the tile sets
                    Sprite tileSetSprite = GetClippedSprite(layerIndex, tileIndex);
                    if (tileSetSprite == null)
                        continue;

                    //Create an entity to render at the proper location
                    Entity tileEntity = GetTileEntity(layerIndex, tileIndex);
                    if (tileEntity == null)
                        continue;

                    //Add the clipped sprite to the entity
                    tileEntity.AddComponent(tileSetSprite);

                    //Add the entity to the scene
                    _entityManager.Add(tileEntity);               
                }
            }

            #endregion

            #region Image Layers

            for (int layerNum = _map.ImageLayers.Count - 1; layerNum > -1; layerNum--)
            {
                TmxImageLayer imageLayer = _map.ImageLayers[layerNum];

                Sprite imageLayerSprite = new Sprite(_map.ImageLayers[layerNum].Image.Source);

                var imageLayerEntity = new Entity(_map.ImageLayers[layerNum].Name)
                   .AddComponent(imageLayerSprite)
                   .AddComponent(new SpriteRenderer(DefaultLayers.Alpha))
                   .AddComponent(new Transform2D());

                _entityManager.Add(imageLayerEntity);
            }

            #endregion
        }

        #region Helpers

        private Sprite GetClippedSprite(int LayerIndex, int TileIndex)
        {
            for (int tileSetIndex = 0; tileSetIndex < _map.Tilesets.Count; tileSetIndex++)
            {
                TmxTileset tileSet = _map.Tilesets[tileSetIndex];

                //Get the tile to draw
                TmxLayerTile currentTile = _map.Layers[LayerIndex].Tiles[TileIndex];
                int currentTileGID = currentTile.Gid;

                int numXTiles = (int)tileSet.Image.Width / tileSet.TileWidth;
                int numYTiles = (int)tileSet.Image.Height / tileSet.TileHeight;

                //if there's nothing to draw in this tile or our rect is in a later tileset, then skip the loop
                if (currentTileGID <= 0 || currentTileGID > (tileSet.FirstGid + numXTiles * numYTiles))
                    continue;

                //Determine where to crop the texture compensating for FirstGid of each tileSet                        
                int rectangleX = (currentTileGID - tileSet.FirstGid) % numYTiles * tileSet.TileWidth;
                int rectangleY = ((currentTileGID - tileSet.FirstGid) - ((currentTileGID - tileSet.FirstGid) % numYTiles)) / numYTiles * tileSet.TileHeight;

                //Get the cropped sprite for this tile
                Sprite tileSetSprite = new Sprite(tileSet.Image.Source);
                tileSetSprite.SourceRectangle = new Rectangle(rectangleX, rectangleY, tileSet.TileWidth, tileSet.TileHeight);

                return tileSetSprite;
            }

            return null;
        }

        
        private Entity GetTileEntity(int LayerIndex, int TileIndex)
        {
            Transform2D rectTransform = new Transform2D();

            //TODO: handle ortho, iso and staggered
            if (_map.Orientation == TmxMap.OrientationType.Orthogonal)
            {
                rectTransform = new Transform2D()
                {
                    X = _map.Layers[LayerIndex].Tiles[TileIndex].X * _map.TileWidth,
                    Y = _map.Layers[LayerIndex].Tiles[TileIndex].Y * _map.TileHeight
                };    
            }
            else if (_map.Orientation == TmxMap.OrientationType.Isometric)
            {
                //TODO: transform this from top moving down and right
                rectTransform = new Transform2D()
                {
                    X = _map.Layers[LayerIndex].Tiles[TileIndex].X * _map.TileWidth,
                    Y = _map.Layers[LayerIndex].Tiles[TileIndex].Y * _map.TileHeight
                }; 
            }
            else if (_map.Orientation == TmxMap.OrientationType.Staggered)
            {
                //TODO: transform this from top moving down and right, then back left
                rectTransform = new Transform2D()
                {
                    X = _map.Layers[LayerIndex].Tiles[TileIndex].X * _map.TileWidth,
                    Y = _map.Layers[LayerIndex].Tiles[TileIndex].Y * _map.TileHeight
                }; 
            }
            else
            {
                //How did we get here?
                throw new NotImplementedException();
            }

            //Create the renerable entity
            Entity currentEntity = new Entity()
                .AddComponent(new RectangleCollider() //adding collision allows for off-screen culling as well
                {
                    Transform2D = rectTransform
                })
                .AddComponent(new SpriteRenderer(DefaultLayers.Alpha))
                .AddComponent(rectTransform);

            return currentEntity;
        }
        #endregion
    }
}
