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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

partial class Scull_Furnaces_Main_Window
 {
	 private void voltagePlotting(ParameterName parameterName,Canvas canGraph)
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
			
			string unitOfMeasure;
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
			#if POLYLINE
			//Значения для графика тока подтягиваются сюда.	
			//Для рисования через Polyline
			points = new PointCollection();	

			int numSec = 0;
			short    intAssembled;
			double realValueOfFunction;
			double peakAtInterval = 0;   //максимальное значение функции на интервале
			double bottomAtInterval = Int32.MaxValue; //минимальное значение функции на интервале
			Point begPoint = new Point(xmin,ymax);
			Point endPoint = new Point(xmin,ymax);
			bool theFirstDotInPair = true;
			for(
			int iParam = (int)parameterName/(Constants.ParameterData[(int)(parameterName)].parameterType == ParameterType.аналоговый ? 1:10)+(int)Globals.startOfParameterOutput.TotalSeconds*Constants.ParamsBlockLengthInBytes;     
			iParam < unpackedParameters.inflatedParameters.Length;
			iParam = iParam+Constants.ParamsBlockLengthInBytes)
			{
				
				if(Constants.ParameterData[(int)(parameterName)].parameterType == ParameterType.аналоговый)
				{
					
					intAssembled = BitConverter.ToInt16(unpackedParameters.inflatedParameters,iParam);
					realValueOfFunction = (double)intAssembled/10;
					switch(Constants.ParameterData[(int)(parameterName)].parameterUnit)
						{
							case ParameterUnit._:
							unitOfMeasure="";
							break;
							case ParameterUnit.м3_ч:
							unitOfMeasure="м3/ч";
							break;
							case ParameterUnit.мм_рт_ст:
							if(intAssembled <0)
							{	
							unitOfMeasure="мм рт.ст";
							intAssembled &= 0x777;
							}
							else
							unitOfMeasure="мк";						
							break;

						}
				}		
				else
					throw new Exception();
				//*
				if(peakAtInterval < realValueOfFunction )peakAtInterval = realValueOfFunction;
				if(bottomAtInterval > realValueOfFunction )bottomAtInterval = realValueOfFunction;
				if(numSec%intSecondsPerDot==0)
				{
					//отмечать в графике МАКСИМАЛЬЕОЕ значение на интервале
					//но надо отмечать и МИНИМАЛЬНОЕ !
					if(!theFirstDotInPair)
					{
						points.Add( new Point(xmin +(numSec-1)*dotsPerSecond,ymax - dotsPerVolt*bottomAtInterval));
						points.Add( new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*peakAtInterval));
						//points.Add( new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*bottomAtInterval));
						peakAtInterval = 0;
						bottomAtInterval = Int32.MaxValue;
						theFirstDotInPair = true;
					}
					else
						theFirstDotInPair = false;
				}
/*				else 
				if(numSec%intSecondsPerDot==(int)(intSecondsPerDot/2))
				{
					//отмечать в графике МАКСИМАЛЬЕОЕ значение на интервале
					//но надо отмечать и МИНИМАЛЬНОЕ !
					points.Add( new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*bottomAtInterval));
					bottomAtInterval = Int32.MaxValue;
				} */
				//*
				/*endPoint = new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*bottomAtInterval);
				plotGeometry.Children.Add(new LineGeometry(begPoint,endPoint));
				WriteLine("Dot number {0}",numSec );
				endPoint = begPoint;
				points.Add( new Point(xmin +(numSec++)*dotsPerSecond,ymax - dotsPerVolt*(double)intAssembled/10));*/
				numSec++;		
				
			} 
			Polyline polyline1 = new Polyline();
			polyline1.StrokeThickness = 1;
			polyline1.Stroke = Brushes.Red;
			polyline1.Points = points;
			canGraph.Children.Add(polyline1); 
			
			#else
    		canGraph.Children.Add(new VisualHostForPlot(unpackedParameters,parameterName,rectBounds));
			#endif
			return null;
		}), null);

	}
		 
}
 