//#define POLYLINE
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static System.Console;
using System.Windows.Input;
using static Constants;

partial class Scull_Furnaces_Main_Window
 {

	 Scull_Furnaces_AppMain_Class app = (Scull_Furnaces_AppMain_Class)Application.Current;
	 bool buttonState = false;
	 public TypeOfParameters _typeOfParameters = new TypeOfParameters();
	 public RemembranceOfFileName _remembranceOfFileName = new RemembranceOfFileName();
	 private PointCollection points = null;
	 //private  TextBox tbWithMaxYValue;
	 
	 public Scull_Furnaces_Main_Window()
	 {
		this.DataContext = _typeOfParameters;
		//this.begTimeOnXAxis.clockWatch.Ticks = app.mySettings.TimeLowerBoundaryForTheCurrent;
		//this.endTimeOnXAxis.clockWatch.Ticks = app.mySettings.TimeUpperBoundaryForTheCurrent;


	 }
	void OnClosing(object o,EventArgs ea)
	 {
		 app.mySettings.TimeLowerBoundaryForTheCurrent=Int32.Parse(this.begTimeOnXAxis.Ticks.Text);
		 app.mySettings.TimeUpperBoundaryForTheCurrent=Int32.Parse(this.endTimeOnXAxis.Ticks.Text);
		 app.mySettings.Save();
		 
	 }

	 void click_b(object o,EventArgs ea)
	 {
		 app.show_number_button.Content = (Int32.Parse(app.show_number_button.Content.ToString()))+1;
		 _typeOfParameters.theNumberOfTab = _typeOfParameters.theNumberOfTab == 1 ? 0:1;
	 }

	 private void moveUp(object sender, RoutedEventArgs e)
	 {
		((ColorSource)Constants.ParameterData[2071].colorBinding.Source).parameterState = ((ColorSource)Constants.ParameterData[2071].colorBinding.Source).parameterState== 0 ? 1:0;
		TimeMover.numberOfSecond++;
	 }

	 private void moveDown(object sender, RoutedEventArgs e)
	 {	
		TimeMover.numberOfSecond--;
	 }

	 private void filePickerButton_Click(object sender, RoutedEventArgs e)
        {
            // Create the OpenFIleDialog object
            Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();
            
            // Add file filters
             openPicker.DefaultExt = ".911";
            openPicker.Filter = "Файлы дискретных и аналоговых данных с гарнисажных печей|*.911";
            
            // Display the OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openPicker.ShowDialog();

            // Check to see if we have a result 
            if (result == true)
            {
                // Application now has read/write access to the picked file
                // I am saving the file path to a textbox in the UI to display to the user
                // as well as a Globals.fileName variable to pass to a method
                WriteLine("Имя прежде выбранного файла : {0} ",Globals.fileName);
				filePathTextBox.Text = openPicker.FileName.ToString();
                Globals.fileName = filePathTextBox.Text;
				//проверка содержимого файла,его первого блока
				WriteLine(Globals.fileName);
				Globals.readAllTheBytes(Globals.fileName);
				ReadParamFromDisk packedParameters = new ReadParamFromDisk(Globals.fileName); //повторное чтение файла параметров
				//в память с инструментарием для работы с ним.
				app.unpackedParameters = new TickParamsAll(packedParameters); //получение распакованного массива
				this._remembranceOfFileName.fileName = Globals.fileName ;
				TimeMover.numberOfSecond = TimeMover.numberOfSecond;
				voltagePlotting(ParameterName.Напряжение_дуги,voltageGraph);
				currentPlotting(ParameterName.Ток_общ,currentGraph);
				vacuumPlotting(ParameterName.Вакуум,vacuumGraph);
				waterPlotting(ParameterName.Расход_воды,waterGraph);
            }
         }
	 private void voltageGraph_Loaded(object sender, RoutedEventArgs e)
	 {	
		
		voltagePlotting(ParameterName.Напряжение_дуги,voltageGraph);
	 }
 	 private void currentGraph_Loaded(object sender, RoutedEventArgs e)
	 {	
		WriteLine("currentGraph_Loaded");
		currentPlotting(ParameterName.Ток_общ,currentGraph);
	 }
 	 private void vacuumGraph_Loaded(object sender, RoutedEventArgs e)
	 {	
		vacuumPlotting(ParameterName.Вакуум,vacuumGraph);
	 }
 	 private void waterGraph_Loaded(object sender, RoutedEventArgs e)
	 {	
		waterPlotting(ParameterName.Расход_воды,waterGraph);
	 }
	 private void setMaxYValue(object sender, EventArgs e)
	 {	
		if(e is KeyEventArgs && (e as KeyEventArgs).Key != Key.Return)return;
		
		WriteLine("tbWithMaxYValue = {0}",maxValueForCurrentOnYAxis);
		if(app.mySettings.UpperLimitForCurrent != maxValueForCurrentOnYAxis.Text)
		{
			app.mySettings.UpperLimitForCurrent = maxValueForCurrentOnYAxis.Text;
			app.mySettings.Save();
			currentGraph.Children.Clear();
			currentPlotting(ParameterName.Ток_общ,currentGraph);

		
		}
	 }
	  public void setMinTimeValue(AlarmEventArgs aea)
	 {	
			currentGraph.Children.Clear();
			currentPlotting(ParameterName.Ток_общ,currentGraph,aea);

	 }
	  public void setMaxTimeValue(AlarmEventArgs aea)
	 {	
			currentGraph.Children.Clear();
			currentPlotting(ParameterName.Ток_общ,currentGraph,null,aea);

	 }


 }

 class MyUniformGrid:UniformGrid{}
 