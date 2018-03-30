#define PRINT
using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Collections;

public class ReadParamFromDisk:IEnumerable
{
	
	private string FileName;
	private int FileLength;
	
	public byte[] readValues;

    public byte this[int byteNumber]=>readValues[byteNumber]; //indexer to access individual bytes in the read-from-the-disk array

	public ReadParamFromDisk(string FileName)
	{
		this.FileName = FileName;
//Считывание всего файла упакованных параметров в память		
		this.FileLength = readAllTheBytes(this.FileName);
//Проверка правильности первого блока в файле
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
				MessageBox.Show("Неверный первый блок параметров  в начале файла!");
			}
		
	}
	public IEnumerator GetEnumerator()
	{
		
		return (IEnumerator)new GoFromBlockToBlock(readValues);
		
	}
class GoFromBlockToBlock:IEnumerator
{
		private int FileLength;
		private byte[] readValues;
		public GoFromBlockToBlock(byte[] readValues)
		{
			this.readValues = readValues;
			this.FileLength = readValues.Length;
		}
	private int startIndexInArray =-1; // индекс массива у ячейки в начале очередного блока в файле,считанном с диска
	private int blockNumber = -1;
	//Признак конца блока: if(readValues[i]==0xFF && readValues[i+1]==0xFB)
	
		public bool MoveNext()
		{
		//пропускаем маркер конца блока,устанавливаем на первую ячейку следующего блока
			if(startIndexInArray==-1)
			{
				blockNumber++;
				startIndexInArray++;
				return true;
			}		
			else
			if(startIndexInArray==FileLength-1)
			{
				return false;
			}
			else
			{
				for(;startIndexInArray<this.FileLength-1;startIndexInArray++)
				{
					if(readValues[startIndexInArray]==0xFF && readValues[startIndexInArray+1]==0xFB)
					{
						if(FileLength - startIndexInArray >6 )
						{
							startIndexInArray+=6;
							return true;
						}
						else 
							return false;
					}	

				}
				
			}
//      	Console.WriteLine("The file traversed! {0}",this.startIndexInArray);
			return false;
		}
	public object Current => startIndexInArray;
	public void Reset(){startIndexInArray=-1;}
	
}
	private int readAllTheBytes(string fileName) //возвращает длину массива в памяти, считанного с диска
	{
		// Open the file
		using (var stream = new FileStream(fileName, FileMode.Open))
		using (var reader = new BinaryReader(stream))
		{
		// Compute how many values the file has

		var numValues = stream.Length;
	 
		// Allocate an array to hold all those values
		readValues = new byte[numValues];
	 
		// Open a reader to make reading those values easy
			// Read all the values
			for (var i = 0; i < numValues; ++i)
			{
				readValues[i] = reader.ReadByte();
			}
		return (int)numValues;
		}
		
	}
	
}

public class TickParamsAll
{
	

	
	private ReadParamFromDisk packedParameters; 
	
	/*packedParameters это адрес считанного с диска набора блоков,
	  оформленного в виде перечисляемой коллекции типа ReadParamFromDisk,удобной для распаковки
	  с помощью метода inflate(),который помещает адрес массива полностью распакованных параметров
	  в поле inflatedParameters*/

	  public byte[]  inflatedParameters;	

	
	public  TickParamsAll(ReadParamFromDisk packedParameters) //ReadParamFromDisk is a custom 
	//collection of packed blocks, so we can traverse it and inflate it into an array.
	{
		this.packedParameters   = packedParameters;
		this.inflatedParameters = new byte[Constants.SecondsInADay*Constants.ParamsBlockLengthInBytes];
		
		inflate();
	}
	void inflate()
	{
		int NumberOfChangedBytes=0;
		//MemoryStream ms;
		int endOfBlockIndex=0; //индекс последней ячейки блока упакованных параметров
		int blockNumber =0; //номер блока параметров,должен совпадать с номером секунды с начала суток,
		//истекшей с начала суток в момент регистрации праметров минус единица,поскольку нумерация блоков начинается с нуля, а секунд с единицы.
		IEnumerator enu = packedParameters.GetEnumerator();
		enu.MoveNext();enu.MoveNext();
		foreach(int startIndexForBlock in packedParameters)
		{
			//Console.WriteLine("startIndexForBlock = {0}",startIndexForBlock);
			
			if(blockNumber == 0)
			{
				//копируем первый (с нулевым номером) блок в начало распаковываемого массива
				//в нём содержаться значения всех параметров
				for(int i = 0;i<Constants.ParamsBlockLengthInBytes;i++)
				{
					this.inflatedParameters[i]=packedParameters[2*i+1];
	//				Console.WriteLine("Первый блок : Байт № {0} = {1}",i,this.inflatedParameters[i]);
				}
	//			enu.MoveNext();	
			}
			else 
			{
				//копируем предыдущий блок,то есть уже распакованную часть массива inflatedParameters за прошлый интервал(прошлую секунду)	в его же как заготовку для новой части за текущий интервал (текущую секунду).
				//
				for(int i=(blockNumber-1)*Constants.ParamsBlockLengthInBytes;i<blockNumber*Constants.ParamsBlockLengthInBytes;i++)
				{
					this.inflatedParameters[i+Constants.ParamsBlockLengthInBytes]=this.inflatedParameters[i];
				//	Console.WriteLine("Блок № {0}: Байт № {1} = {2}",blockNumber,i%Constants.ParamsBlockLengthInBytes,this.inflatedParameters[i+Constants.ParamsBlockLengthInBytes]);
				}
			
				//определяем длину блока изменившихся параметров,проверяем его на корректность,если блок некорректен,оставляем значения всех параметров прежними
				//проверка корректности заключается в следующем : количество байтов в блоке должно быть 
				//чётным,не превышать максимального,номера изменившихся байтов должны идти в  возрастающем порядке, и лежать в пределах от 0 до максимального номера.
				if(enu.MoveNext())endOfBlockIndex=(int)enu.Current-7;
				else
				{
					if(this.packedParameters.readValues[this.packedParameters.readValues.Length-1]!=0xff)throw new Exception();
					endOfBlockIndex = this.packedParameters.readValues.Length-2;
				}	
		
				try
				{
					if((endOfBlockIndex + 1 - startIndexForBlock)%2!=0)throw new Exception();
					NumberOfChangedBytes = (endOfBlockIndex + 1 - startIndexForBlock)/2;

					if(NumberOfChangedBytes>Constants.ParamsBlockLengthInBytes)throw new Exception();
				

					for(int changedByteIndex=startIndexForBlock;changedByteIndex<endOfBlockIndex;changedByteIndex+=2)
					{
						this.inflatedParameters[blockNumber*Constants.ParamsBlockLengthInBytes+this.packedParameters.readValues[changedByteIndex]]=this.packedParameters.readValues[changedByteIndex+1];
					}
				}
				catch(Exception e)
				{
					Console.WriteLine("Блок № {0} значений изменившихся параметров некорректен!",blockNumber);
					goto NextPackedBlock;
				}
			}
				/*	;			
				//Console.WriteLine("Number of changed bytes = {0}",NumberOfChangedBytes);}

				//
				for(int changedByteIndex=startIndexForBlock;changedByteIndex<=endOfBlockIndex;changedByteIndex+=2)
				{
					inflatedParameters[blockNumber*packedParameters[changedByteIndex]] = packedParameters[changedByteIndex+1];
					//Console.WriteLine("blockNumber,changedByteNumber {0} {1}",blockNumber,changedByteNumber);
				}
			}*/
			NextPackedBlock:
			blockNumber++;
		}
			
	}
		
		
}
