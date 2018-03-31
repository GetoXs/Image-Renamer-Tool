using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileRenamer
{
	class Program
	{
		static void Main(string[] args)
		{
			FileRenamerModule();
			//ImageTakenSetterModule();
		}
		static void ImageTakenSetterModule()
		{
			string folderIn = @"Z:\Zdjecia\2015-08 Azjatycka przygoda\Wybrane zdjecia\2015-09-03 Szanghaj";

			var files = Directory.GetFiles(folderIn, "*.jpg", SearchOption.AllDirectories);
			foreach (var file in files)
			{
				FileInfo fi = new FileInfo(file);

				var dateTakenStr = fi.Name.Substring(0, 18);
				DateTime dateTaken;
				if (DateTime.TryParseExact(dateTakenStr, "yyyy-MM-dd HHmm.ss", null, System.Globalization.DateTimeStyles.None, out dateTaken))
				{
					SetDateTaken(file, dateTaken);
				}
			}
		}

		static void FileRenamerModule()
		{
			string postfix = "GoPro";

			string folderIn = @"Z:\Zdjecia\2015-08 Azjatycka przygoda\Wybrane zdjecia\test";
			double hourOffset = -2.5;
			int monthOffset = 0;

			var files = Directory.GetFiles(folderIn, "*.jpg", SearchOption.AllDirectories);

			foreach (var file in files)
			{
				FileInfo fi = new FileInfo(file);

				string newFileName;
				if ((fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				{
					var dateTaken = GetDateTakenFromImage(file);
					dateTaken = dateTaken.AddHours(hourOffset);
					dateTaken = dateTaken.AddMonths(monthOffset);

					newFileName = Path.Combine(Path.GetDirectoryName(file), dateTaken.ToString("yyyy-MM-dd HHmm.ss") + " " + postfix + ".jpg");

					if (file != newFileName)
					{
						int i = 1;
						while (File.Exists(newFileName))
						{
							newFileName = Path.Combine(Path.GetDirectoryName(file), dateTaken.ToString("yyyy-MM-dd HHmm.ss") + " " + i++ + " " + postfix + ".jpg");
						}
						Debug.Assert(!File.Exists(newFileName));
						//rename
						File.Move(file, newFileName);
					}
				}
			}
		}

		/// <summary>
		/// We init this once so that if the function is repeatedly called
		/// It isn't stressing the garbage man
		/// </summary>
		private static Regex r = new Regex(":");

		/// <summary>
		/// Retrieves the datetime WITHOUT loading the whole image
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DateTime GetDateTakenFromImage(string path)
		{
			using (Image theImage = new Bitmap(path))
			{
				PropertyItem[] propItems = theImage.PropertyItems;
				Encoding _Encoding = Encoding.UTF8;
				var DataTakenProperty1 = propItems.Where(a => a.Id.ToString("x") == "9004").FirstOrDefault();
				var DataTakenProperty2 = propItems.Where(a => a.Id.ToString("x") == "9003").FirstOrDefault();
				string originalDateString = _Encoding.GetString(DataTakenProperty1.Value);
				originalDateString = originalDateString.Remove(originalDateString.Length - 1);
				DateTime originalDate = DateTime.ParseExact(originalDateString, "yyyy:MM:dd HH:mm:ss", null);
				return originalDate;
			}
		}
		static void SetDateTaken(string path, DateTime dateTaken)
		{
			string new_path = System.IO.Path.GetDirectoryName(path) + "\\_" + System.IO.Path.GetFileName(path);
			using (Image image = new Bitmap(path))
			{
				PropertyItem[] propItems = image.PropertyItems;
				Encoding _Encoding = Encoding.UTF8;
				var DataTakenProperty1 = propItems.Where(a => a.Id.ToString("x") == "9004").FirstOrDefault();
				var DataTakenProperty2 = propItems.Where(a => a.Id.ToString("x") == "9003").FirstOrDefault();
				string originalDateString = _Encoding.GetString(DataTakenProperty1.Value);
				originalDateString = originalDateString.Remove(originalDateString.Length - 1);
				DateTime originalDate = DateTime.ParseExact(originalDateString, "yyyy:MM:dd HH:mm:ss", null);

				originalDate = dateTaken;

				DataTakenProperty1.Value = _Encoding.GetBytes(originalDate.ToString("yyyy:MM:dd HH:mm:ss") + '\0');
				DataTakenProperty2.Value = _Encoding.GetBytes(originalDate.ToString("yyyy:MM:dd HH:mm:ss") + '\0');
				image.SetPropertyItem(DataTakenProperty1);
				image.SetPropertyItem(DataTakenProperty2);
				image.Save(new_path);
			}
			File.Delete(path);
			File.Move(new_path, path);
		}
	}
}
