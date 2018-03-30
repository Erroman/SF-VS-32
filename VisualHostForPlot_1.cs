using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Constants;
using static System.Console;
using static TransformWorldToScreen;
class VisualHostForPlot_1:FrameworkElement
{
	private static Scull_Furnaces_Main_Window scull_Furnaces_Main_Window = ((Scull_Furnaces_AppMain_Class)Application.Current).scull_Furnaces_Main_Window;
	private readonly VisualCollection _children;
	private readonly TickParamsAll unpackedParameters;
	private readonly ParameterName parameterName;
	private readonly Rect rectBounds;
	public VisualHostForPlot_1(TickParamsAll unpackedParameters,ParameterName parameterName,Rect rect)
    {
		//Основные параметры для рисования графика:
		this.unpackedParameters = unpackedParameters; //массив, содержащий исходные данные
		this.parameterName=parameterName; //выбранный параметр для построения графика
		this.rectBounds = rect;           //прямоугольник,ограничивающий место на экране для графика
		//внутри уже нарисованных осей координат
		_children = new VisualCollection(this)
		{
			CreateDrawingVisualPlot()
		};
	}
	private DrawingVisual CreateDrawingVisualPlot()
	{
		const double dashTickLength = 10;
		double xmin = rectBounds.X;
		double xmax = rectBounds.Width;
		double ymin = rectBounds.X;
		double ymax = rectBounds.Height;
		double LowerLimitForCurrentOnYAxis = 0;
		double UpperLimitForCurrentOnYAxis = Convert.ToDouble(scull_Furnaces_Main_Window.maxValueForCurrentOnYAxis.Text);
		
		double LowerLimitForTimeOnXAxis = Int32.Parse(scull_Furnaces_Main_Window.begTimeOnXAxis.Ticks.Text);
		double UpperLimitForTimeOnXAxis = Int32.Parse(scull_Furnaces_Main_Window.endTimeOnXAxis.Ticks.Text);
		WriteLine("LowerLimitForTimeOnXAxis = {0}",LowerLimitForTimeOnXAxis);
		WriteLine("UpperLimitForTimeOnXAxis = {0}",UpperLimitForTimeOnXAxis);
		double step = Math.Round((xmax - xmin)/(24*6));
		PrepareTransformations
		(
		LowerLimitForTimeOnXAxis,UpperLimitForTimeOnXAxis,
		LowerLimitForCurrentOnYAxis,UpperLimitForCurrentOnYAxis,
		xmin,xmax,
		ymin,ymax
		);
		string unitOfMeasure;
		// double dotsPerSecond = (xmax - xmin)/SecondsInADay;
		// int intSecondsPerDot = (int)(SecondsInADay/(xmax - xmin));
		double dotsPerSecond = (xmax - xmin)/(UpperLimitForTimeOnXAxis-LowerLimitForTimeOnXAxis);
		int intSecondsPerDot = (int)((UpperLimitForTimeOnXAxis-LowerLimitForCurrentOnYAxis)/(xmax - xmin));
		
		double dotsPerVolt = UpperLimitForCurrentOnYAxis*step/UpperLimitForCurrentOnYAxis;
		dotsPerVolt = (ymax-ymin)/(UpperLimitForCurrentOnYAxis - LowerLimitForCurrentOnYAxis);
		DrawingVisual drawingVisual = new DrawingVisual();
		DrawingContext drawingContext = drawingVisual.RenderOpen();
		Pen pen = new Pen(Brushes.Blue,1.0);
		int numSec = 0;
		short    intAssembled;
		double realValueOfFunction;
		double peakAtInterval = Int32.MinValue;   //максимальное значение функции на интервале
		double bottomAtInterval = Int32.MaxValue; //минимальное значение функции на интервале
		Point begPoint = WtoD(new Point(xmin,ymax)); //What should be put here?!
		Point endPoint; // = WtoD(new Point(xmin,ymax));
		WriteLine("LowerLimitForCurrentOnYAxis = {0}",LowerLimitForCurrentOnYAxis);
		int iParam;
		for	
		(
		int currSec = (int)LowerLimitForCurrentOnYAxis;
		currSec < (UpperLimitForTimeOnXAxis-LowerLimitForCurrentOnYAxis);
		currSec ++
		)	
		{
			iParam = (int)parameterName + currSec * Constants.ParamsBlockLengthInBytes;
			
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
			if(numSec%intSecondsPerDot==0)  //intSecondsPerDot обращается в 0!
			{
				//отмечать в графике МАКСИМАЛЬЕОЕ значение на интервале
				//но надо отмечать и МИНИМАЛЬНОЕ !

				endPoint = WtoD(new Point(currSec,(bottomAtInterval+peakAtInterval)/2));
				drawingContext.DrawLine(pen,begPoint,endPoint);

				begPoint = WtoD(new Point(currSec,bottomAtInterval));
				endPoint = WtoD(new Point(currSec,peakAtInterval));
				drawingContext.DrawLine(pen,begPoint,endPoint);

				begPoint = new Point((begPoint.X+endPoint.X)/2,(begPoint.Y+endPoint.Y)/2);

				peakAtInterval = 0;
				bottomAtInterval = Int32.MaxValue;
			}
			numSec++;	
			//if(numSec>15000)break;		
	
		}

		drawingContext.Close();
		return drawingVisual;
	}
 // Provide a required override for the VisualChildrenCount property.
	protected override int VisualChildrenCount => _children.Count;

	// Provide a required override for the GetVisualChild method.
	protected override Visual GetVisualChild(int index)
	{
		if (index < 0 || index >= _children.Count)
		{
			throw new ArgumentOutOfRangeException();
		}

		return _children[index];
	}
}	