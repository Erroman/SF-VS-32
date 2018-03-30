using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using static System.Console;
using static Constants;
public enum TimeUnit
{
	second,
	minute,
	hour
}
public static class TimeMover
{
	private static Scull_Furnaces_Main_Window scull_Furnaces_Main_Window = ((Scull_Furnaces_AppMain_Class)Application.Current).scull_Furnaces_Main_Window;
	private static TickParamsAll unpackedParameters;
	private static int _numberOfSecond = 1;
	private static int _Seconds = 0;
	private static int _Minutes = 0;
	private static int _Hours   = 0;
	
	
	public static event EventHandler numberOfSecondChanged;
	
	public static int numberOfSecond 
	{
		get
		{
			return _numberOfSecond;
		}
		set
		{
			if(value>0 & value<=SecondsInADay)
			{
				_numberOfSecond = value;
				foreach(var buttonControl in scull_Furnaces_Main_Window.uniGrid.Children)
				{
					//по номеру секунды и номеру параметра выбираем нужный байт в памяти и маску для него
					//1. Байт в памяти,содержащий значание данного дискретного параметра с номером (ключём)
					//((DiscretePlaque)buttonControl).discreteNumber
					//WriteLine(((DiscretePlaque)buttonControl).discreteNumber/10);
					//WriteLine(ParameterData[((DiscretePlaque)buttonControl).discreteNumber].parameterName);
					unpackedParameters = ((Scull_Furnaces_AppMain_Class)Application.Current).unpackedParameters;
					byte byteWithDiscretes = unpackedParameters.inflatedParameters[(_numberOfSecond - 1)*Constants.ParamsBlockLengthInBytes +((DiscretePlaque)buttonControl).discreteNumber/10];
					((ColorSource)Constants.ParameterData[((DiscretePlaque)buttonControl).discreteNumber].colorBinding.Source).parameterState = 
					(byte)Constants.ParameterData[((DiscretePlaque)buttonControl).discreteNumber].bitMask & byteWithDiscretes;    // !!!!!!!!!!!!!!!

				}	
				if(numberOfSecondChanged!=null)numberOfSecondChanged(null, EventArgs.Empty);
			}
		
		}
	}

}	
public class TimePresenter:IValueConverter
{
	private static Scull_Furnaces_Main_Window scull_Furnaces_Main_Window = ((Scull_Furnaces_AppMain_Class)Application.Current).scull_Furnaces_Main_Window;
	 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string timeUnit = parameter.ToString();
			int timeTicks = (int)value;
			switch(timeUnit)
			{
				case "Seconds":
				return TimeSpan.FromSeconds(timeTicks).Seconds;
				WriteLine("Convert: from seconds field:");
				WriteLine("Seconds {0}",timeTicks);
				WriteLine("Minutes {0}",scull_Furnaces_Main_Window.timeMin?.Text);
				WriteLine("Hours {0}",scull_Furnaces_Main_Window.timeHour?.Text);
				//return timeTicks;
				break;
				case "Minutes":
				return TimeSpan.FromSeconds(timeTicks).Minutes;
				WriteLine("Convert: from minutes field:");
				WriteLine("Seconds {0}",scull_Furnaces_Main_Window.timeSec?.Text);
				WriteLine("Minutes {0}",timeTicks);
				WriteLine("Hours {0}",scull_Furnaces_Main_Window.timeHour?.Text);
				
				//return timeTicks;
				
				break;
				case "Hours":
				return TimeSpan.FromSeconds(timeTicks).Hours;
				WriteLine("Convert: from hours field:");
				WriteLine("Seconds {0}",scull_Furnaces_Main_Window.timeSec?.Text);
				WriteLine("Minutes {0}",scull_Furnaces_Main_Window.timeMin?.Text);
				WriteLine("Hours {0}",timeTicks);

				//return timeTicks;
				
				break;
				default:
				WriteLine("No such unit!");
				break;
			}
            return null;
        }
    //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture){return null;}
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string timeUnit = parameter.ToString();
			//int timeTicks = (int)value;

			int SecondsFromMidnight; 
			int Hours;
			int Minutes;
			int Seconds;
			string timeUnit = parameter.ToString();
			if(Int32.TryParse(scull_Furnaces_Main_Window.textBox.Text,out SecondsFromMidnight))
			{	
				 //return SecondsFromMidnight; 
				//return TimeSpan.FromSeconds(SecondsFromMidnight).Seconds;
				WriteLine("ConvertBack parameter = {0}",timeUnit);
			switch(timeUnit)
			{
				case "Seconds":
		
				Int32.TryParse(scull_Furnaces_Main_Window.timeHour?.Text,out Hours);
				Int32.TryParse(scull_Furnaces_Main_Window.timeMin?.Text,out Minutes);
				Int32.TryParse(value.ToString(),out Seconds);
				return (Hours*3600+Minutes*60+Seconds);
				break;
				case "Minutes":

				Int32.TryParse(scull_Furnaces_Main_Window.timeHour?.Text,out Hours);
				Int32.TryParse(value.ToString(),out Minutes);
				Int32.TryParse(scull_Furnaces_Main_Window.timeSec?.Text,out Seconds);
				return (Hours*3600+Minutes*60+Seconds);
				break;
				case "Hours":

				Int32.TryParse(value.ToString(),out Hours);
				Int32.TryParse(scull_Furnaces_Main_Window.timeMin?.Text,out Minutes);
				Int32.TryParse(scull_Furnaces_Main_Window.timeSec?.Text,out Seconds);
				return (Hours*3600+Minutes*60+Seconds);
				break;
				default:
				WriteLine("No such unit!");
				break;
			}
				
				return null;
				
			}
			else 
				 return null;
        }
}

public class DiscretePlaque:Button
{
	 public int discreteNumber;
}