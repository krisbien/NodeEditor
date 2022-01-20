using SkiaSharp;
using SkiaSharpTestApp.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        }

        List<DrawItem> _drawItems = new List<DrawItem>();

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

            const int count = 10;
            _drawItems = new List<DrawItem>(count);

            var perRow = (int)Math.Sqrt(count);
            const int size = 40;
            const int margin = 100;
            const int off = 100;
            for (int i = 0; i < count; i++)
            {
                int row = i / perRow;
                int col = i % perRow;

                _drawItems.Add(new DrawItem { 
                    X = off + col * (size + margin),
                    Y = off + row * (size + margin),
                    Radius = size/2,
                    Paint = _paint
                });
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

        private void CtrSkElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var orgPoint = GetWorldPointFromMouseEvent(e);
            var pointWnd = GetPointFromMouseEvent(e);

            Logger.Trace(">> MouseDown");

            var hitItem = GetHitTestItem(orgPoint);

            if (hitItem != null)
            {
                ctrSkElement.CaptureMouse();
                //ctrSkElement.Cursor = Cursors.SizeAll; //delay till first mouse move

                _isMoveItemMode = true;
                _moveItem = hitItem;
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
            ctrSkElement.ReleaseMouseCapture();
            ctrSkElement.Cursor = Cursors.Arrow;

            _isPanMode = false;
            _startMove = new SKPoint(0,0); // clear

            // *** move mode
            _isMoveItemMode = false;
            _moveItem = null;
        }

        private void HandlePanOnMouseMove(SKPoint point)
        {
            if (_isPanMode)
            {
                var off = (point - _startMove);
                _matrix = _matrix.PostConcat(SKMatrix.CreateTranslation(off.X, off.Y));
                _startMove = point;
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

            HandlePanOnMouseMove(pointWnd);
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
            return GetHitTestItem(point) != null;
        }


        private DrawItem? GetHitTestItem(SKPoint? point)
        {
            //DrawItem[] cc = new DrawItem[2];

            int hitCount = 0;
            DrawItem hitItem = null;

            foreach (var it in _drawItems)
            //for (int i = 0; i < _drawItems.Count; i++)
            {
                //ref DrawItem it = ref _drawItems[i];

                if(point == null)
                {
                    it.Paint = _paint;
                    continue;
                }

                
                var vv = point.Value - new SKPoint(it.X, it.Y);
                if(vv.Length <= it.Radius)
                {
                    it.Paint = _paintHighlight;
                    hitCount++;
                    hitItem = it;
                    break; // todo: handle many items
                }
                else
                {
                    it.Paint = _paint;
                }
            }

            return hitItem;
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
            Color = SKColors.BlueViolet,
            StrokeCap = SKStrokeCap.Round
        };

        SKPaint _paintRedCross = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Red,
            StrokeCap = SKStrokeCap.Square
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




