using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace OpenNUI.Samples.ColorBasics
{
    public class ClickableTextBlock : TextBlock
    {

        public static readonly DependencyProperty HoverBrushProperty =
          DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(ClickableTextBlock), new UIPropertyMetadata(new SolidColorBrush(Colors.Black)));

        private bool _hoverLock;
        public bool hoverLock
        {
            get
            {
                return _hoverLock;
            }
            set
            {
                _hoverLock = value;
                if (_hoverLock)
                    this.Foreground = HoverBrush;
                else
                    this.Foreground = NormalBrush;
            }
        }

        public Brush HoverBrush
        {
            get { return (Brush)this.GetValue(HoverBrushProperty); }
            set { this.SetValue(HoverBrushProperty, value); }
        }
        public Brush NormalBrush;

        public override void EndInit()
        {
            NormalBrush = this.Foreground;
            base.EndInit();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this.Foreground = HoverBrush;
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (hoverLock == false)
                this.Foreground = NormalBrush;
            base.OnMouseLeave(e);
        }
    }
}
