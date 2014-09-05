using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;

namespace TiledWaveEngineProject.TiledWave
{
    public class TiledWaveImageLayerComponent
    {
        TmxMap _map;
        EntityManager _entityManager;

        public TiledWaveImageLayerComponent(TmxMap map, EntityManager entityManager)
            :base()
        {
            _map = map;
            _entityManager = entityManager;
        }

        public void DrawBackgrounds()
        {
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
        }
    }
}
