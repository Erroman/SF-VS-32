using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static Constants;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

partial class Scull_Furnaces_Main_Window
 {
	 private void vacuumPlotting(ParameterName parameterName,Canvas canGraph)
	 {
		 		TickParamsAll unpackedParameters = app.unpackedParameters; //данные для построения графика
		Rect rectBounds = new Rect(0,0,0,0); //структура для хранения координат внутри части окна, где будет рисоваться график
		this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
		{
			
			if(canGraph.ActualWidth==0)return null;
			canGraph.Children.Clear();
			rectBounds.Width=canGraph.ActualWidth;
			rectBounds.Height=canGraph.ActualHeight;
			
		
		const double dashTickLength = 10;
		const double marginX = 10;
		const double marginY = 10;
            double xmin = marginX;
            double xmax = rectBounds.Width - marginX;
            double ymin = marginY;
            double ymax = rectBounds.Height-marginY;
            double step = Math.Round((xmax - xmin)/(24*6));
		
			double dotsPerSecond = (xmax - xmin)/SecondsInADay;
			int intSecondsPerDot = (int)(SecondsInADay/(xmax - xmin));
			double dotsPerVolt = 100*step/100;


            // Make the X axis.
            GeometryGroup axis_X_geom = new GeometryGroup();
			
            axis_X_geom.Children.Add(new LineGeometry(new Point(xmin, ymax), new Point(xmax, ymax)));
			
            for (double x = xmin + step; x <= xmax ; x += step)
            {
				
				if((x-xmin)/step%6 == 0)
                axis_X_geom.Children.Add(new LineGeometry
				(
                    new Point(x, ymax - dashTickLength),
                    new Point(x, ymax))
				);
				else
                axis_X_geom.Children.Add(new LineGeometry
				(
                    new Point(x, ymax - dashTickLength / 2),
                    new Point(x, ymax))
				);
					
            }

            Path axis_X_path = new Path();
            axis_X_path.StrokeThickness = 1;
            axis_X_path.Stroke = Brushes.Black;
            axis_X_path.Data = axis_X_geom;
            
            canGraph.Children.Add(axis_X_path);

            // Make the Y ayis.
            GeometryGroup axis_Y_geom = new GeometryGroup();
			
           axis_Y_geom.Children.Add(new LineGeometry(new Point(xmin, ymin), new Point(xmin, ymax)));
			
            for (double y = ymax; y >= ymin; y -= step)
            {
				
				if(Math.Round((ymax - y)/step)%10 == 0)
				axis_Y_geom.Children.Add(new LineGeometry
				(
							new Point(xmin, y),
							new Point(xmin + 2*dashTickLength, y))
				);
				else
				if(Math.Round((ymax - y)/step)%5 == 0)
				axis_Y_geom.Children.Add(new LineGeometry
				(
							new Point(xmin, y),
							new Point(xmin + dashTickLength, y))
				);
				else
				axis_Y_geom.Children.Add(new LineGeometry
				(
							new Point(xmin, y),
							new Point(xmin + dashTickLength / 2, y))
				);
					
			}	

            Path axis_Y_path = new Path();
            axis_Y_path.StrokeThickness = 1;
            axis_Y_path.Stroke = Brushes.Black;
            axis_Y_path.Data = axis_Y_geom;

            canGraph.Children.Add(axis_Y_path);
    		canGraph.Children.Add(new VisualHostForPlot(unpackedParameters,parameterName,rectBounds));

            return null;

		}), null);

	}
		 
}
 