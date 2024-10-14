using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lKHM
{
	public class hatCopyRequest
	{
		public string name = "";
		public uint sourceID = 0xFFFFFFFF;
		public uint destinationID = 0xFFFFFFFF;
	}

	class HatTXTParser
	{
		private const string commentChars = "/#";

		private static string scrubUnquotedBlankChars(string stringIn)
		{
			string result = "";

			bool inQuote = false;
			bool doEscapeChar = false;
			foreach (char x in stringIn)
			{
				if (!(inQuote || doEscapeChar) && char.IsWhiteSpace(x)) continue;

				if (!doEscapeChar && x == '\"')
				{
					inQuote = !inQuote;
				}
				else if (inQuote || doEscapeChar || (x != '\"' && x != '\\'))
				{
					result += x;
					doEscapeChar = false;
				}
				else if (x == '\\')
				{
					doEscapeChar = true;
				}
			}

			return result;
		}

		public static void parseHatsFromTXT(string filepath, List<hatCopyRequest> destinationList)
		{
			if (System.IO.File.Exists(filepath))
			{
				string currentLine;
				System.IO.StreamReader fileIn = new System.IO.StreamReader(filepath);
				while ((currentLine = fileIn.ReadLine()) != null)
				{
					if (string.IsNullOrEmpty(currentLine)) continue;
					if (commentChars.Contains(currentLine[0])) continue;

					hatCopyRequest tempInfo = new hatCopyRequest();
					currentLine = scrubUnquotedBlankChars(currentLine);

					int equalsLoc = currentLine.IndexOf('=');
					if (equalsLoc != -1)
					{
						tempInfo.name = currentLine.Substring(0, equalsLoc);
						currentLine = currentLine.Substring(equalsLoc + 1);
					}

					int colonLoc = currentLine.IndexOf(':');
					if (colonLoc != -1)
					{
						tempInfo.destinationID = lKHM.Conversions.convertHexStringToNum(currentLine.Substring(0, colonLoc), uint.MaxValue);
						tempInfo.sourceID = lKHM.Conversions.convertHexStringToNum(currentLine.Substring(colonLoc + 1), uint.MaxValue);

						destinationList.Add(tempInfo);
					}
				}
			}
		}
		public static void importHatsFromTXT(KirbyHatManager managerIn, string filepath)
		{
			List<hatCopyRequest> incomingHats = new List<hatCopyRequest>();
			parseHatsFromTXT(filepath, incomingHats);

			foreach (hatCopyRequest currHat in incomingHats)
			{
				if (managerIn.copyHatToSlot(currHat.sourceID, currHat.destinationID, true))
				{
					HatNames.setFIDName(currHat.destinationID, currHat.name, true);
				}
			}
		}
		public static void importHatsFromTXTs(KirbyHatManager managerIn, string[] filepaths)
		{
			foreach (string currPath in filepaths)
			{
				importHatsFromTXT(managerIn, currPath);
			}
		}
	}
}
