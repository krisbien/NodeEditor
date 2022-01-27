using SkiaSharp;
using SkiaSharp.Views.WPF;
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
using static SkiaSharpTestApp.MainWindow;

namespace SkiaSharpTestApp
{
    /// <summary>
    /// Interaction logic for InsertNodeItemView.xaml
    /// </summary>
    public partial class InsertNodeItemView : UserControl
    {

        public event Action<NodeItemType> ItemClicked;
        
        public InsertNodeItemView()
        {
            InitializeComponent();

            //var src = new BitmapImage(new Uri(@"D:\ICONS\ICONS\fatcow-hosting-icons-3.9.2\FatCow_Icons32x32\acoustic_guitar.png"));

            //var pp = new SKPaint()
            //{
            //    Style = SKPaintStyle.Fill,
            //    Color = SKColors.Goldenrod,
            //    IsAntialias = true,
            //};

            //SKBitmap bitmap = new SKBitmap(32, 32);
            //using (SKCanvas canvas = new SKCanvas(bitmap))
            //{
            //    canvas.DrawRoundRect(new SKRect(0, 0, 32, 32), 5, 5, pp);
            //}

            //WriteableBitmap bb = WPFExtensions.ToWriteableBitmap(bitmap);


            //Items = new List<Item>();
            //Items.Add(new Item("Source", bb));
            //Items.Add(new Item("Terminal", src));
            //Items.Add(new Item("Junction", null));

            DataContext = this;
        }

        //public void AddNodeType()

        public List<Item> Items { get; set;  } = new List<Item> { };

        public class Item
        {
            public Item(string name, BitmapSource? icon, NodeItemType type)
            {
                Name = name;
                Icon = icon;
                NodeItemType = type;
            }

            public string Name { get; set; }
            public BitmapSource? Icon { get; set; }
            public NodeItemType NodeItemType { get; }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListBoxItem)sender).DataContext as Item;
            Debug.Assert(item != null);
            //MessageBox.Show($"DBL: {item.Name}");

            Window.GetWindow(this).Close();

            if (item != null)
                ItemClicked?.Invoke(item.NodeItemType);
        }

        internal void AddNodeType(MainWindow.NodeItemType nt)
        {
            var item = new NodeItem(nt, "") { 
                X = nt.Size/2f,
                Y = nt.Size/2f
            };
            item.Recalc();

            SKBitmap bitmap = new SKBitmap((int)nt.Size, (int)nt.Size);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                //canvas.DrawRoundRect(new SKRect(0, 0, 32, 32), 5, 5, pp);
                item.Draw(canvas);
                
            }

            WriteableBitmap bb = WPFExtensions.ToWriteableBitmap(bitmap);
            Items.Add(new Item(nt.Name, bb, nt));
        }

        //private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    //MessageBox.Show("DBL");
        //}

        //private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if(e.ClickCount == 2)
        //    {
        //        MessageBox.Show("DBL");
        //    }
        //}
    }
}
