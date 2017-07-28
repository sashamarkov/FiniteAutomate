using System;
using System.Windows.Data;
using System.Collections.Generic;
using QuickGraph;

namespace FiniteAutomate.Converters
{
    class HighlightEdgesConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<IEdge<object>> highlightEdges = (List<IEdge<object>>)value;
            if (highlightEdges == null) return "";
            string path = "";
            foreach (EdgeExp e in highlightEdges)
            {
                path += e.Tag + ", ";
            }

            if (path != "")
            {
                path = path.Remove(path.Length - 2, 2);
                path = "Путь от начала: " + path;
            }

            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }

        #endregion

    }
}
