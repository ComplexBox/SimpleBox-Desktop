using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json;
using NuGet;

namespace SimpleBox.Controls
{
    public sealed class FluentIcon : Control
    {
        private Path _iconDisplay;

        private int _iconSize = 24;

        public FluentIcon() => DefaultStyleKey = typeof(FluentIcon);

        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
            "Symbol", typeof(string), typeof(FluentIcon), new PropertyMetadata("", OnSymbolChanged));

        public string Symbol
        {
            get => (string) GetValue(SymbolProperty);
            set
            {
                SetValue(SymbolProperty, value);
                OnSymbolChanged(this, new DependencyPropertyChangedEventArgs());
            }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(double), typeof(FluentIcon), new PropertyMetadata(1d, OnSizeChanged));

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FluentIcon self) || self._iconDisplay is null) return;
            double scale = self.Size * (self._iconSize / 24d);
            self._iconDisplay.SetValue(RenderTransformProperty,
                new TransformGroup
                {
                    Children =
                    {
                        new ScaleTransform(scale, scale,
                            self._iconSize / 2d, self._iconSize / 2d)
                    }
                });
            Thickness padding = (Thickness) self.GetValue(PaddingProperty);
            self.SetValue(WidthProperty, self._iconSize * scale + padding.Left + padding.Right);
            self.SetValue(HeightProperty, self._iconSize * scale + padding.Top + padding.Bottom);
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set
            {
                SetValue(SizeProperty, value);
                OnSizeChanged(this, new DependencyPropertyChangedEventArgs());
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!(GetTemplateChild("IconDisplay") is Path pi)) return;
            _iconDisplay = pi;
            Symbol = Symbol;
            Size = Size;
        }

        private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FluentIcon self) || self.Symbol is null || self._iconDisplay is null) return;
            if (!FluentIconDataSource.TryGetValue(self.Symbol, out FluentIconSource value)) return;
            self._iconDisplay.Data = Geometry.Parse(value.Data);
            self._iconSize = value.Size;
        }

        #region Data Source

        private static Dictionary<string, FluentIconSource> _fluentIconDataSource;

        private static Dictionary<string, FluentIconSource> FluentIconDataSource
        {
            get
            {
                if (!(_fluentIconDataSource is null)) return _fluentIconDataSource;

                string data = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Icons.json"))?.Stream.ReadToEnd();
                if (!string.IsNullOrEmpty(data))
                    _fluentIconDataSource = JsonConvert.DeserializeObject<Dictionary<string, FluentIconSource>>(data);

                return _fluentIconDataSource;
            }
        }

        #endregion
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class FluentIconSource
    {
        [JsonProperty]
        private string data = "";

        public string Data => data;

        [JsonProperty]
        private int size = 24;

        public int Size => size;
    }
}
