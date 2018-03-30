using System;
using System.ComponentModel;
public class RemembranceOfFileName:INotifyPropertyChanged
{
 public event PropertyChangedEventHandler PropertyChanged;
 void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("theNumberOfTab"));

        }
//Свойство  fileName меняется пользователем, чтобы запоминать на диске 
//между сеансами работы имя выбранного файла.
 string _fileName;
 public string fileName
 {
  	get 
	{
		return _fileName;
	}
	set 
	{
		 _fileName = value;
		 OnPropertyChanged("fileName");
	}
 }
}
