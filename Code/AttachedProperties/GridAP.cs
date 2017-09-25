using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PowerSupplyNotifier.Code.AttachedProperties
{
    // ReSharper disable once InconsistentNaming
    public class GridAP
    {
        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached(
            "Rows", typeof(string), typeof(GridAP), new PropertyMetadata(default(string), RowsPropertyChangedCallback));

        private static readonly Regex ItemFillRegex = new Regex(@"(?<count>\d*)?\*", RegexOptions.Compiled);

        private static readonly CultureInfo NumberCulture = new CultureInfo("en-US");

        private static void RowsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var grid = (Grid) dependencyObject;

            if (grid.RowDefinitions.Count > 0)
                grid.RowDefinitions.Clear();

            var value = (string) args.NewValue;

            var items = 
                value
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(it => it.ToUpperInvariant().Trim())
                    .ToList();

            foreach (var item in items)
            {
                if (item == "A" || item == "AUTO")
                    grid.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
                else if (double.TryParse(item,  NumberStyles.Any, NumberCulture, out double pixelSize))
                    grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(pixelSize)});
                else
                {
                    var match = ItemFillRegex.Match(item);

                    var size = !string.IsNullOrEmpty(match.Groups["count"].Value) ? double.Parse(match.Groups["count"].Value, NumberCulture) : 1;

                    grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(size, GridUnitType.Star)});
                }
            }
        }

        public static void SetRows(DependencyObject element, string value)
        {
            element.SetValue(RowsProperty, value);
        }

        public static string GetRows(DependencyObject element)
        {
            return (string) element.GetValue(RowsProperty);
        }
    }
}
