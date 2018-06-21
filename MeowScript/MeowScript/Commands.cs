using MeowDSIO.DataTypes.LUAGNL;
using MeowDSIO.DataTypes.LUAINFO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MeowScript
{
	public static class Commands
	{
		private static INI ConfigIni;

		private static string ConfigININame => Utils.Frankenpath(Utils.AssemblyDirectory, "\\MeowScript_Config.ini");

		private static string DarkSoulsDataPath => ConfigIni["General", "DarkSoulsDataPath"];

		static Commands()
		{
			if (!File.Exists(ConfigININame))
			{
				using (Stream stream = typeof(Commands).Assembly.GetManifestResourceStream(string.Format("{0}.MeowScript_Config.ini", "MeowScript")))
				{
					using (FileStream fs = File.OpenWrite(ConfigININame))
					{
						stream.CopyTo(fs);
					}
				}
			}
			string iniString = File.ReadAllText(ConfigININame);
			ConfigIni = INI.Parse(iniString);
		}

		public static bool Build(string[] args)
		{
			if (args.Length == 0)
			{
				return false;
			}
			ValueTuple<int, string, string, bool, bool> luainfoEntry;
			foreach (string inputFile in args)
			{
				string inputText = File.ReadAllText(inputFile);
				string[] inputLines = inputText.Split('\n');
				List<ValueTuple<string, string>> cur_PackageDestinations = new List<ValueTuple<string, string>>();
				List<ValueTuple<int, string, string, bool, bool>> cur_LUAINFO = new List<ValueTuple<int, string, string, bool, bool>>();
				IEnumerable<string> specialTokens = from x in inputLines
				select x.Trim() into x
				where x.StartsWith("--@")
				select x;
				foreach (string item in specialTokens)
				{
					int colonIndex = item.IndexOf(':');
					if (colonIndex == -1)
					{
						throw new Exception($"Invalid special token comment in lua script: \"{item}\"");
					}
					string tokenName = item.Substring("--@".Length, colonIndex - "--@".Length).ToLower();
					string[] tokenParams = (from x in item.Substring(colonIndex + 1).Split(',')
					select x.Trim()).ToArray();
					if (tokenName == "package")
					{
						cur_PackageDestinations.Add(new ValueTuple<string, string>(Utils.RemoveExtension(tokenParams[0], ".luabnd"), Utils.RemoveExtension(tokenParams[1], ".lua")));
					}
					else if (tokenName == "battle_goal")
					{
						cur_LUAINFO.Add(new ValueTuple<int, string, string, bool, bool>(int.Parse(tokenParams[0]), tokenParams[1], null, true, false));
					}
					else if (tokenName == "logic_goal")
					{
						cur_LUAINFO.Add(new ValueTuple<int, string, string, bool, bool>(int.Parse(tokenParams[0]), tokenParams[1], tokenParams[2], false, true));
					}
					else if (tokenName == "misc_goal")
					{
						cur_LUAINFO.Add(new ValueTuple<int, string, string, bool, bool>(int.Parse(tokenParams[0]), tokenParams[1], null, false, false));
					}
				}
				foreach (ValueTuple<string, string> item2 in cur_PackageDestinations)
				{
					LUABND luabnd = LUABND.Load(DarkSoulsDataPath, item2.Item1);
					List<string> compilationErrors = new List<string>();
					List<string> luaGnlCheckErrors = new List<string>();
					List<string> luaGnlEntries = new List<string>();
					luabnd.AddOrUpdateScript(item2.Item2, inputText, compilationErrors, luaGnlCheckErrors, luaGnlEntries);
					if (compilationErrors.Count > 0)
					{
						Console.WriteLine("COMPILE ERRORS:");
						foreach (string item3 in compilationErrors)
						{
							Console.Error.WriteLine(item3);
						}
						throw new Exception();
					}
					if (luaGnlCheckErrors.Count > 0)
					{
						Console.WriteLine("GNL CHECK ERRORS:");
						foreach (string item4 in luaGnlCheckErrors)
						{
							Console.Error.WriteLine(item4);
						}
						throw new Exception();
					}
					foreach (string item5 in luaGnlEntries)
					{
						if (!luabnd.GlobalVariableNames.Any((StringRef x) => x.Value == item5))
						{
							luabnd.GlobalVariableNames.Add(item5);
						}
					}
					foreach (ValueTuple<int, string, string, bool, bool> item6 in cur_LUAINFO)
					{
						luainfoEntry = item6;
						if (!luabnd.Goals.Any((Goal x) => x.ID == luainfoEntry.Item1))
						{
							luabnd.Goals.Add(new Goal
							{
								ID = luainfoEntry.Item1,
								IsBattleInterrupt = luainfoEntry.Item4,
								IsLogicInterrupt = luainfoEntry.Item5,
								Name = luainfoEntry.Item2,
								LogicInterruptName = luainfoEntry.Item3
							});
						}
					}
					LUABND.Save(luabnd, DarkSoulsDataPath, item2.Item1);
				}
			}
			return true;
		}
	}
}
