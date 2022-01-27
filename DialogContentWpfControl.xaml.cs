using System;
using System.Collections.Generic;
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
    /// Interaction logic for DialogContentWpfControl.xaml
    /// </summary>
    public partial class DialogContentWpfControl : UserControl
    {
        public DialogContentWpfControl()
        {
            InitializeComponent();
        }

        public void SetContent(Control userControl)
        {
            ctrContent.Content = userControl;
        }

        public Button ButtonOK => btnOk;
        public Button ButtonCancel => btnCancel;
    }
}
