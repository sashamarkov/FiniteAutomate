using System;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace GraphSharp.Converters
{
    public class EdgeMarksConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.Assert(values != null && values.Length == 11, "EdgeRouteToPathConverter should have 9 parameters: pos (1,2), size (3,4) of source; pos (5,6), size (7,8) of target; routeInformation (9).");

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

            /*
            PathSegment[] segments = new PathSegment[1 + (hasRouteInfo ? routeInformation.Length : 0)];
            if (hasRouteInfo)
                //append route points
                for (int i = 0; i < routeInformation.Length; i++)
                    segments[i] = new LineSegment(routeInformation[i], true);
            */

            Point pLast = (hasRouteInfo ? routeInformation[routeInformation.Length - 1] : p1);
            Vector v = pLast - p2;
            if (v.Length != 0)
                v = v / v.Length * 5;

            /*
            Vector n = new Vector(-v.Y, v.X) * 0.3;
            if (v.X == 0 && v.Y == 0)
                segments[segments.Length - 1] = new ArcSegment(new Point(p2.X - 6, p2.Y - targetSize.Height / 2 - 6), new Size(20, 20), 0, true, SweepDirection.Clockwise, true);
            else
                segments[segments.Length - 1] = new LineSegment(p2 + v, true);
            */

            PathFigureCollection pfc = new PathFigureCollection(2);
            //pfc.Add(new PathFigure(p1, segments, false));

            if (v.X == 0 && v.Y == 0)
            {
                /*
                pfc.Add(new PathFigure(new Point(p2.X - 6, p2.Y - targetSize.Height / 2 - 6),
                                     new PathSegment[] {
                                                        
			                                           	new LineSegment(new Point(p2.X - 5, p2.Y - targetSize.Height / 2 - 7), true),
                                                        new LineSegment(new Point(p2.X - 2, p2.Y - targetSize.Height / 2 - 2), true),
                                                        new LineSegment(new Point(p2.X - 7, p2.Y - targetSize.Height / 2 - 5), true)
			                                           	}, true));
                */
                if (!double.IsNaN(p2.X) && !double.IsNaN(p2.Y) && values[9] != null)
                {
                    Typeface face = new Typeface(new FontFamily("Courier"), FontStyles.Normal, FontWeights.Thin, FontStretches.Normal);
                    FormattedText formattedText =
                    new FormattedText(values[9].ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, face, 14, new SolidColorBrush(Colors.Red));
                    Geometry formattedTextGeometry = formattedText.BuildGeometry(new Point(p2.X - 35, p2.Y - targetSize.Height - 10));
                    PathGeometry flattenedTextPathGeometry = PathGeometry.CreateFromGeometry(formattedTextGeometry).GetOutlinedPathGeometry();
                    for (int i = 0; i < flattenedTextPathGeometry.Figures.Count; i++)
                    {
                        pfc.Add(flattenedTextPathGeometry.Figures[i]);
                    }
                }
            }
            else
            {
                /*
                pfc.Add(new PathFigure(p2,
                                         new PathSegment[] {
			                                           	new LineSegment(p2 + v - n, true),
			                                           	new LineSegment(p2 + v + n, true)}, true));*/

                int Type = (values[10] != DependencyProperty.UnsetValue ? (int)values[10] : 0);

                if (!double.IsNaN(p1.X) && !double.IsNaN(p1.Y) && !double.IsNaN(p2.X) && !double.IsNaN(p2.Y) && values[9] != null)
                {
                    Typeface face = new Typeface(new FontFamily("Courier"), FontStyles.Normal, FontWeights.Thin, FontStretches.Normal);
                    FormattedText formattedText =
                    new FormattedText(values[9].ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, face, 14, new SolidColorBrush(Colors.Red));
                    Geometry formattedTextGeometry;

                    if (Type == 0)
                    {
                        formattedTextGeometry = formattedText.BuildGeometry(new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2));
                    }
                    else
                    {
                        double length = Math.Sqrt(Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.X - p1.X, 2));
                        double d1, d2;
                        //d1 = d2 = Math.Pow(length, 0.4);
                        d1 = d2 = Math.Pow(length, 0.4);

                        double x, y;
                        y = (p1.Y + p2.Y) / 2 -5;
                        x = (p1.X + p2.X) / 2;

                        if ((p2.Y - p1.Y) < 0)
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

                        formattedTextGeometry = formattedText.BuildGeometry(new Point(x + d1, y + d2));
                    }
                        
                    //Geometry formattedTextGeometry = formattedText.BuildGeometry(new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2));
                    PathGeometry flattenedTextPathGeometry = PathGeometry.CreateFromGeometry(formattedTextGeometry).GetOutlinedPathGeometry();
                    for (int i = 0; i < flattenedTextPathGeometry.Figures.Count; i++)
                    {
                        pfc.Add(flattenedTextPathGeometry.Figures[i]);
                    }
                }
            }

            return pfc;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
