using System;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace GraphSharp.Converters
{
	/// <summary>
	/// Converts the position and sizes of the source and target points, and the route informations
	/// of an edge to a path.
	/// The edge can bend, or it can be straight line.
	/// </summary>
    public class EdgeRouteToPathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.Assert(values != null && values.Length == 10, "EdgeRouteToPathConverter should have 9 parameters: pos (1,2), size (3,4) of source; pos (5,6), size (7,8) of target; routeInformation (9).");

            #region Get the inputs
            //get the position of the source
            Point sourcePos = new Point()
            {
                X = (values[0] != DependencyProperty.UnsetValue ? (double)values[0] : 0.0),
                Y = (values[1] != DependencyProperty.UnsetValue ? (double)values[1] : 0.0)
            };
            //get the size of the source
            Size sourceSize = new Size()
            {
                Width = (values[2] != DependencyProperty.UnsetValue ? (double)values[2] : 0.0),
                Height = (values[3] != DependencyProperty.UnsetValue ? (double)values[3] : 0.0)
            };
            //get the position of the target
            Point targetPos = new Point()
            {
                X = (values[4] != DependencyProperty.UnsetValue ? (double)values[4] : 0.0),
                Y = (values[5] != DependencyProperty.UnsetValue ? (double)values[5] : 0.0)
            };
            //get the size of the target
            Size targetSize = new Size()
            {
                Width = (values[6] != DependencyProperty.UnsetValue ? (double)values[6] : 0.0),
                Height = (values[7] != DependencyProperty.UnsetValue ? (double)values[7] : 0.0)
            };


            //get the route informations
            Point[] routeInformation = (values[8] != DependencyProperty.UnsetValue ? (Point[])values[8] : null);
            #endregion
            bool hasRouteInfo = routeInformation != null && routeInformation.Length > 0;

            //
            // Create the path
            //
            Point p1 = GraphConverterHelper.CalculateAttachPoint(sourcePos, sourceSize, (hasRouteInfo ? routeInformation[0] : targetPos));
            Point p2 = GraphConverterHelper.CalculateAttachPoint(targetPos, targetSize, (hasRouteInfo ? routeInformation[routeInformation.Length - 1] : sourcePos));

            // Тип ребра
            int Type = (values[9] != DependencyProperty.UnsetValue ? (int)values[9] : 0);
            double d1, d2;
            d1 = d2 = 3;

            PathSegment[] segments = new PathSegment[1 + (hasRouteInfo ? routeInformation.Length : 0)];
            if (hasRouteInfo)
                //append route points
                for (int i = 0; i < routeInformation.Length; i++)
                    segments[i] = new LineSegment(routeInformation[i], true);

            Point pLast = (hasRouteInfo ? routeInformation[routeInformation.Length - 1] : p1);
            Vector v = pLast - p2;
            if (v.Length != 0)
                v = v / v.Length * 5;

            Vector n = new Vector(-v.Y, v.X) * 0.3;

            if (v.X == 0 && v.Y == 0)
                segments[segments.Length - 1] = new ArcSegment(new Point(p2.X - 6, p2.Y - targetSize.Height / 2 - 6), new Size(20, 20), 0, true, SweepDirection.Clockwise, true);
            else
            {

                if (Type == 0)
                {
                    segments[segments.Length - 1] = new LineSegment(p2 + v, true);
                }
                else
                {
                    Point p = p2 + v;
                    double length = Math.Sqrt(Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.X - p1.X, 2));

                    if((p2.Y - p1.Y) < 0)
                    {
                        if ((p2.X - p1.X) > 0)
                        {
                            d1 = -d1;
                            d2 = -d2;
                        }
                        else
                        {
                            d1 = -d1;
                        }
                    }
                    else
                    {
                        if ((p2.X - p1.X) > 0)
                        {
                            d2 = -d2;
                        }
                    }

                    segments[segments.Length - 1] = new ArcSegment(new Point(p2.X + d1, p2.Y + d2), new Size(length, length / 5), Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X)) * (180 / Math.PI), false, SweepDirection.Clockwise, true);
                }
            }

            PathFigureCollection pfc = new PathFigureCollection(2);
            pfc.Add(new PathFigure(p1, segments, false));

            if (v.X == 0 && v.Y == 0)
            {
                pfc.Add(new PathFigure(new Point(p2.X - 6, p2.Y - targetSize.Height / 2 - 6),
                                     new PathSegment[] {
                                                        
			                                           	new LineSegment(new Point(p2.X - 5, p2.Y - targetSize.Height / 2 - 7), true),
                                                        new LineSegment(new Point(p2.X - 2, p2.Y - targetSize.Height / 2 - 2), true),
                                                        new LineSegment(new Point(p2.X - 7, p2.Y - targetSize.Height / 2 - 5), true)
			                                           	}, true));
            }
            else
            {
                Point arrowPoint;
                if (Type == 0)
                    arrowPoint = p2;
                else
                {
                    arrowPoint = new Point(p2.X + d1, p2.Y + d2);
                }

                pfc.Add(new PathFigure(arrowPoint,
                                         new PathSegment[] {
			                                           	new LineSegment(arrowPoint + v - n, true),
			                                           	new LineSegment(arrowPoint + v + n, true)}, true));

                /*
                pfc.Add(new PathFigure(p2,
                                         new PathSegment[] {
			                                           	new LineSegment(p2 + v - n, true),
			                                           	new LineSegment(p2 + v + n, true)}, true));
                */
            }

            return pfc;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}