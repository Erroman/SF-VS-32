using System;
using System.ComponentModel;
public class TypeOfParameters:INotifyPropertyChanged
{
 public event PropertyChangedEventHandler PropertyChanged;
 void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("theNumberOfTab"));

        }
//свойство  theNumberOfTab привязывается к XAML коду на окне,чтобы отображать, и далее запоминать 
//на диске номер просматриваемой  вкладки 
//между сеансами работы
 int _theNumberOfTab = 1;
 public int theNumberOfTab
 {
  	get 
	{
		return _theNumberOfTab;
	}
	set 
	{
		 _theNumberOfTab = value;
		 OnPropertyChanged("theNumberOfTab");
	}
 }
}
