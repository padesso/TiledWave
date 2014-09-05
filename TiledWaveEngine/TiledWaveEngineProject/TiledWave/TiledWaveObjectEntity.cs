using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using TiledWaveEngineProject.Helpers;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics2D;

namespace TiledWaveEngineProject.TiledWave
{
    public class TiledWaveObjectEntity : Drawable2D
    {
        TmxMap _map;
        TmxObjectGroup.TmxObject _tiledObject; 

        public TiledWaveObjectEntity(TmxMap map, TmxObjectGroup.TmxObject tiledObject) :
            base()
        {
            _map = map;
            _tiledObject = tiledObject;
        }

        protected override void Initialize()
        {

        }

        public override void Draw(TimeSpan gameTime)
        {
            // Add our shapes to be rendered
            Color col = Color.AntiqueWhite; 

            switch (_tiledObject.ObjectType)
            {               
                case TmxObjectGroup.TmxObjectType.Basic:     //Rectangle
                    RectangleF rect = new RectangleF(_tiledObject.X, _tiledObject.Y, _tiledObject.Width, _tiledObject.Height);
                    RenderManager.LineBatch2D.DrawRectangle(ref rect, ref col);
                    break; 

                case TmxObjectGroup.TmxObjectType.Ellipse:   //Ellipse    
                    //Turn the ellipse into a polygon
                    List<Tuple<int, int>> ellipsePoints = EllipsePolygonCreator.CreateEllipsePoints(0.1, _tiledObject.Width / 2, _tiledObject.Height / 2);
                    //Shift the local ellipse points by the width/height
                    for (int ellipsePointIndex = 0; ellipsePointIndex < ellipsePoints.Count; ellipsePointIndex++ )
                    {
                        ellipsePoints[ellipsePointIndex] = new Tuple<int,int>(
                            ellipsePoints[ellipsePointIndex].Item1 + _tiledObject.Width / 2,
                            ellipsePoints[ellipsePointIndex].Item2 + _tiledObject.Height / 2);
                    }
                    DrawPolygon(ellipsePoints, col);
                    break;

                case TmxObjectGroup.TmxObjectType.Polygon:   //Polygon
                    DrawPolygon(_tiledObject.Points, col);
                    break;

                case TmxObjectGroup.TmxObjectType.Polyline:  //Polyline
                    for (int pointIndex = 0; pointIndex < _tiledObject.Points.Count - 1; pointIndex++)
                    {
                        Vector2 startPos = new Vector2(_tiledObject.X + _tiledObject.Points[pointIndex].Item1, _tiledObject.Y + _tiledObject.Points[pointIndex].Item2);
                        Vector2 endPos = new Vector2(_tiledObject.X + _tiledObject.Points[pointIndex + 1].Item1, _tiledObject.Y + _tiledObject.Points[pointIndex + 1].Item2);
                        RenderManager.LineBatch2D.DrawLine(ref startPos, ref endPos, ref col);
                    }
                    break;

                case TmxObjectGroup.TmxObjectType.Tile:      //Tile
                    //Nothing here because the tiles are loaded in the Initialize method so they
                    //are not continuously added to the EntityManager
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
        }

        private void DrawPolygon(List<Tuple<int,int>> points, Color color)
        {
            for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
            {
                if (pointIndex < points.Count - 1)
                {
                    Vector2 startPos = new Vector2(_tiledObject.X + points[pointIndex].Item1, _tiledObject.Y + points[pointIndex].Item2);
                    Vector2 endPos = new Vector2(_tiledObject.X + points[pointIndex + 1].Item1, _tiledObject.Y + points[pointIndex + 1].Item2);
                    RenderManager.LineBatch2D.DrawLine(ref startPos, ref endPos, ref color);
                }
                else
                {
                    Vector2 startPos = new Vector2(_tiledObject.X + points[pointIndex].Item1, _tiledObject.Y + points[pointIndex].Item2);
                    Vector2 endPos = new Vector2(_tiledObject.X + points[0].Item1, _tiledObject.Y + points[0].Item2);
                    RenderManager.LineBatch2D.DrawLine(ref startPos, ref endPos, ref color);
                }
            }
        }
    }
}
