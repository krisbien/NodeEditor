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
        public enum NodeItemShape { Rect, Circle }
        public class NodeItemType
        {
            public string Name;
            public SKColor BkColor;
            public string? TextAsIcon;
            public float Size;
            public NodeItemShape Shape;
        }

        List<NodeItemType> _nodeTypes = new List<NodeItemType>(3);

        public int AddNodeType(NodeItemType nodeType)
        {
            _nodeTypes.Add(nodeType);
            return _nodeTypes.Count - 1;
        }

        public sealed class NodeItem
        {
            private static int IdSeq = 1;
            public static int GetNextId() => IdSeq++;

            public float X;
            public float Y;
            //public float Radius;
            public SKRect Bounds;

            public bool IsHovered;

            //public int NodeItemTypeIdx;

            public NodeItemType NodeItemType;

            public NodeItem(NodeItemType nodeItemType, string name = null)
            {
                NodeItemType = nodeItemType;
                AddPort();
                if (name == null)
                    Name = $"{nodeItemType.Name} {GetNextId()}";
                else
                    Name = name;
            }

            //public SKColor Color;

            //public SKPaint Paint;

            public List<PortItem> Ports = new List<PortItem>(1);

            //public DrawItem(float x, float y, float radius)
            //{
            //    X = x;
            //    Y = y;
            //    Radius = radius;
            //    Ports.Add(new PortDrawItem(x + Radius, y));
            //}

            public void Recalc()
            {
                var color = NodeItemType.BkColor;
                _paint.Color = color;
                _paintHighlight.Color = ColorTools.ShadeRGBColor(color, 0.1f);
                _shape = NodeItemType.Shape;
                _size_2 = NodeItemType.Size / 2;
                _size = NodeItemType.Size;
                _textAsIcon = NodeItemType.TextAsIcon;
                _textAsIcon_height = GetTextHeight(_textAsIcon, _paint_icon);

                Bounds = new SKRect(X - _size_2, Y - _size_2, X + _size_2, Y + _size_2);
                foreach (var port in Ports)
                {
                    //port.Point = new SKPoint(X + Radius, Y);
                    port.Point = new SKPoint(X + _size_2, Y);
                }
            }

            public void AddPort()
            {
                //Ports.Add(new PortDrawItem(x + Radius, y));
                Ports.Add(new PortItem());
                //Recalc();
            }

            //const float wh2 = 20;
            //const float wh = wh2*2;

            internal void Draw(SKCanvas canvas)
            {
                //canvas.DrawCircle(new SKPoint(X, Y), Radius, IsHovered ? _paintHighlight : _paint);

                var pp = IsHovered ?  _paintHighlight : _paint;
                //canvas.DrawRect(X-wh2, Y-wh2, wh, wh, pp);

                if (_shape == NodeItemShape.Rect)
                {
                    const float rr = 5f;
                    //canvas.DrawRoundRect(X - wh2, Y - wh2, wh, wh, rr, rr, pp);
                    canvas.DrawRoundRect(X - _size_2, Y - _size_2, _size, _size, rr, rr, pp);
                }
                else
                {
                    canvas.DrawCircle(new SKPoint(X, Y), _size_2, IsHovered ? _paintHighlight : _paint);
                }

                //float hh = GetTextHeight(_textAsIcon, _paint_icon);
                //canvas.DrawText("S", new SKPoint(Bounds.MidX, Bounds.MidY+hh/2f), _paint_icon);

                SKPath pt = _paint_icon.GetTextPath(_textAsIcon, 0, 0);
                pt.Transform(SKMatrix.CreateTranslation(Bounds.MidX - pt.Bounds.MidX, Bounds.MidY + _textAsIcon_height / 2f));
                canvas.DrawPath(pt, _paint_icon);

                //pa.get
            }

            //static SKColor _color = new SKColor(0x2c, 0x3e, 0x50);
            static SKColor _color = SKColors.DarkGoldenrod;

            SKPaint _paint_icon = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black,
                StrokeCap = SKStrokeCap.Round,
                TextAlign = SKTextAlign.Center,
                TextSize = 22
            };

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

            private NodeItemShape _shape;
            private float _size_2;
            private float _size;
            private string? _textAsIcon;
            private float _textAsIcon_height;

            public string Name = "??";
        }

        public sealed class PortItem
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

            public static SKPaint _paintDarkGreenPen = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.DarkGreen,
                StrokeCap = SKStrokeCap.Square,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f
            };
        } // end class

        public sealed class ConnectionItem
        {
            public NodeItem FromItem;
            public int FromPortIdx;

            public NodeItem ToItem;
            public int ToPortIdx;

            public bool IsHovered = false;
            public bool IsSelected = false;


            public ConnectionItem(NodeItem fromItem, int fromPortIdx, NodeItem toItem, int toPortIdx)
            {
                FromItem = fromItem;
                FromPortIdx = fromPortIdx;
                ToItem = toItem;
                ToPortIdx = toPortIdx;
            }

            public void Draw(SKCanvas canvas)
            {
                var from = FromItem.Ports[FromPortIdx].Point;
                var to = ToItem.Ports[ToPortIdx].Point;

                if (IsHovered)
                    canvas.DrawLine(from, to, _paintDarkGreenPen_Hover);
                else
                    canvas.DrawLine(from, to, _paintDarkGreenPen);

                if(IsSelected)
                {
                    canvas.DrawLine(from, to, _paintDarkGreenPen_Select);
                }
            }




            //public ConnectionEnd(DrawItem fromItem, int toPortIdx)
            //{
            //    DrawItem = item;
            //    PortIdx = portIdx;
            //}

            public static SKPaint _paintDarkGreenPen_Select = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.WhiteSmoke,
                StrokeCap = SKStrokeCap.Square,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 4, 8 }, 4)
            };

            private static readonly SKColor darkGreen = SKColors.DarkGreen;


            public static SKPaint _paintDarkGreenPen = new SKPaint
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

            SKPath aLine = new SKPath();
            SKPath aLineRender = new SKPath();

            public bool HitTest(SKPoint point)
            {
                aLine.Rewind();
                aLineRender.Rewind();

                aLine.MoveTo(FromItem.Ports[FromPortIdx].Point);
                aLine.LineTo(ToItem.Ports[ToPortIdx].Point);


                _paintDarkGreenPen_Hit.GetFillPath(aLine, aLineRender, 0.5f);

                return (aLineRender.Contains(point.X, point.Y));
            }
        }

        List<ConnectionItem> _connectionItems = new List<ConnectionItem>();

        List<NodeItem> _drawItems = new List<NodeItem>();
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

            ctrSkElement.MouseRightButtonDown += CtrSkElement_MouseRightButtonDown;
            ctrSkElement.MouseLeftButtonDown += CtrSkElement_MouseLeftButtonDown;

            // *** measure text
            var bounds = new SKRect();
            _paintNodeText.MeasureText("X", ref bounds);
            _fontHeight = bounds.Bottom - bounds.Top;
            _fontMargin = Math.Min(_fontHeight / 2f, 5f);

            var srcNodeTypeIdx = AddNodeType(new NodeItemType { 
                Name = "Source",
                BkColor = SKColors.DarkGoldenrod,
                Shape = NodeItemShape.Rect,
                Size = 40f,
                TextAsIcon = "S"
            });

            var terminalNodeTypeIdx = AddNodeType(new NodeItemType
            {
                Name = "Terminal",
                BkColor = SKColor.Parse("#256590"),
                Shape = NodeItemShape.Rect,
                Size = 40f,
                TextAsIcon = "T"
            });

            var junctionNodeTypeIdx = AddNodeType(new NodeItemType
            {
                Name = "Junction",
                BkColor = SKColor.Parse("#BF2813"),
                Shape = NodeItemShape.Circle,
                Size = 30f,
                TextAsIcon = "J"
            });

            // *** create items for a test
            const int count = 12;
            _drawItems = new List<NodeItem>(count);

            var perRow = (int)Math.Sqrt(count);
            const int diam = 20;
            const int margin = 100;
            const int off = 100;
            for (int i = 0; i < count; i++)
            {
                int row = i / perRow;
                int col = i % perRow;

                _drawItems.Add(new NodeItem(_nodeTypes[srcNodeTypeIdx]) { 
                    //NodeItemTypeIdx = srcNodeTypeIdx,
                    X = off + col * (diam + margin),
                    Y = off + row * (diam + margin),
                    //Radius = diam/2,
                    //Paint = _paint
                });

                _drawItems.Last().Recalc();
            }

            //_portItems = new List<PortDrawItem>(_drawItems.Count);
            //foreach(NodeItem it in _drawItems)
            //{
            //    it.AddPort();
            //}

            //ctrSkElement.DragEnter
            //Draw(ctrSkElement);
        }

        Window _wndToolInsert = null;
        private void CtrSkElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // *** HANDLE DOUBLECLICKS
            var orgPoint = GetWorldPointFromMouseEvent(e);

            Logger.Trace("CtrSkElement_MouseLeftButtonDown");
            

            if(e.ClickCount == 2)
            {
                
                var hh = GetHitTestItem(orgPoint);
                if(hh.HitType == HitType.None)
                {
                    Logger.Trace(" >>> double click");
                    e.Handled = true;
                    //MessageBox.Show("+ add");

                    var view = new InsertNodeItemView();
                    view.ItemClicked += (nt) => {
                        var item = new NodeItem(nt)
                        {
                            X = orgPoint.X,
                            Y = orgPoint.Y
                        };
                        item.Recalc();
                        _drawItems.Add(item);
                        ctrSkElement.InvalidateVisual();
                    };

                    foreach (var nt in _nodeTypes)
                    {
                        view.AddNodeType(nt);
                    }

                    var pos = e.GetPosition(this);
                    var posS = PointToScreen(pos);
                    var wnd = new Window { 
                        Owner = Window.GetWindow(this),
                        Title = "Insert",
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        WindowStyle = WindowStyle.ToolWindow,
                        Topmost = true,
                        Width = 300,
                        Height = 400,
                        Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                        BorderBrush = Brushes.BlueViolet,
                        BorderThickness = new Thickness(1),
                        Top = posS.Y + 5,
                        Left = posS.X + 5,
                        ShowActivated = true,
                        
                        Content = view
                    };

                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    wnd.KeyDown += (s, e) => { if (e.Key == Key.Escape) wnd.Close(); };
                    wnd.ShowDialog();
                    //EventHandler dd = (object s, EventArgs e) => { wnd.Visibility = Visibility.Collapsed; };
                    //wnd.Closing += (s, e) => { wnd.Visibility = Visibility.Collapsed; e.Cancel = true; };
                    //wnd.Deactivated += dd;
                }    
                else if(hh.HitType == HitType.Elem)
                {
                    var node = hh.Item;
                    Debug.Assert(node != null);

                    
                    var ss = new StackPanel();
                    ss.Children.Add(new Label() { 
                        Content = $"Name: {node.Name}"
                    });
                    var cc = new UserControl() { Content = ss };

                    WpfTools.ShowDialog($"{node.Name} - Properties", cc);

                    ctrSkElement.InvalidateVisual();
                }
                else if (hh.HitType == HitType.Conn)
                {
                    var link = hh.ItemConn;
                    Debug.Assert(link != null);

                    var fromNode = link.FromItem;
                    var toNode = link.ToItem;

                    var ss = new StackPanel();
                    ss.Children.Add(new Label()
                    {
                        Content = $"From: {fromNode.Name}"
                    });
                    ss.Children.Add(new Label()
                    {
                        Content = $"To: {toNode.Name}"
                    });
                    var cc = new UserControl() { Content = ss };

                    WpfTools.ShowDialog($"[{fromNode.Name}] to [{toNode.Name}] - Link Properties", cc);

                    ctrSkElement.InvalidateVisual();
                }
            }
        }

        private void CtrSkElement_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Logger.Trace("> CtrSkElement_MouseRightButtonDown");

            var orgPoint = GetWorldPointFromMouseEvent(e);

            if (e.ClickCount == 1)
            {
                var hh = GetHitTestItem(orgPoint);
                if (hh.HitType == HitType.None)
                {
                    Logger.Trace(" >>> open context menu");
                    e.Handled = true;

                    var menu = new ContextMenu();
                    menu.Items.Add(new MenuItem { Header = "Item 1" });
                    ctrSkElement.ContextMenu = menu;
                }

            }
        }

        private void CtrSkElement_MouseLeave(object sender, MouseEventArgs e)
        {
            Logger.Trace(">>>>>>>>>>>> LEAVE");
            DoHitTest(null);
            ctrSkElement.InvalidateVisual();
        }

        private SKPoint _startMove; //shared for pan and move operation
        private bool _movedMouse;
        private bool _isCanvasClickDown = false;
        private bool _isMoveItemMode = false;
        private NodeItem? _moveItem;

        // *** connection drawing when connecting
        private bool _isConnectionDrawMode = false;
        private SKPoint _connectionToPoint;
        private SKPoint _connectionFromPoint;

        private NodeItem? _connectionFromItem = null;
        //private DrawItem? _connectionToItem = null;

        private int _connectionFromPortIdx = -1;
        //private int _connectionToPortIdx = -1;

        // ***

        enum HitType { None, Elem, Port, Conn };
        struct HitResult
        {
            public HitType HitType;
            public NodeItem? Item;
            public ConnectionItem? ItemConn;
            public int PortIdx;
        }

        private void CtrSkElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Logger.Trace("> MouseDown");

            var orgPoint = GetWorldPointFromMouseEvent(e);
            var pointWnd = GetPointFromMouseEvent(e);

            _isPanMode = false;
            _isSelectItemMode = false;
            _isConnectionDrawMode = false;

            //if (e.RightButton == MouseButtonState.Pressed)
            //{
            //    Logger.Trace(" >> Context menu");
            //    return;
            //}

            //e.ChangedButton == MouseButton.Left
            if (e.LeftButton != MouseButtonState.Pressed)
                return;


            var hitResult = GetHitTestItem(orgPoint);

            if (hitResult.HitType == HitType.Conn)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    hitResult.ItemConn.IsSelected = !hitResult.ItemConn.IsSelected;
                }
                else
                {
                    foreach(var cc in _connectionItems) 
                        cc.IsSelected = false;

                    hitResult.ItemConn.IsSelected = true;
                }

                _isSelectItemMode = true;
            }
            else if (hitResult.HitType == HitType.Port)
            {
                _editingConnection = null;
                // *** CONNECTING
                //if (!_isConnectionDrawMode)
                {
                    Logger.Trace(">> CONNECT - FIRST END");

                    var count = GetPortConnectionsCount(hitResult.Item, hitResult.PortIdx);
                    if (count > 0)
                    {
                        if(count == 1)
                        {
                            _editingConnection = GetPortConnections(hitResult.Item,hitResult.PortIdx).First();
                            
                        }
                        else
                        {
                            // *** count > 1
                            _editingConnection = GetPortConnections(hitResult.Item, hitResult.PortIdx).Where(x => x.IsSelected).SingleOrDefault();
                        }

                        if (_editingConnection != null)
                        {
                            _connectionItems.Remove(_editingConnection);

                            var otherItem = _editingConnection.FromItem == hitResult.Item ? _editingConnection.ToItem : _editingConnection.FromItem;
                            var otherPortIdx = _editingConnection.FromItem == hitResult.Item ? _editingConnection.ToPortIdx : _editingConnection.FromPortIdx;

                            _connectionFromPoint = otherItem.Ports[otherPortIdx].Point;
                            _connectionToPoint = hitResult.Item.Ports[hitResult.PortIdx].Point;

                            _connectionFromPortIdx = otherPortIdx;
                            _connectionFromItem = otherItem;

                            _isConnectionDrawMode = true;
                        }
                        else
                        {
                            SystemSounds.Beep.Play();
                        }    
                        
                    }
                    else
                    {
                        _connectionFromPoint = orgPoint;
                        _connectionToPoint = orgPoint;

                        _connectionFromPortIdx = hitResult.PortIdx;
                        _connectionFromItem = hitResult.Item;

                        _isConnectionDrawMode = true;
                    }
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
                //ctrSkElement.Cursor = Cursors.Hand;

                //var pos = e.GetPosition(ctrSkElement);
                //var pointWnd = new SKPoint((float)pos.X, (float)pos.Y);


                _isCanvasClickDown = true;
                _startMove = pointWnd;

                

            }
        }

        private IEnumerable<ConnectionItem> GetPortConnections(NodeItem? item, int portIdx)
        {
            foreach (var cc in _connectionItems)
            {
                var isConnected = (item == cc.FromItem && portIdx == cc.FromPortIdx) ||
                (item == cc.ToItem && portIdx == cc.ToPortIdx);

                if (isConnected)
                    yield return cc;
            }
        }

        private int GetPortConnectionsCount(NodeItem? item, int portIdx)
        {
            var count = 0;
            foreach (var cc in _connectionItems)
            {
                var isConnected = (item == cc.FromItem && portIdx == cc.FromPortIdx)
                    || (item == cc.ToItem && portIdx == cc.ToPortIdx);
                if (isConnected)
                    count++;
            }

            return count;
        }

        private void CtrSkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // *** pan mode
            ctrSkElement.ReleaseMouseCapture();
            ctrSkElement.Cursor = Cursors.Arrow;

            _isCanvasClickDown = false;
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

                        var ci = new ConnectionItem(_connectionFromItem, _connectionFromPortIdx, connectionToItem, connectionToPortIdx);
                        _connectionItems.Add(ci);
                    }
                }
                else
                {
                    if(_editingConnection != null)
                    {
                        _connectionItems.Add(_editingConnection);
                    }
                }

            }

            _isConnectionDrawMode = false;

            if(!_isSelectItemMode && !_isPanMode)
            {
                //*** deselect items
                foreach (var cc in _connectionItems)
                    cc.IsSelected = false;
            }

            _isPanMode = false;

            ctrSkElement.InvalidateVisual();
        }

        private void HandlePanOnMouseMove(SKPoint pointWnd, SKPoint orgPoint)
        {
            // *** PANING
            if (_isCanvasClickDown)
            {
                ctrSkElement.Cursor = Cursors.Hand; // panning
                _isPanMode = true;
                
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


        //SKPath aLine = new SKPath();
        //SKPath aLineRender = new SKPath();

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
                        PortItem? pp = it.Ports[i];
                        pp.IsHovered = false;
                    }
                }

                foreach (var cc in _connectionItems)
                {
                    cc.IsHovered = false;
                }

                foreach (var it in _drawItems)
                {
                    //it.Paint = _paint;
                    it.IsHovered = false;
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

            NodeItem? hitItem = null;

            HitType hitType = HitType.None;
            int portIdxHit = -1;

            bool found = false;
            // *** fist goes for ports
            foreach (var it in _drawItems)
            {
                for (int i = 0; i < it.Ports.Count; i++)
                {
                    PortItem? pp = it.Ports[i];

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
            ConnectionItem? connHit = null;
            foreach (var cc in _connectionItems)
            {
                //aLine.Rewind();
                //aLineRender.Rewind();

                //aLine.MoveTo(cc.FromItem.Ports[cc.FromPortIdx].Point);
                //aLine.LineTo(cc.ToItem.Ports[cc.ToPortIdx].Point);


                //_paintDarkGreenPen_Hit.GetFillPath(aLine, aLineRender, 0.5f);

                cc.IsHovered = cc.HitTest(point);

                if(!found && cc.IsHovered)
                {
                    //SystemSounds.Beep.Play();
                    //cc.IsHovered = true;
                    found = true;
                    hitType = HitType.Conn;
                    connHit = cc;
                }
                //else
                //{
                //    cc.IsHovered = false;
                //}
            }


            // *** now goes for nodes
            foreach (var it in _drawItems)
            {
                //SKRectI kk;
                //kk.COn

                //var vv = point - new SKPoint(it.X, it.Y);
                //if(!found && vv.Length <= it.Radius)
                if (!found && it.Bounds.Contains(point))
                {
                    it.IsHovered = true;
                    //it.Paint = _paintHighlight;
                    //hitCount++;
                    hitItem = it;
                    hitType = HitType.Elem;
                    found = true;
                    //break; // todo: handle many items
                }
                else
                {
                    //it.Paint = _paint;
                    it.IsHovered = false;
                }
            }

            return new HitResult { 
                HitType = hitType,
                Item = hitItem,
                ItemConn = connHit,
                PortIdx = portIdxHit
            };
        }

        //static SKColor _color = new SKColor(0x2c, 0x3e, 0x50);

        //SKPaint _paint = new SKPaint
        //{
        //        IsAntialias = true,
        //        Color = _color, //new SKColor(0x2c, 0x3e, 0x50),
        //        StrokeCap = SKStrokeCap.Round
        //};

        //SKPaint _paintHighlight = new SKPaint
        //{
        //    IsAntialias = true,
        //    Color = ColorTools.ShadeRGBColor(_color, 0.1f), // SKColors.BlueViolet,
        //    StrokeCap = SKStrokeCap.Round
        //};

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

        //private static readonly SKColor darkGreen = SKColors.DarkGreen;

        //SKPaint _paintDarkGreenPen = new SKPaint
        //{
        //    IsAntialias = true,
        //    Color = darkGreen,
        //    StrokeCap = SKStrokeCap.Square,
        //    Style = SKPaintStyle.Stroke,
        //    StrokeWidth = 2f
        //};

        //SKPaint _paintDarkGreenPen_Hover = new SKPaint
        //{
        //    IsAntialias = true,
        //    Color = ColorTools.ShadeRGBColor(darkGreen, +0.1f),
        //    StrokeCap = SKStrokeCap.Square,
        //    Style = SKPaintStyle.Stroke,
        //    StrokeWidth = 3f
        //};

        //SKPaint _paintDarkGreenPen_Hit = new SKPaint
        //{
        //    IsAntialias = true,
        //    Color = ColorTools.ShadeRGBColor(darkGreen, +0.1f),
        //    StrokeCap = SKStrokeCap.Square,
        //    Style = SKPaintStyle.Stroke,
        //    StrokeWidth = 5f
        //};

        SKPaint _paintNodeText = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Gray,
            //StrokeCap = SKStrokeCap.Butt,
            Style = SKPaintStyle.Fill,
            StrokeWidth = 1f,
            TextAlign = SKTextAlign.Center
        };
        private float _fontHeight;
        private float _fontMargin;
        private bool _isPanMode;
        private bool _isSelectItemMode;
        private ConnectionItem? _editingConnection;

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
                //canvas.DrawCircle(new SKPoint(it.X, it.Y), it.Radius, it.Paint);
                it.Draw(canvas);
            }

            // *** draw connections
            foreach(var cc in _connectionItems)
            {
                //var from = cc.FromItem.Ports[cc.FromPortIdx].Point;
                //var to = cc.ToItem.Ports[cc.ToPortIdx].Point;

                //if(cc.IsHovered)
                //    canvas.DrawLine(from, to, _paintDarkGreenPen_Hover);
                //else
                //    canvas.DrawLine(from, to, _paintDarkGreenPen);
                cc.Draw(canvas);
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
                canvas.DrawLine(_connectionFromPoint, _connectionToPoint, ConnectionItem._paintDarkGreenPen);
            }

            int ii = 0;
            foreach (var it in _drawItems)
            {
                //canvas.DrawText($"Node {ii}", new SKPoint(it.X, it.Y + it.Radius + _fontHeight + _fontMargin), _paintNodeText);
                canvas.DrawText(it.Name, new SKPoint(it.Bounds.MidX, it.Bounds.Bottom + _fontHeight + _fontMargin), _paintNodeText);
                ii++;
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
            //_color = SKColors.CadetBlue;
            //_paint.Color = _color;
            ctrSkElement.InvalidateVisual();
        }

        
        static public float GetTextHeight(string text, SKPaint paint)
        {
            var bounds = new SKRect();
            paint.MeasureText(text, ref bounds);
            var hh = bounds.Bottom - bounds.Top;
            return  hh;
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




