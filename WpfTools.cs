using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace SkiaSharpTestApp
{
    public static class WpfTools
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();



        public static void ShowDialog(string title, Control userControl)
        {

            var dialogContent = new DialogContentWpfControl() {
            };

            


            var wnd = new Window
            {
                Owner = GetCurrentActiveWindow(),
                Title = title,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                //Topmost = true,
                Width = 600,
                Height = 500,
                Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                //Foreground  = Brushes.WhiteSmoke,
                //BorderBrush = Brushes.BlueViolet,
                BorderThickness = new Thickness(1),
                //Top = posS.Y + 5,
                //Left = posS.X + 5,
                ShowActivated = true,
                Content = dialogContent
            };

            dialogContent.SetContent(userControl);
            dialogContent.ButtonCancel.IsCancel = true;
            dialogContent.ButtonOK.Click += (s, e) => {
                wnd.DialogResult = true;
            };


            wnd.ShowDialog();
        }


        private static Window GetCurrentActiveWindow()
        {
            IntPtr active = GetActiveWindow();

            var activeWindow = Application.Current.Windows.OfType<Window>()
                .SingleOrDefault(window => new WindowInteropHelper(window).Handle == active);

            if(activeWindow == null)
            {
                SystemSounds.Beep.Play();
                activeWindow = Application.Current.MainWindow;
            }

            return activeWindow;
        }
    }
}
