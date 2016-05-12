using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileRenamer
{
	class Program
	{
		static void Main(string[] args)
		{
			string postfix = "Justyna";

			string folderIn = @"Z:\Zdjecia\2016-05 Rumunia czyli zwiedzanie z Drakula\Justyna";
			int hourOffset = 1;
			int monthOffset = -1;

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
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			using (Image myImage = Image.FromStream(fs, false, false))
			{
				PropertyItem propItem = myImage.GetPropertyItem(36867);
				string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
				return DateTime.Parse(dateTaken);
			}
		}

	}
}
