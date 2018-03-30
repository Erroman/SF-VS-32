 using System;
 using System.ComponentModel;
 using System.Runtime.CompilerServices;
 namespace System.Runtime.CompilerServices{sealed class CallerMemberNameAttribute:Attribute{}}
 public class AlarmEventArgs:EventArgs
 {
	 public int TicksToAlarm
	 {
		 get;
		 set;
	 }
 }

 public class ClockWatch:INotifyPropertyChanged
 {
	 public bool Alarm_On = true;
	 public event Action<AlarmEventArgs> AlarmProcedure;

	 public event PropertyChangedEventHandler PropertyChanged;
	 private int _hours;
	 private int _minutes;
	 private int _seconds;
	 private int _ticks;
	 private bool externalCorrection = true;
	 private void OnPropertyChanged([CallerMemberName]string propertyName=null)
	 {
		 //Tick calculation
		 if(propertyName=="Ticks" & externalCorrection)
		 {
			externalCorrection = false; 
			Hours = TimeSpan.FromSeconds(_ticks).Hours;
			externalCorrection = false;
			Minutes = TimeSpan.FromSeconds(_ticks).Minutes;
			externalCorrection = false;
			Seconds = TimeSpan.FromSeconds(_ticks).Seconds;
		 }
		 else
		 {
		    if(externalCorrection)
			Ticks=(int)(new TimeSpan(Hours,Minutes,Seconds)).TotalSeconds;
			externalCorrection = true;
		 }
		 PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
		 
	 }
	 public  int  Hours
	 {
		 get
		 {
			 return _hours;
		 }
		 set
		 {
			 if(value<24 & value>=0)
			 {
			 _hours=value;
			 OnPropertyChanged();
			 }
		 }
	 }
	 public int Minutes
	 {
		 get
		 {
			 return _minutes;
		 }
		 set
		 {
			 if(value<60 & value>=0)
			 {
			 _minutes=value;
			 OnPropertyChanged();
			 }
		 }
	 }
	 public int Seconds
	 {
		 get
		 {
			 return _seconds;
		 }
		 set
		 {
			 if(value<60 & value>=0)
			 {
			 _seconds=value;
			 OnPropertyChanged();
			 }
		 }
	 } 
	 public int Ticks
	 {
		 get
		 {
			 //count ticks!
			 return _ticks;
		 }
		 set
		 {
			 if(value<24*60*60 & value>=0)
			 {
			 _ticks=value;
			 OnPropertyChanged();
 			if(Alarm_On)
				if(AlarmProcedure!=null)
			AlarmProcedure(new AlarmEventArgs(){TicksToAlarm=_ticks});

			 }
		 }
	  }

 }