using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowWhatProcessLocksFile.Gui.Controls
{
    public partial class IconButton : Button
    {
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(IconButton));

        public IconButton()
        {
            InitializeComponent();
        }
    }
}
