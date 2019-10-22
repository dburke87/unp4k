using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace extract
{
	public class CopyDir
	{
		public static void Copy(string sourceDirectory, string targetDirectory)
		{
			DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
			DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

			CopyAll(diSource, diTarget);
		}

		private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
			Directory.CreateDirectory(target.FullName);

			// Copy each file into the new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}

		public static void MoveDirectoryContents(string sourceDirectory, string targetDirectory)
		{
			DirectoryInfo source = new DirectoryInfo(sourceDirectory);

			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{				
				diSourceSubDir.MoveTo(Path.Combine(targetDirectory, diSourceSubDir.Name));
			}
		}

		public static void ExtractDirectory(string sourceDirectory, bool subdirectories = true, bool deleteArchive = false)
		{
			if (!Directory.Exists(sourceDirectory))
				throw new Exception("Directory does not exist.");

			DirectoryInfo source = new DirectoryInfo(sourceDirectory);

			ExtractDirectory(source, subdirectories, deleteArchive);
		}

		private static void ExtractDirectory(DirectoryInfo source, bool subdirectories = true, bool deleteArchive = false)
		{
			// decompress all files in directory and subdirectory
			foreach (FileInfo fi in source.GetFiles("*.socpak", SearchOption.AllDirectories))
			{
				Console.WriteLine(@"Decompressing {0}", fi.FullName);
				ExtractFile(fi.FullName);
				if (deleteArchive)
					fi.Delete();
			}

			// use recursion to search through subdirectories, including new ones from decompression
			if (subdirectories)
			{
				foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
				{
					ExtractDirectory(diSourceSubDir, subdirectories, deleteArchive);
				}
			}
		}

		private static void ExtractFile(string path)
		{
			if (File.Exists(path))
			{
				string directory = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

				try
				{
					ZipFile.ExtractToDirectory(path, directory/*, true*/);
				}
				catch (PathTooLongException e)
				{

				}
				//catch (IOException) { }
			}
			else
			{
				throw new Exception("File does not exist.");
			}
		}

		//public static void Copy2(string sourceDirectory, string targetDirectory)
		//{
		//	//Now Create all of the directories
		//	foreach (string dirPath in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
		//	{
		//		Directory.CreateDirectory(dirPath.Replace(sourceDirectory, targetDirectory));
		//	}				

		//	//Copy all the files & Replaces any files with the same name
		//	foreach (string newPath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
		//	{
		//		File.Copy(newPath, newPath.Replace(sourceDirectory, targetDirectory), true);
		//	}				
		//}
	}
}
