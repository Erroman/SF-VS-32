using System;
using static System.Console;
using System.Windows;
using System.IO;
class Globals
{
	public static string fileName = "20170808.911";
	public static byte[] readValues;
	public static TimeSpan startOfParameterOutput = TimeSpan.Zero;
	public static DateTime startTimeOfFileRecording; //We should obtain it from the date encoded in the name of file
	//and possibly the time of file creation. "Прикинутое" время начала записи в файл.
	public static TimeSpan deltaForStartingOfRecord= new TimeSpan(0,0,0);
	
	public static void swapBytes(ref short word_out)
	{
		int x,y;
		x = word_out<<8;
		y = word_out>>8;
		word_out = (short)(x | y);
	}
	public static bool checkTheHead()
	{
		try
			{
				int FileMarkerIndex=0;
				for(int i=0;i<Constants.ParamsBlockLengthInBytes;i++)
				{
					FileMarkerIndex = 2*i;
					if(i!=readValues[FileMarkerIndex])throw new Exception();
					//FileMarkerIndex++;
				}
				if(readValues[FileMarkerIndex+2]!=0xFF || readValues[FileMarkerIndex+3]!=0xFB)
					throw new Exception();
			}
		catch
			{
				MessageBox.Show("Неверная полная запись в начале файла!");
			}

		return true;
	}
	public static void readAllTheBytes(string fileName)
	{
		//переданное в качестве параметра имя файла должно совпадать с указанным в глобальной переменной
		try	{if(fileName!=Globals.fileName)throw new Exception("Something  wrong with the file's name!");}
		catch(Exception e){WriteLine(e);}

		using (var stream = new FileStream(fileName, FileMode.Open))
		using (var reader = new BinaryReader(stream))
		{
		// Compute how many values the file has
//		Console.WriteLine("Length of the Stream: {0}",stream.Length);
		var numValues = stream.Length;
	 
		// Allocate an array to hold all those values
		readValues = new byte[numValues];
	 //readValues = reader.ReadBytes(numValues); //прочитать всё сразу!
		// Open a reader to make reading those values easy
			// Read all the values
			for (var i = 0; i < numValues; ++i)
			{
				readValues[i] = reader.ReadByte();
			}

		}
		checkTheHead(); //проверка первого блока считанного файла
    }
}
