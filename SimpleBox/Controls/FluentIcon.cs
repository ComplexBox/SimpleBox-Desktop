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

        public FluentIcon()
        {
            DefaultStyleKey = typeof(FluentIcon);
        }

        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
            "Symbol", typeof(string), typeof(FluentIcon), new PropertyMetadata(default(string)));

        public string Symbol
        {
            get => (string) GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!(GetTemplateChild("IconDisplay") is Path pi)) return;
            _iconDisplay = pi;
            Symbol = Symbol;
        }

        private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FluentIcon self) || !(e.NewValue is string) || self._iconDisplay is null) return;
            if (FluentIconDataSource.TryGetValue((string)e.NewValue, out string value))
                self._iconDisplay.Data = Geometry.Parse(value);
        }

        #region Data Source

        private static Dictionary<string, string> _fluentIconDataSource;

        private static Dictionary<string, string> FluentIconDataSource
        {
            get
            {
                if (!(_fluentIconDataSource is null)) return _fluentIconDataSource;

                string data = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Icons.json"))?.Stream.ReadToEnd();
                if (!string.IsNullOrEmpty(data))
                    _fluentIconDataSource = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

                return _fluentIconDataSource;
            }
        }

        #endregion
    }
}
