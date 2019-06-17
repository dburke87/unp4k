﻿using ICSharpCode.SharpZipLib.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Net.Http;

namespace unp4k
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0) args = new[] { @"Z:\3.5.1 LIVE.1835013\Data.p4k" };
			string dataFileDirectory = args[0].Substring(0, args[0].IndexOf("Data.p4k", StringComparison.InvariantCultureIgnoreCase));

			if (args.Length == 1) args = new[] { args[0], "*.*" };

			using (var pakFile = File.OpenRead(args[0]))
			{
				var pak = new ZipFile(pakFile) { Key = new Byte[] { 0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47 } };

				foreach (ZipEntry entry in pak)
				{
					try
					{
						var crypto = entry.IsAesCrypted ? "Crypt" : "Plain";

						if (args[1].StartsWith("*.")) args[1] = args[1].Substring(1);                                                                                           // Enable *.ext format for extensions

						//if (args[1] == ".*" ||                                                                                                                                 // Searching for everything
						//	args[1] == "*" ||                                                                                                                                   // Searching for everything
						//	entry.Name.ToLowerInvariant().Contains(args[1].ToLowerInvariant()) ||                                                                               // Searching for keywords / extensions
						//	(args[1].EndsWith("xml", StringComparison.InvariantCultureIgnoreCase) && entry.Name.EndsWith(".dcb", StringComparison.InvariantCultureIgnoreCase))) // Searching for XMLs - include game.dcb
						if (entry.Name.EndsWith(".dcb", StringComparison.InvariantCultureIgnoreCase) ||
							entry.Name.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) ||
							entry.Name.EndsWith(".ini", StringComparison.InvariantCultureIgnoreCase) ||
							entry.Name.EndsWith(".socpak", StringComparison.InvariantCultureIgnoreCase))
						{
							string fileName;
							if (entry.Name.EndsWith(".dcb", StringComparison.InvariantCultureIgnoreCase))
							{
								fileName = Path.Combine(dataFileDirectory, "dcb", entry.Name);
							}
							else
							{
								fileName = Path.Combine(dataFileDirectory, "non-dcb", entry.Name);
							}

							var target = new FileInfo(fileName);

							if (!target.Directory.Exists) target.Directory.Create();

							if (!target.Exists)
							{
								Console.WriteLine($"{entry.CompressionMethod} | {crypto} | {entry.Name}");

								using (Stream s = pak.GetInputStream(entry))
								{
									byte[] buf = new byte[4096];

									using (FileStream fs = File.Create(fileName))
									{
										StreamUtils.Copy(s, fs, buf);
									}
								}

								// target.Delete();
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Exception while extracting {entry.Name}: {ex.Message}");

						try
						{
							using (var client = new HttpClient { })
							{
								// var server = "http://herald.holoxplor.local";
								var server = "https://herald.holoxplor.space";

								client.DefaultRequestHeaders.Add("client", "unp4k");

								using (var content = new MultipartFormDataContent("UPLOAD----"))
								{
									content.Add(new StringContent($"{ex.Message}\r\n\r\n{ex.StackTrace}"), "exception", entry.Name);

									using (var errorReport = client.PostAsync($"{server}/p4k/exception/{entry.Name}", content).Result)
									{
										if (errorReport.StatusCode == System.Net.HttpStatusCode.OK)
										{
											Console.WriteLine("This exception has been reported.");
										}
									}
								}
							}
						}
						catch (Exception)
						{
							Console.WriteLine("There was a problem whilst attempting to report this error.");
						}
					}
				}
			}
		}
	}
}
