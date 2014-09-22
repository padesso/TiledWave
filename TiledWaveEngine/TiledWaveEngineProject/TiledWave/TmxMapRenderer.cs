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
            //for (int layerIndex = 0; layerIndex < _map.Layers.Count; layerIndex++)
            for (int layerIndex = _map.Layers.Count - 1; layerIndex >= 0; layerIndex--)
            {
                if (!_map.Layers[layerIndex].Visible)
                    continue;

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

        //TODO: update this for margin, etc
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

                int sheetIndex = currentTileGID - tileSet.FirstGid;

                //Determine where to crop the texture compensating for FirstGid of each tileSet                        
                int rectangleX = (sheetIndex % numXTiles); //OK
                int rectangleY = ((sheetIndex - rectangleX) / numXTiles);

                //Get the cropped sprite for this tile
                Sprite tileSetSprite = new Sprite(tileSet.Image.Source);
                tileSetSprite.SourceRectangle = new Rectangle(rectangleX * tileSet.TileWidth, rectangleY * tileSet.TileHeight, tileSet.TileWidth, tileSet.TileHeight);

                return tileSetSprite;
            }

            return null;
        }
      
        private Entity GetTileEntity(int LayerIndex, int TileIndex)
        {
            Transform2D rectTransform = new Transform2D();
            
            if (_map.Orientation == TmxMap.OrientationType.Orthogonal) //Ortho is easy
            {
                rectTransform = new Transform2D()
                {
                    X = _map.Layers[LayerIndex].Tiles[TileIndex].X * _map.TileWidth,
                    Y = _map.Layers[LayerIndex].Tiles[TileIndex].Y * _map.TileHeight
                };    
            }
            else if (_map.Orientation == TmxMap.OrientationType.Isometric)
            {
                rectTransform = new Transform2D()
                {
                    //TODO: Verify positions are correct screen versus iso and draw order
                    X = ((_map.Layers[LayerIndex].Tiles[TileIndex].X - _map.Layers[LayerIndex].Tiles[TileIndex].Y) * _map.TileWidth / 2) + ((_map.Width * _map.TileWidth) / 2),
                    Y = ((_map.Layers[LayerIndex].Tiles[TileIndex].X + _map.Layers[LayerIndex].Tiles[TileIndex].Y) * _map.TileHeight / 2)
                };                
            }
            else if (_map.Orientation == TmxMap.OrientationType.Staggered)
            {
                //TODO: Verify positions are correct screen versus iso and draw order
                int x = 0;
                int y = 0;

                if (_map.Layers[LayerIndex].Tiles[TileIndex].Y % 2 == 0) //even rows
                {
                    x = _map.Layers[LayerIndex].Tiles[TileIndex].X * _map.TileWidth;
                }
                else //odd rows
                {
                    x = _map.Layers[LayerIndex].Tiles[TileIndex].X * _map.TileWidth + (_map.TileWidth / 2); //shift row right by 1/2 tile width                    
                }
                
                rectTransform = new Transform2D()
                {

                    X = x,
                    Y = _map.Layers[LayerIndex].Tiles[TileIndex].Y * _map.TileHeight - (_map.Layers[LayerIndex].Tiles[TileIndex].Y * (_map.TileHeight / 2))
                };  
            }
            else
            {
                //How did we get here?
                throw new NotImplementedException();
            }

            rectTransform.DrawOrder = GetTileDrawOrder(LayerIndex, TileIndex);

            //Create the renderable entity
            Entity currentEntity = new Entity()
                .AddComponent(new RectangleCollider() //adding collision allows for off-screen culling as well
                {
                    Transform2D = rectTransform
                })
                .AddComponent(new SpriteRenderer(DefaultLayers.Alpha))
                .AddComponent(rectTransform);

            return currentEntity;
        }

        //TODO: Figure out why some tiles are missing with multiple layers
        private float GetTileDrawOrder(int LayerIndex, int TileIndex)
        {
            //Determine the draw order of the current sprite for all layers
            float ceiling = (float)LayerIndex + 1.0f / (float)_map.Layers.Count;
            float floor = (float)LayerIndex / (float)_map.Layers.Count;

            return 1.0f - (floor + ((float)TileIndex / (float)_map.Layers[LayerIndex].Tiles.Count * (ceiling - floor)));
        }

        #endregion
    }
}
