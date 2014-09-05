#region Using Statements
using System;
using TiledSharp;
using NTiled;
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
#endregion

namespace TiledWaveEngineProject
{
    public class MyScene : Scene
    {
        protected override void CreateScene()
        {
            RenderManager.BackgroundColor = Color.CornflowerBlue;

            TmxMapRenderer tmxMapRenderer = new TmxMapRenderer(@".\Content\TiledTest.tmx", EntityManager);
            tmxMapRenderer.Render();
        }
    }
}
