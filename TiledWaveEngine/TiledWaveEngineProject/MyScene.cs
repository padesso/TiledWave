#region Using Statements
using System;
using TiledSharp;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework.Graphics;
using WaveEngine.Common.Math;
using System.Collections.Generic;
using WaveEngine.Framework.Physics2D;
using TiledWaveEngineProject.TiledWave;
using TiledWaveEngineProject.dbrCamera;
#endregion

namespace TiledWaveEngineProject
{
    public class MyScene : Scene
    {
        protected override void CreateScene()
        {
            RenderManager.BackgroundColor = Color.CornflowerBlue;

            //TODO: get camera and picking working
            //Entity orthogonalCamera = new Entity("orthogonal")
            //                            .AddComponent(new Camera()
            //                            {
            //                                Position = new Vector3(0, 0f, 0f),
            //                                LookAt = new Vector3(0, 55f, 55f),
            //                            })
            //                            .AddComponent(new CameraBehavior());

            //RenderManager.SetActiveCamera(orthogonalCamera);
            //EntityManager.Add(orthogonalCamera); 

            TmxMapRenderer tmxMapRenderer = new TmxMapRenderer(@".\Content\IsoTiledTest.tmx", EntityManager);
            tmxMapRenderer.Render();


        }
    }
}
