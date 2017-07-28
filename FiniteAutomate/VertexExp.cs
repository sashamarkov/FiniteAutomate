using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using QuickGraph;

namespace FiniteAutomate
{
    public class VertexExp : INotifyPropertyChanged, IComparable
    {
        public VertexExp() { }
        public VertexExp(int id, int type)
        {
            Id = id;
            Type = type;
        }

        public int Id { get; set; }
        public int Type { get; set; }

        public bool IsEndVertex { get; set; }

        private int sign;
        public int Sign
        {
            get
            {
                return sign;
            }
            set
            {
                sign = value;
                NotifyPropertyChanged("Sign");
            }
        }

        private Color vertexColor;
        public Color VertexColor
        {
            get
            {
                return vertexColor;
            }
            set
            {
                vertexColor = value;
                NotifyPropertyChanged("VertexColor");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            VertexExp v = (VertexExp)obj;
            return (Id == v.Id && Type == v.Type);
        }

        public int CompareTo(object obj)
        {
            VertexExp v = (VertexExp)obj;
            return this.Id.CompareTo(v.Id);
        }

        public static VertexExp AddVertex(int sourceId, int type, bool replace_type, BidirectionalGraph<object, IEdge<object>> g)
        {
            VertexExp vertex = new VertexExp(sourceId, type);
            /*VertexExp old = g.Vertices.SingleOrDefault(v => v.Id == sourceId);
            if (old != null)
            {
                if (replace_type) old.Type = type;
                vertex = old;
            }
            else
            {
                g.AddVertex(vertex);
            }*/

            object old = g.Vertices.SingleOrDefault(v => (v as VertexExp).Id == sourceId);
            if (old != null)
            {
                if (replace_type) (old as VertexExp).Type = type;
                vertex = old as VertexExp;
            }
            else
            {
                g.AddVertex(vertex);
            }

            return vertex;
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color)
            {
                Color color = (Color)value;
                    return new SolidColorBrush(color);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
