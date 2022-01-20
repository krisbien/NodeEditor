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

            const int count = 10000;
            _drawItems = new List<DrawItem>(count);

            var perRow = (int)Math.Sqrt(count);
            const int size = 40;
            const int margin = 10;
            for (int i = 0; i < count; i++)
            {
                int row = i / perRow;
                int col = i % perRow;

                _drawItems.Add(new DrawItem { 
                    X = col * (size + margin),
                    Y = row * (size + margin),
                    Radius = size/2,
                    Paint = _paint
                });
            }
            

            //ctrSkElement.DragEnter
            //Draw(ctrSkElement);
        }


        private SKPoint _startMove;
        private bool isPanMode = false;

        private void CtrSkElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Logger.Trace(">> MouseDown");
            
            ctrSkElement.CaptureMouse();
            ctrSkElement.Cursor = Cursors.Hand;

            var pos = e.GetPosition(ctrSkElement);
            var pointWnd = new SKPoint((float)pos.X, (float)pos.Y);
            //var point = _matrix.MapPoint(pointWnd.X, pointWnd.Y);

            isPanMode = true;
            _startMove = pointWnd;
        }

        private void CtrSkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ctrSkElement.ReleaseMouseCapture();
            ctrSkElement.Cursor = Cursors.Arrow;

            isPanMode = false;
            _startMove = new SKPoint(0,0); // clear
        }

        private void HandlePanOnMouseMove(SKPoint point)
        {
            if (isPanMode)
            {
                var off = (point - _startMove);
                _matrix = _matrix.PostConcat(SKMatrix.CreateTranslation(off.X, off.Y));
                _startMove = point;
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

        private void CtrSkElement_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(ctrSkElement);
            var pointWnd = new SKPoint((float)pos.X, (float)pos.Y);
            //var point = _matrix.MapPoint(pointWnd);

            SKMatrix inverse;
            bool inversedOK = _matrix.TryInvert(out inverse);
            if (!inversedOK)
                Logger.Trace("ERROR: inverse");

            var orgPoint = inverse.MapPoint(pointWnd);

            ctrLabelXYWnd.Text = $"Wnd(X={pos.X:F1} Y={pos.Y:F1})";
            //ctrLabelXYLogical.Text = $"Map(X={point.X:F2} Y={point.Y:F2})";
            ctrLabelXYOrg.Text = $"Org(X={orgPoint.X:F2} Y={orgPoint.Y:F2})";



            //DoHitTest1(point);
            DoHitTest(orgPoint);
            HandlePanOnMouseMove(pointWnd);

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



        private void DoHitTest(SKPoint point)
        {
            //DrawItem[] cc = new DrawItem[2];

            foreach(var it in _drawItems)
            //for (int i = 0; i < _drawItems.Count; i++)
            {
                //ref DrawItem it = ref _drawItems[i];
                var vv = point - new SKPoint(it.X, it.Y);
                if(vv.Length <= it.Radius)
                {
                    it.Paint = _paintHighlight;
                }
                else
                {
                    it.Paint = _paint;
                }
            }
        }

        private void DoHitTest2(SKPoint point)
        {
            //var pp = _matrix.MapPoint(_cirCenter);
            //pp = _matrix.MapPoint(pp);

            //Logger.Trace($"px={pp.X}, py={pp.Y}");

            var vv = point - _cirCenter;

            //Logger.Trace($"len={vv.Length}");

            var orgColor = _paint.Color;

            if (vv.Length <= 100)
            {
                _paint.Color = SKColors.BlueViolet;
            }
            else
            {
                _paint.Color = _color;
            }

            //if (orgColor != _paint.Color)
            //    ctrSkElement.InvalidateVisual();
        }

        private void DoHitTest1(SKPoint point)
        {
            var pp = _matrix.MapPoint(_cirCenter);
            pp = _matrix.MapPoint(pp);

            //Logger.Trace($"px={pp.X}, py={pp.Y}");

            var vv = point - pp;

            //Logger.Trace($"len={vv.Length}");

            var orgColor = _paint.Color;

            if (vv.Length <= 100)
            {
                _paint.Color = SKColors.BlueViolet;
            }
            else
            {
                _paint.Color = _color;
            }

            if (orgColor != _paint.Color)
                ctrSkElement.InvalidateVisual();
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




