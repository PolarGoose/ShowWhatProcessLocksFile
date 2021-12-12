using System;
using System.Windows;
using System.Windows.Controls;

namespace ShowWhatProcessLocksFile.Gui.Controls
{
    public partial class IconButton : Button
    {
        public Uri Icon
        {
            get => (Uri)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(Uri), typeof(IconButton));

        public IconButton()
        {
            InitializeComponent();
        }
    }
}
