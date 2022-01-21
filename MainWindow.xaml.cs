using SkiaSharp;
using SkiaSharpTestApp.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkiaSharpTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        class DrawItem
        {
            public float X;
            public float Y;
            public float Radius;
            //public SKColor Color;
            public SKPaint Paint;

            public List<PortDrawItem> Ports = new List<PortDrawItem>(1);

            //public DrawItem(float x, float y, float radius)
            //{
            //    X = x;
            //    Y = y;
            //    Radius = radius;
            //    Ports.Add(new PortDrawItem(x + Radius, y));
            //}

            public void Recalc()
            {
                foreach(var port in Ports)
                {
                    port.Point = new SKPoint(X + Radius, Y);
                }
            }

            public void AddPort()
            {
                //Ports.Add(new PortDrawItem(x + Radius, y));
                Ports.Add(new PortDrawItem());
                Recalc();
            }
        }

        class PortDrawItem
        {
            const float Radius = 5f;
            const float HoverRadius = Radius + 5f;

            //const float RadiusSquared = Radius * Radius;

            public SKPoint Point;
            public bool IsHovered;

            //public PortDrawItem(float x, float y)
            //{
            //    Point = new SKPoint(x, y);
            //}

            public void Draw(SKCanvas canvas)
            {
                canvas.DrawCircle(new SKPoint(Point.X, Point.Y), Radius, _paintDarkGreen);

                if(IsHovered)
                    canvas.DrawCircle(new SKPoint(Point.X, Point.Y), HoverRadius, _paintDarkGreenPen);
            }

            public bool HitTest(SKPoint hitPoint)
            {
                //return (Point-hitPoint).LengthSquared < RadiusSquared;
                return (Point - hitPoint).Length < HoverRadius;
            }

            static SKPaint _paintDarkGreen = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.DarkGreen,
                StrokeCap = SKStrokeCap.Square
            };

            static SKPaint _paintDarkGreenPen = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.DarkGreen,
                StrokeCap = SKStrokeCap.Square,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            };
        } // end class

        class Connection
        {
            public DrawItem FromItem;
            public int FromPortIdx;

            public DrawItem ToItem;
            public int ToPortIdx;

            public bool IsHovered = false;

            public Connection(DrawItem fromItem, int fromPortIdx, DrawItem toItem, int toPortIdx)
            {
                FromItem = fromItem;
                FromPortIdx = fromPortIdx;
                ToItem = toItem;
                ToPortIdx = toPortIdx;
            }

            


            //public ConnectionEnd(DrawItem fromItem, int toPortIdx)
            //{
            //    DrawItem = item;
            //    PortIdx = portIdx;
            //}
        }

        List<Connection> _connectionItems = new List<Connection>();

        List<DrawItem> _drawItems = new List<DrawItem>();
        //List<PortDrawItem> _portItems = new();

        //private readonly DrawingBox _box;
        private SKMatrix _matrix;

        public MainWindow()
        {
            InitializeComponent();
            //_box = new DrawingBox();
            _matrix = SKMatrix.CreateIdentity();

            ctrSkElement.PaintSurface += CtrSkElement_PaintSurface;
            ctrSkElement.MouseMove += CtrSkElement_MouseMove;
            ctrSkElement.MouseWheel += CtrSkElement_MouseWheel;

            //ctrSkElement.MouseLeftButtonDown += CtrSkElement_MouseLeftButtonDown;
            //ctrSkElement.MouseLeftButtonUp += CtrSkElement_MouseLeftButtonUp;
            ctrSkElement.MouseDown += CtrSkElement_MouseDown;
            ctrSkElement.MouseUp += CtrSkElement_MouseUp;

            ctrSkElement.MouseLeave += CtrSkElement_MouseLeave;

            // *** create items for a test
            const int count = 20;
            _drawItems = new List<DrawItem>(count);

            var perRow = (int)Math.Sqrt(count);
            const int diam = 40;
            const int margin = 100;
            const int off = 100;
            for (int i = 0; i < count; i++)
            {
                int row = i / perRow;
                int col = i % perRow;

                _drawItems.Add(new DrawItem { 
                    X = off + col * (diam + margin),
                    Y = off + row * (diam + margin),
                    Radius = diam/2,
                    Paint = _paint
                });
            }

            //_portItems = new List<PortDrawItem>(_drawItems.Count);
            foreach(DrawItem it in _drawItems)
            {
                it.AddPort();
            }

            //ctrSkElement.DragEnter
            //Draw(ctrSkElement);
        }

        private void CtrSkElement_MouseLeave(object sender, MouseEventArgs e)
        {
            Logger.Trace(">>>>>>>>>>>> LEAVE");
            DoHitTest(null);
            ctrSkElement.InvalidateVisual();
        }

        private SKPoint _startMove; //shared for pan and move operation
        private bool _movedMouse;
        private bool _isPanMode = false;
        private bool _isMoveItemMode = false;
        private DrawItem? _moveItem;

        // *** connection drawing when connecting
        private bool _isConnectionDrawMode = false;
        private SKPoint _connectionToPoint;
        private SKPoint _connectionFromPoint;

        private DrawItem? _connectionFromItem = null;
        //private DrawItem? _connectionToItem = null;

        private int _connectionFromPortIdx = -1;
        //private int _connectionToPortIdx = -1;

        // ***

        enum HitType { None, Elem, Port, Conn };
        struct HitResult
        {
            public HitType HitType;
            public DrawItem? Item;
            public int PortIdx;
        }

        private void CtrSkElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SKPath pp;

            var orgPoint = GetWorldPointFromMouseEvent(e);
            var pointWnd = GetPointFromMouseEvent(e);

            //Logger.Trace(">> MouseDown");

            var hitResult = GetHitTestItem(orgPoint);

            if (hitResult.HitType == HitType.Conn)
            {

            }
            else if (hitResult.HitType == HitType.Port)
            {
                // *** CONNECTING
                //if (!_isConnectionDrawMode)
                {
                    Logger.Trace(">> CONNECT - FIRST END");

                    _connectionFromPoint = orgPoint;
                    _connectionToPoint = orgPoint;

                    _connectionFromPortIdx = hitResult.PortIdx;
                    _connectionFromItem = hitResult.Item;

                    _isConnectionDrawMode = true;
                }
   
            }
            else if (hitResult.HitType == HitType.Elem)
            {
                // *** MOVING

                ctrSkElement.CaptureMouse();
                //ctrSkElement.Cursor = Cursors.SizeAll; //delay till first mouse move

                _isMoveItemMode = true;
                _moveItem = hitResult.Item;
                Debug.Assert(_moveItem != null);
                _startMove = new SKPoint(_moveItem.X, _moveItem.Y) - orgPoint; // here as offset in world coord
                _movedMouse = false;
            }
            else
            {
                // **** PANING
                ctrSkElement.CaptureMouse();
                ctrSkElement.Cursor = Cursors.Hand;

                //var pos = e.GetPosition(ctrSkElement);
                //var pointWnd = new SKPoint((float)pos.X, (float)pos.Y);


                _isPanMode = true;
                _startMove = pointWnd;

            }
        }

        private void CtrSkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // *** pan mode
            ctrSkElement.ReleaseMouseCapture();
            ctrSkElement.Cursor = Cursors.Arrow;

            _isPanMode = false;
            _startMove = new SKPoint(0,0); // clear

            // *** move mode
            _isMoveItemMode = false;
            _moveItem = null;

            // *** PORT CONNECTION
            if (_isConnectionDrawMode)
            {
                Logger.Trace(">> CONNECT - SECOND END");

                var orgPoint = GetWorldPointFromMouseEvent(e);
                //var pointWnd = GetPointFromMouseEvent(e);

                var hitResult = GetHitTestItem(orgPoint);
                if (hitResult.HitType == HitType.Port)
                {
                    var connectionToPortIdx = hitResult.PortIdx;
                    var connectionToItem = hitResult.Item;

                    if ( (connectionToItem == _connectionFromItem && connectionToPortIdx == _connectionFromPortIdx) ) 
                    {
                        // A -> A is not allowed
                        SystemSounds.Beep.Play();
                    }
                    else
                    {
                        Debug.Assert(_connectionFromItem != null);
                        Debug.Assert(connectionToItem != null);

                        var ci = new Connection(_connectionFromItem, _connectionFromPortIdx, connectionToItem, connectionToPortIdx);
                        _connectionItems.Add(ci);
                    }
                }

            }

            _isConnectionDrawMode = false;

            ctrSkElement.InvalidateVisual();
        }

        private void HandlePanOnMouseMove(SKPoint pointWnd, SKPoint orgPoint)
        {
            if (_isPanMode)
            {
                var off = (pointWnd - _startMove);
                _matrix = _matrix.PostConcat(SKMatrix.CreateTranslation(off.X, off.Y));
                _startMove = pointWnd;
            }

            if(_isConnectionDrawMode)
            {
                _connectionToPoint = orgPoint;
            }
        }

        private void HandleMoveItemOnMouseMove(SKPoint point, SKPoint orgPoint)
        {
            
            if (_moveItem == null)
                return;

            if (_isMoveItemMode)
            {
                if(!_movedMouse)
                {
                    ctrSkElement.Cursor = Cursors.SizeAll;
                    _movedMouse = true;
                }

                var newPos = orgPoint + _startMove;
                _moveItem.X = newPos.X;
                _moveItem.Y = newPos.Y;
                _moveItem.Recalc();
            }
        }


        private void CtrSkElement_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var pos = e.GetPosition(ctrSkElement);
            var pointWnd = new SKPoint((float)pos.X, (float)pos.Y);

            const float ff = 1.25f;
            var scaleFact = e.Delta > 0 ? (ff * (e.Delta / 120.0f)) : (1.0f/(ff * (-e.Delta / 120.0f)));
            //var scaleFact = e.Delta > 0 ? ff : 1 / ff;
            Zoom(pointWnd, scaleFact);
            ctrSkElement.InvalidateVisual();

        }

        public void Zoom(SKPoint point, float scaleFactor)
        {
            _matrix = _matrix.PostConcat(SKMatrix.CreateScale(scaleFactor, scaleFactor, point.X, point.Y));
        }



        SKPoint _cirCenter = new SKPoint(100, 100);

        SKPoint GetPointFromMouseEvent(MouseEventArgs e)
        {
            var pos = e.GetPosition(ctrSkElement);
            var pointWnd = new SKPoint((float)pos.X, (float)pos.Y);
            return pointWnd;
        }

        SKPoint GetWorldPointFromMouseEvent(MouseEventArgs e)
        {
            var pointWnd = GetPointFromMouseEvent(e);
            return GetWorldPoint(pointWnd);
        }

        private SKPoint GetWorldPoint(SKPoint pointWnd)
        {
            SKMatrix inverse;
            bool inversedOK = _matrix.TryInvert(out inverse); //todo: optimise
            if (!inversedOK)
            {
                Console.Beep();
                Logger.Trace("ERROR: inverse");
            }

            var orgPoint = inverse.MapPoint(pointWnd);
            return orgPoint;
        }

        private void CtrSkElement_MouseMove(object sender, MouseEventArgs e)
        {
            //var pos = e.GetPosition(ctrSkElement);
            //var pointWnd = new SKPoint((float)pos.X, (float)pos.Y);
            ////var point = _matrix.MapPoint(pointWnd);

            //SKMatrix inverse;
            //bool inversedOK = _matrix.TryInvert(out inverse);
            //if (!inversedOK)
            //    Logger.Trace("ERROR: inverse");

            //var orgPoint = inverse.MapPoint(pointWnd);

            var pointWnd = GetPointFromMouseEvent(e);
            var orgPoint = GetWorldPointFromMouseEvent(e);

            ctrLabelXYWnd.Text = $"Wnd(X={pointWnd.X:F1} Y={pointWnd.Y:F1})";
            //ctrLabelXYLogical.Text = $"Map(X={point.X:F2} Y={point.Y:F2})";
            ctrLabelXYOrg.Text = $"Org(X={orgPoint.X:F2} Y={orgPoint.Y:F2})";

            DoHitTest(orgPoint);

            HandlePanOnMouseMove(pointWnd, orgPoint);
            HandleMoveItemOnMouseMove(pointWnd, orgPoint);

            //if (DoHitTest(orgPoint))
            //{

            //}
            //else
            //{

            //}

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Logger.Trace("PRESSED");
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                //Logger.Trace("RELEASED");
            }

            ctrSkElement.InvalidateVisual();
        }

        private bool DoHitTest(SKPoint? point)
        {
            return GetHitTestItem(point).HitType != HitType.None;
        }


        SKPath aLine = new SKPath();
        SKPath aLineRender = new SKPath();

        private HitResult GetHitTestItem(SKPoint? point1)
        {

            //DrawItem[] cc = new DrawItem[2];

            //int hitCount = 0;


            if (point1 == null)
            {
                // *** OUTSIDE VIEW

                foreach (var it in _drawItems)
                {
                    for (int i = 0; i < it.Ports.Count; i++)
                    {
                        PortDrawItem? pp = it.Ports[i];
                        pp.IsHovered = false;
                    }
                }

                foreach (var cc in _connectionItems)
                {
                    cc.IsHovered = false;
                }

                foreach (var it in _drawItems)
                {
                    it.Paint = _paint;
                }

                return new HitResult
                {
                    HitType = HitType.None,
                    Item = null,
                    PortIdx = -1
                };
            }

            

            // *** INSIDE VIEW

            var point = point1.Value;

            DrawItem? hitItem = null;

            HitType hitType = HitType.None;
            int portIdxHit = -1;

            bool found = false;
            // *** fist goes for ports
            foreach (var it in _drawItems)
            {
                for (int i = 0; i < it.Ports.Count; i++)
                {
                    PortDrawItem? pp = it.Ports[i];

                    pp.IsHovered = pp.HitTest(point);

                    if (pp.IsHovered)
                    {
                        found = true;
                        hitType = HitType.Port;
                        portIdxHit = i;
                        hitItem = it;
                        break; // port hit but not element
                    }
                }
            }

            // *** now goes for connections



            //SKPath aLine = new SKPath();
            //SKPath aLineRender = new SKPath();

            foreach (var cc in _connectionItems)
            {
                aLine.Rewind();
                aLineRender.Rewind();

                aLine.MoveTo(cc.FromItem.Ports[cc.FromPortIdx].Point);
                aLine.LineTo(cc.ToItem.Ports[cc.ToPortIdx].Point);

                _paintDarkGreenPen_Hit.GetFillPath(aLine, aLineRender, 0.5f);

                if(!found && aLineRender.Contains(point.X, point.Y))
                {
                    //SystemSounds.Beep.Play();
                    cc.IsHovered = true;
                    found = true;
                    hitType = HitType.Conn;
                }
                else
                {
                    cc.IsHovered = false;
                }
            }


            // *** now goes for nodes
            foreach (var it in _drawItems)
            {

                
                var vv = point - new SKPoint(it.X, it.Y);
                if(!found && vv.Length <= it.Radius)
                {
                    it.Paint = _paintHighlight;
                    //hitCount++;
                    hitItem = it;
                    hitType = HitType.Elem;
                    found = true;
                    //break; // todo: handle many items
                }
                else
                {
                    it.Paint = _paint;
                }
            }

            return new HitResult { 
                HitType = hitType,
                Item = hitItem,
                PortIdx = portIdxHit
            };
        }

        static SKColor _color = new SKColor(0x2c, 0x3e, 0x50);

        SKPaint _paint = new SKPaint
        {
                IsAntialias = true,
                Color = _color, //new SKColor(0x2c, 0x3e, 0x50),
                StrokeCap = SKStrokeCap.Round
        };

        SKPaint _paintHighlight = new SKPaint
        {
            IsAntialias = true,
            Color = ColorTools.ShadeRGBColor(_color, 0.1f), // SKColors.BlueViolet,
            StrokeCap = SKStrokeCap.Round
        };

        SKPaint _paintRedCross = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Red,
            StrokeCap = SKStrokeCap.Square
        };

        SKPaint _paintDarkGreen = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.DarkGreen,
            StrokeCap = SKStrokeCap.Square
        };

        private static readonly SKColor darkGreen = SKColors.DarkGreen;

        SKPaint _paintDarkGreenPen = new SKPaint
        {
            IsAntialias = true,
            Color = darkGreen,
            StrokeCap = SKStrokeCap.Square,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f
        };

        SKPaint _paintDarkGreenPen_Hover = new SKPaint
        {
            IsAntialias = true,
            Color = ColorTools.ShadeRGBColor(darkGreen, +0.1f),
            StrokeCap = SKStrokeCap.Square,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3f
        };

        SKPaint _paintDarkGreenPen_Hit = new SKPaint
        {
            IsAntialias = true,
            Color = ColorTools.ShadeRGBColor(darkGreen, +0.1f),
            StrokeCap = SKStrokeCap.Square,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5f
        };

        private void CtrSkElement_PaintSurface(object? sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            canvas.SetMatrix(_matrix);

            var color = new SKColor(0x22, 0x22, 0x22, (byte)(255 * 1.0));
            canvas.Clear(color);

            //canvas.DrawCircle(new SKPoint(100, 100), 100, _paint);
            //// paint red cross "+"
            //canvas.DrawLine(new SKPoint(100, 100) + new SKPoint(-10, 0), new SKPoint(100, 100) + new SKPoint(+10, 0), _paintRedCross);
            //canvas.DrawLine(new SKPoint(100, 100) + new SKPoint(0, -10), new SKPoint(100, 100) + new SKPoint(0, +10), _paintRedCross);

            foreach(var it in _drawItems)
            {
                canvas.DrawCircle(new SKPoint(it.X, it.Y), it.Radius, it.Paint);
            }

            // *** draw connections
            foreach(var cc in _connectionItems)
            {
                var from = cc.FromItem.Ports[cc.FromPortIdx].Point;
                var to = cc.ToItem.Ports[cc.ToPortIdx].Point;

                if(cc.IsHovered)
                    canvas.DrawLine(from, to, _paintDarkGreenPen_Hover);
                else
                    canvas.DrawLine(from, to, _paintDarkGreenPen);
            }

            // *** draw ports

            //foreach (var it in _drawItems)
            //{
            //    canvas.DrawCircle(new SKPoint(it.X + it.Radius, it.Y), it.Radius/4.0f, _paintDarkGreen);

            //    canvas.DrawCircle(new SKPoint(it.X + it.Radius, it.Y), it.Radius / 4.0f + 5.0f, _paintDarkGreenPen);
            //}

            foreach(var it in _drawItems)
            {
                foreach(var pp in it.Ports)
                {
                    pp.Draw(canvas);
                }
            }

            if(_isConnectionDrawMode)
            {
                canvas.DrawLine(_connectionFromPoint, _connectionToPoint, _paintDarkGreenPen);
            }
        }

        private void btnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            //_box.XTranslate += 50;
            _matrix.TransX += 50;
            ctrSkElement.InvalidateVisual();
        }

        private void btnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            //_box.XTranslate -= 50;
            _matrix.TransX -= 50;
            ctrSkElement.InvalidateVisual();
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            //_box.YTranslate += 50;
            _matrix.TransY += 50;
            ctrSkElement.InvalidateVisual();
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            //_box.YTranslate -= 50;
            _matrix.TransY -= 50;
            ctrSkElement.InvalidateVisual();
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            //_paint.Color = SKColors.CadetBlue;
            _color = SKColors.CadetBlue;
            _paint.Color = _color;
            ctrSkElement.InvalidateVisual();
        }

        

        //see: https://stackoverflow.com/questions/61417583/skiasharp-make-a-bitmap-on-a-path-clickable
        // canvas.TotalMatrix ??
        //public bool HitTest(SKMatrix Matrix, SKPoint location)
        //{
        //    // Invert the matrix
        //    SKMatrix inverseMatrix;

        //    if (Matrix.TryInvert(out inverseMatrix))
        //    {
        //        // Transform the point using the inverted matrix
        //        SKPoint transformedPoint = inverseMatrix.MapPoint(location);

        //        // Check if it's in the untransformed bitmap rectangle
        //        SKRect rect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
        //        return rect.Contains(transformedPoint);
        //    }
        //    return false;
        //}
    }
}




