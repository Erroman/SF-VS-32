using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Constants;
class VisualHostForPlot:FrameworkElement
{
	private readonly VisualCollection _children;
	private readonly TickParamsAll unpackedParameters;
	private readonly ParameterName parameterName;
	private readonly Rect rectBounds;
	public VisualHostForPlot(TickParamsAll unpackedParameters,ParameterName parameterName,Rect rect)
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

		DrawingVisual drawingVisual = new DrawingVisual();
		DrawingContext drawingContext = drawingVisual.RenderOpen();
		Pen pen = new Pen(Brushes.Blue,1.0);
		int numSec = 0;
		short    intAssembled;
		double realValueOfFunction;
		double peakAtInterval = Int32.MinValue;   //максимальное значение функции на интервале
		double bottomAtInterval = Int32.MaxValue; //минимальное значение функции на интервале
		Point begPoint = new Point(xmin,ymax);
		Point endPoint = new Point(xmin,ymax);
		for(
		int iParam = (int)parameterName/(Constants.ParameterData[(int)(parameterName)].parameterType == ParameterType.аналоговый ? 1:10)+(int)Globals.startOfParameterOutput.TotalSeconds*Constants.ParamsBlockLengthInBytes;     
		iParam < this.unpackedParameters.inflatedParameters.Length;
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
//				points.Add( new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*peakAtInterval));
				//points.Add( new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*bottomAtInterval));

				//endPoint = new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*peakAtInterval);
				//drawingContext.DrawLine(pen,begPoint,endPoint);
				endPoint = new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*(bottomAtInterval+peakAtInterval)/2);
				drawingContext.DrawLine(pen,begPoint,endPoint);

				begPoint = new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*bottomAtInterval);
				endPoint = new Point(xmin +(numSec)*dotsPerSecond,ymax - dotsPerVolt*peakAtInterval);
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