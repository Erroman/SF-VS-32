using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static Constants;
using static System.Console;
using static TransformWorldToScreen;
using System.Windows.Shapes;

partial class Scull_Furnaces_Main_Window
 {
	 private void currentPlotting(ParameterName parameterName,Canvas canGraph,params AlarmEventArgs[] aea)
	 {
 		const double dashTickLength = 10;
		const double marginX = 25;
		const double marginY = 25;
		const double dashHourTickLength = 12;
		const double dashHalfHourTickLength = 8;
        const double dashTenMinuteTickLength = 5;
        const double dashFiveMinuteTickLength = 3;
        const double dashMinuteTickLength = 2;
		const double axisLineThickness = 1;
		double LowerLimitForTimeOnXAxis;
		double UpperLimitForTimeOnXAxis;
		double LowerLimitForCurrentOnYAxis;
		double UpperLimitForCurrentOnYAxis; 
		GeometryGroup axisX = new GeometryGroup();
		GeometryGroup axisY = new GeometryGroup();


		TickParamsAll unpackedParameters = app.unpackedParameters; //данные для построения графика
		Rect rectGraphWithAxesBounds = new Rect(0,0,0,0); //структура для хранения координат внутри части окна, где будет рисоваться график с осями координат,
		//обозначаними осей, числоввыми значаниями и полями для масштабирования осей
		Rect rectGraphBounds = new Rect(0,0,0,0); //структура для хранения координат внутри части окна, где будет рисоваться только линия графика 
		this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
		{
			
			if(canGraph.ActualWidth==0)return null;
			canGraph.Children.Clear();
			rectGraphWithAxesBounds.Width=canGraph.ActualWidth;
			rectGraphWithAxesBounds.Height=canGraph.ActualHeight;
			
	
            double xmin = marginX;
            double xmax = rectGraphWithAxesBounds.Width - marginX;
            double ymin = marginY;
            double ymax = rectGraphWithAxesBounds.Height-marginY;
			//вызов может быть 1) с часов, 2) при начальном запуске программы, 3) при смене файла для отображения параметров
			switch(aea.Length)
			{
				case 1:
					LowerLimitForTimeOnXAxis = aea[0].TicksToAlarm;										//изиенение нижней границы интервала времени
					UpperLimitForTimeOnXAxis = Int32.Parse(this.endTimeOnXAxis.Ticks.Text); 
					break;
				case 2:
					LowerLimitForTimeOnXAxis = Int32.Parse(this.begTimeOnXAxis.Ticks.Text); 
					UpperLimitForTimeOnXAxis = aea[1].TicksToAlarm;										//изиенение верхней границы интервала времени
					break;
				default:
					LowerLimitForTimeOnXAxis = Int32.Parse(this.begTimeOnXAxis.Ticks.Text); //интервал времени берётся непосредственно с часов,
					UpperLimitForTimeOnXAxis = Int32.Parse(this.endTimeOnXAxis.Ticks.Text); // вызов пришёл при выполнении условий 2) или 3)
					break;
			}
			LowerLimitForCurrentOnYAxis = 0;
			UpperLimitForCurrentOnYAxis = Int32.Parse(maxValueForCurrentOnYAxis.Text);
			axisX = new GeometryGroup();
			axisY = new GeometryGroup();
			
			PrepareTransformations(
			LowerLimitForTimeOnXAxis,UpperLimitForTimeOnXAxis,LowerLimitForCurrentOnYAxis,UpperLimitForCurrentOnYAxis,
			xmin,xmax,ymin,ymax
			);
			rectGraphBounds.X = xmin;
			rectGraphBounds.Y = ymin;
			rectGraphBounds.Width = xmax;
			rectGraphBounds.Height = ymax;
            double step = Math.Round((xmax - xmin)/(24*6));

			axisX = new GeometryGroup();
			
			axisX.Children.Add(new LineGeometry(WtoD(new Point(LowerLimitForTimeOnXAxis, LowerLimitForCurrentOnYAxis)), WtoD(new Point(UpperLimitForTimeOnXAxis, LowerLimitForCurrentOnYAxis))));
			//Расставляем часовые деления на оси X
			//Расставляем получасовые деления на оси X
			//Расставляем 10-минутные деления на оси X
			//Для крупного масштаба расставляем 5 - минутные
			//и минутные деления.
			Action<string,Point,HorizontalAlignment,VerticalAlignment> PutLabel = DrawText;
			PutTimeTicks(3600,dashHourTickLength,PutLabel);
			PutLabel=null;
			PutTimeTicks(1800,dashHalfHourTickLength,PutLabel);
			PutTimeTicks(600,dashTenMinuteTickLength,PutLabel);
			if((UpperLimitForTimeOnXAxis-LowerLimitForTimeOnXAxis)/3600<=2)
			{
				PutTimeTicks(300,dashFiveMinuteTickLength,PutLabel);
				PutTimeTicks(60,dashMinuteTickLength,PutLabel);
			}
			
			Path axisX_path = new Path();
			axisX_path.StrokeThickness = axisLineThickness;
			axisX_path.Stroke = Brushes.Black;
			axisX_path.Data = axisX;
			
           axisY = new GeometryGroup();
           PutYTicks(10,10.0,null);
           axisY.Children.Add(new LineGeometry(new Point(xmin, ymin), new Point(xmin, ymax)));
			
            for (double y = ymax; y >= ymin; y -= step)
            {
				
				if(Math.Round((ymax - y)/step)%10 == 0)
				axisY.Children.Add(new LineGeometry
				(
							new Point(xmin, y),
							new Point(xmin + 2*dashTickLength, y))
				);
				else
				if(Math.Round((ymax - y)/step)%5 == 0)
				axisY.Children.Add(new LineGeometry
				(
							new Point(xmin, y),
							new Point(xmin + dashTickLength, y))
				);
				else
				axisY.Children.Add(new LineGeometry
				(
							new Point(xmin, y),
							new Point(xmin + dashTickLength / 2, y))
				);
					
			}	

            Path axisY_path = new Path();
            axisY_path.StrokeThickness = 1;
            axisY_path.Stroke = Brushes.Black;
            axisY_path.Data = axisY;
			
			canGraph.Children.Add(axisX_path);
            canGraph.Children.Add(axisY_path);
			
			WriteLine("Adding VisualHostForPlot_1 to the Children collection of the Canvas");
  			canGraph.Children.Add(new VisualHostForPlot_1(unpackedParameters,parameterName,rectGraphBounds));

			return null;
		}), null);
	void PutTimeTicks(int IntervalInSeconds,double TickLength,Action<string,Point,HorizontalAlignment,VerticalAlignment> PutLabel)
	{
		Point onX_axis;
		Point overX_axis;

		for(double Dash = TickMeasure(LowerLimitForTimeOnXAxis);Dash < UpperLimitForTimeOnXAxis;Dash+=IntervalInSeconds)
		{
				  onX_axis = WtoD(new Point(Dash,LowerLimitForCurrentOnYAxis));
				  overX_axis = onX_axis;
				  overX_axis.Y -= TickLength;
				  axisX.Children.Add(new LineGeometry
				(
					onX_axis,
					overX_axis
				)
				);
				Point underX_axis = onX_axis;
					  underX_axis.Y = overX_axis.Y;	
				if(PutLabel!=null)
				{
					PutLabel(((int)Dash / IntervalInSeconds).ToString(),underX_axis,HorizontalAlignment.Center,VerticalAlignment.Top);
				}
		
		}
		double TickMeasure(double timeInSeconds)
		{
			return (((int)timeInSeconds / IntervalInSeconds) + (( timeInSeconds % IntervalInSeconds == 0) ? 0:1))*IntervalInSeconds;
		}
	}
    void PutYTicks(int IntervalInAmperes, double TickLength, Action<string, Point, HorizontalAlignment, VerticalAlignment> PutLabel)
        {
            Point onY_axis;
            Point right_toY_axis;

            for (double Dash = TickMeasure(LowerLimitForCurrentOnYAxis); Dash < UpperLimitForCurrentOnYAxis; Dash += IntervalInAmperes)
            {
                onY_axis = WtoD(new Point(LowerLimitForTimeOnXAxis, Dash));
                right_toY_axis = onY_axis;
                right_toY_axis.Y += TickLength;
                axisX.Children.Add(new LineGeometry
              (
                  onY_axis,
                  right_toY_axis
              )
              );
                Point left_toY_axis = onY_axis;
                left_toY_axis.Y = right_toY_axis.Y;
                if (PutLabel != null)
                {
                    PutLabel(((int)Dash / IntervalInAmperes).ToString(), left_toY_axis, HorizontalAlignment.Center, VerticalAlignment.Top);
                }

            }
            double TickMeasure(double CurrentInAmperes)
            {
                return (((int)CurrentInAmperes / IntervalInAmperes) + ((CurrentInAmperes % IntervalInAmperes == 0) ? 0 : 1)) * IntervalInAmperes;
            }
        }


        void DrawText(string text, Point location,
            HorizontalAlignment halign, VerticalAlignment valign)
        {
            // Make the label.
            Label label = new Label();
            label.Content = text;
            canGraph.Children.Add(label);

            // Position the label.
            label.Measure(new Size(double.MaxValue, double.MaxValue));

            double x = location.X;
            if (halign == HorizontalAlignment.Center)
                x -= label.DesiredSize.Width / 2;
            else if (halign == HorizontalAlignment.Right)
                x -= label.DesiredSize.Width;
            Canvas.SetLeft(label, x);

            double y = location.Y;
            if (valign == VerticalAlignment.Center)
                y += label.DesiredSize.Height / 2;
            else if (valign == VerticalAlignment.Bottom)
                y += label.DesiredSize.Height;
					else y += label.DesiredSize.Height / 6;
            Canvas.SetTop(label, y);
        }


	}
		 
}
 