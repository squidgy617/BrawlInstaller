using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lKHM
{
	public static class HatNames
	{
		enum LAVA_CHARA_FIGHTER_IDS
		{
			BOWSER = 0x0B,
			CAPTAIN_FALCON = 0x09,
			CHARIZARD = 0x1D,
			DEDEDE = 0x20,
			DIDDY_KONG = 0x1B,
			DONKEY_KONG = 0x01,
			FALCO = 0x13,
			FOX = 0x06,
			GANONDORF = 0x14,
			GIGA_BOWSER = 0x30,
			IKE = 0x22,
			IVYSAUR = 0x1F,
			JIGGLYPUFF = 0x25,
			KIRBY = 0x05,
			LINK = 0x02,
			LUCARIO = 0x21,
			LUCAS = 0x1A,
			LUIGI = 0x08,
			MARIO = 0x00,
			MARTH = 0x11,
			META_KNIGHT = 0x16,
			MR_GAME_AND_WATCH = 0x12,
			NANA = 0x10,
			NESS = 0x0A,
			OLIMAR = 0x19,
			PEACH = 0x0C,
			PIKACHU = 0x07,
			PIT = 0x17,
			POKEMON_TRAINER = 0x1C,
			POPO = 0x0F,
			ROB = 0x23,
			SAMUS = 0x03,
			SHEIK = 0x0E,
			SNAKE = 0x2E,
			SONIC = 0x2F,
			SOPO = 0x0F,
			SQUIRTLE = 0x1E,
			TOON_LINK = 0x29,
			WARIO = 0x15,
			WARIOMAN = 0x31,
			WOLF = 0x2C,
			YOSHI = 0x04,
			ZELDA = 0x0D,
			ZERO_SUIT_SAMUS = 0x18,
			RED_ALLOY = 0x32,
			BLUE_ALLOY = 0x33,
			YELLOW_ALLOY = 0x34,
			GREEN_ALLOY = 0x35,
			PM_MEWTWO = 0x26,
			PM_ROY = 0x27,
			PP_KNUCKLES = 0x2D,
			EX_RIDLEY = 0x2A,
			EX_WALUIGI = 0x28,
			EX_DARK_SAMUS = 0x40,
			EX_SCEPTILE = 0x62,
			EX_KRYSTAL = 0x41,
		}

		public static SortedDictionary<uint, string> fighterIDsToNames = new SortedDictionary<uint, string>();

		public static void populateFromInternalList()
		{
			fighterIDsToNames.Clear();

			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.CAPTAIN_FALCON] = "CAPTAIN";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.DEDEDE] = "DEDEDE";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.DIDDY_KONG] = "DIDDY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.DONKEY_KONG] = "DONKEY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.FALCO] = "FALCO";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.FOX] = "FOX";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.GANONDORF] = "GANON";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.MR_GAME_AND_WATCH] = "GAMEWATCH";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.GIGA_BOWSER] = "GKOOPA";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.IKE] = "IKE";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.KIRBY] = "KIRBY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.BOWSER] = "KOOPA";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.LINK] = "LINK";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.LUCARIO] = "LUCARIO";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.LUCAS] = "LUCAS";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.LUIGI] = "LUIGI";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.MARIO] = "MARIO";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.MARTH] = "MARTH";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.META_KNIGHT] = "METAKNIGHT";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.NANA] = "NANA";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.NESS] = "NESS";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.PEACH] = "PEACH";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.PIKACHU] = "PIKACHU";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.OLIMAR] = "PIKMIN";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.PIT] = "PIT";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.IVYSAUR] = "POKEFUSHIGISOU";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.CHARIZARD] = "POKELIZARDON";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.POKEMON_TRAINER] = "POKETRAINER";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.SQUIRTLE] = "POKEZENIGAME";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.POPO] = "POPO";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.JIGGLYPUFF] = "PURIN";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.ROB] = "ROBOT";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.SAMUS] = "SAMUS";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.SHEIK] = "SHEIK";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.SNAKE] = "SNAKE";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.SONIC] = "SONIC";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.SOPO] = "SOPO";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.ZERO_SUIT_SAMUS] = "SZEROSUIT";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.TOON_LINK] = "TOONLINK";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.WARIO] = "WARIO";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.WARIOMAN] = "WARIOMAN";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.WOLF] = "WOLF";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.YOSHI] = "YOSHI";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.ZELDA] = "ZELDA";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.RED_ALLOY] = "RED_ALLOY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.BLUE_ALLOY] = "BLUE_ALLOY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.YELLOW_ALLOY] = "YELLOW_ALLOY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.GREEN_ALLOY] = "GREEN_ALLOY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.PM_MEWTWO] = "PM_MEWTWO";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.PM_ROY] = "PM_ROY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.PP_KNUCKLES] = "P+_KNUCKLES";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.EX_RIDLEY] = "EX_RIDLEY";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.EX_WALUIGI] = "EX_WALUIGI";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.EX_DARK_SAMUS] = "EX_DARK_SAMUS";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.EX_SCEPTILE] = "EX_SCEPTILE";
			fighterIDsToNames[(uint)LAVA_CHARA_FIGHTER_IDS.EX_KRYSTAL] = "EX_KRYSTAL";
		}
		public static bool tryPopulateFromFighterConfigFolder(string folderPath, bool includeInternalListNames)
		{
			bool result = false;

			if (System.IO.Directory.Exists(folderPath))
			{
				string[] collectedFiles = System.IO.Directory.GetFiles(folderPath, "Fighter??.dat");
				if (collectedFiles.Length > 0x00)
				{
					fighterIDsToNames.Clear();
					if (includeInternalListNames)
					{
						populateFromInternalList();
					}

					BrawlLib.SSBB.ResourceNodes.FCFGNode currentNode = new BrawlLib.SSBB.ResourceNodes.FCFGNode();
					foreach (string currentPath in collectedFiles)
					{
						string IDString = currentPath.Substring(folderPath.Length + 7, 2);
						uint currentFID = lKHM.Conversions.convertHexStringToNum(IDString, uint.MaxValue);
						if (currentFID < lKHM.KirbyHatManager.maxHatCount)
						{
							currentNode.Replace(currentPath);
							fighterIDsToNames[currentFID] = currentNode.FighterName.ToUpper();
						}
					}
					result = fighterIDsToNames.Count > 0;
				}
			}

			return result;
		}
		public static bool pruneNamesWithNoAssociatedHat(KirbyHatManager hatManagerIn)
		{
			bool result = false;

			uint[] toRemove = fighterIDsToNames.Keys.Except(hatManagerIn.fighterIDToInfoPacks.Keys).ToArray();
			foreach (uint currID in toRemove)
			{
				fighterIDsToNames.Remove(currID);
			}

			return result;
		}

		public static bool getFIDHasName(uint fighterIDIn)
		{
			return fighterIDsToNames.ContainsKey(fighterIDIn);
		}
		public static string getNameFromFID(uint fighterIDIn)
		{
			string result = "UNRECOGNIZED";

			if (getFIDHasName(fighterIDIn))
			{
				result = fighterIDsToNames[fighterIDIn];
			}

			return result;
		}
		public static bool eraseFIDName(uint fighterIDIn)
		{
			bool result = false;

			if (getFIDHasName(fighterIDIn))
			{
				fighterIDsToNames.Remove(fighterIDIn);
			}

			return result;
		}
		public static bool setFIDName(uint fighterIDIn, string nameIn, bool allowOverwrite)
		{
			bool result = false;

			if (allowOverwrite || !getFIDHasName(fighterIDIn))
			{
				if (!String.IsNullOrEmpty(nameIn))
				{
					fighterIDsToNames[fighterIDIn] = nameIn.ToUpper().Replace(" ", "_");
				}
				else
				{
					eraseFIDName(fighterIDIn);
				}
			}

			return result;
		}
		public static bool copyFIDName(uint sourceFighterID, uint destinationFighterID, bool allowOverwrite)
		{
			bool result = false;

			if (getFIDHasName(sourceFighterID))
			{
				result = setFIDName(destinationFighterID, getNameFromFID(sourceFighterID), allowOverwrite);
			}

			return result;
		}
		public static bool moveFIDName(uint sourceFighterID, uint destinationFighterID, bool allowOverwrite)
		{
			bool result = false;

			if (copyFIDName(sourceFighterID, destinationFighterID, allowOverwrite))
			{
				result = eraseFIDName(sourceFighterID);
			}

			return result;
		}

		static HatNames()
		{
			populateFromInternalList();
		}
	}
}
