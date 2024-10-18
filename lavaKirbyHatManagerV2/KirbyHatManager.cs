using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.ComponentModel;

namespace lKHM
{
	internal static class Conversions
	{
		internal static string convertNumToHexString(uint numIn, uint paddingLength = 0x8)
		{
			return "0x" + numIn.ToString("X" + Math.Min(paddingLength, 0x8).ToString());
		}
		internal static uint convertHexStringToNum(string strIn, uint defaultValue)
		{
			uint result = defaultValue;

			string tempStr = strIn;
			uint tempConv = uint.MaxValue;
			if (tempStr.StartsWith("0x"))
			{
				tempStr = tempStr.Substring(2);
			}
			if (uint.TryParse(tempStr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out tempConv))
			{
				result = tempConv;
			}

			return result;
		}
	}

	public class WriteWordCmdTypeConverter : TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(typeof(writeWordCmd));
		}
	}
	[TypeConverter(typeof(WriteWordCmdTypeConverter))]
	public class writeWordCmd
	{
		internal uint _targetOffset = uint.MaxValue;
		internal byte _targetSection = byte.MaxValue;
		internal uint _targetModuleID = uint.MaxValue;

		internal bool targetModuleIDValid()
		{
			return BrawlLib.SSBB.ResourceNodes.RELNode._idNames.ContainsKey(_targetModuleID);
		}

		[TypeConverter(typeof(BrawlLib.Internal.DropDownListRELModuleIDs))]
		public string TargetModuleID
		{
			get => targetModuleIDValid() ? BrawlLib.SSBB.ResourceNodes.RELNode._idNames[_targetModuleID] : "---";
			set
			{
				if (BrawlLib.SSBB.ResourceNodes.RELNode._idNames.ContainsValue(value))
				{
					_targetModuleID = BrawlLib.SSBB.ResourceNodes.RELNode._idNames.Keys[BrawlLib.SSBB.ResourceNodes.RELNode._idNames.IndexOfValue(value)];
				}
				else
				{
					_targetModuleID = uint.MaxValue;
				}
			}
		}
		public string TargetOffset
		{
			get => (targetModuleIDValid()) ? Conversions.convertNumToHexString(_targetOffset) : "";
			set
			{
				if (targetModuleIDValid())
				{
					_targetOffset = Conversions.convertHexStringToNum(value, _targetOffset);
				}
			}
		}
		public string TargetSection
		{
			get => (targetModuleIDValid()) ? Conversions.convertNumToHexString(_targetSection, 0x2) : "";
			set
			{
				if (targetModuleIDValid())
				{
					_targetSection = (byte)Conversions.convertHexStringToNum(value, _targetSection);
				}
			}
		}

		public override string ToString()
		{
			return "";
		}
		public bool isValid()
		{
			return targetModuleIDValid() && 
				_targetSection != byte.MaxValue &&
				_targetOffset != uint.MaxValue;
		}
		public bool isSameAs(writeWordCmd sourceCmd)
		{
			bool result = _targetOffset == sourceCmd._targetOffset;
			result &= _targetSection == sourceCmd._targetSection;
			result &= _targetModuleID == sourceCmd._targetModuleID;

			return result;
		}
		public void copyInfoFrom(writeWordCmd sourceCmd)
		{
			_targetOffset = sourceCmd._targetOffset;
			_targetSection = sourceCmd._targetSection;
			_targetModuleID = sourceCmd._targetModuleID;
		}
		public string summaryString()
		{
			return "Write @ " + TargetOffset + " in " + TargetModuleID + "[" + TargetSection + "]" ;
		}
	}

	public class HatInfoPackTypeConverter : TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(typeof(HatInfoPack));
		}
	}
	[TypeConverter(typeof(HatInfoPackTypeConverter))]
	public class HatInfoPack
	{
		// AbilityTopStatusKind
		internal uint table1Entry = 0xFFFFFFFF;

		// AbilityProcesses (REL Command Offset Value)
		internal writeWordCmd table2Entry = new writeWordCmd();

		// AbilityInfos
		internal writeWordCmd table3Entry1 = new writeWordCmd();
		internal uint table3Entry2 = 0x00000001;
		internal uint table3Entry3 = 0x0000023B;
		internal uint table3Entry4 = 0x00000000;

		// AbilityConvertParams
		internal writeWordCmd table4Entry1 = new writeWordCmd();
		internal writeWordCmd table4Entry2 = new writeWordCmd();
		internal writeWordCmd table4Entry3 = new writeWordCmd();
		internal writeWordCmd table4Entry4 = new writeWordCmd();

		public override string ToString()
		{
			return "";
		}
		public void copyInfoFrom(HatInfoPack sourceHat)
		{
			table1Entry = sourceHat.table1Entry;
			table2Entry = sourceHat.table2Entry;
			table3Entry1.copyInfoFrom(sourceHat.table3Entry1);
			table3Entry2 = sourceHat.table3Entry2;
			table3Entry3 = sourceHat.table3Entry3;
			table3Entry4 = sourceHat.table3Entry4;
			table4Entry1.copyInfoFrom(sourceHat.table4Entry1);
			table4Entry2.copyInfoFrom(sourceHat.table4Entry2);
			table4Entry3.copyInfoFrom(sourceHat.table4Entry3);
			table4Entry4.copyInfoFrom(sourceHat.table4Entry4);
		}

		// Properties
		public const string table1CatName = "Table 1";
		public const string table2CatName = "Table 2";
		public const string table3CatName = "Table 3";
		public const string table4CatName = "Table 4";

		[Category(table1CatName)]
		public string TopStatusKind
		{
			get => "0x" + table1Entry.ToString("X8");
			set
			{
				uint convertedInput = uint.MaxValue;
				string tempStr = value;
				if (tempStr.StartsWith("0x"))
				{
					tempStr = tempStr.Substring(2);
				}
				if (uint.TryParse(tempStr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out convertedInput))
				{
					table1Entry = convertedInput;
				}
			}
		}

		[Category(table2CatName)]
		public writeWordCmd AbilityProcesses
		{
			get => table2Entry;
		}

		[Category(table3CatName)]
		public writeWordCmd AbilityInfo1
		{
			get => table3Entry1;
		}
		[Category(table3CatName)]
		public string AbilityInfo2
		{
			get => Conversions.convertNumToHexString(table3Entry2);
			set
			{
				table3Entry2 = Conversions.convertHexStringToNum(value, table3Entry2);
			}
		}
		[Category(table3CatName)]
		public string AbilityInfo3
		{
			get => Conversions.convertNumToHexString(table3Entry3);
			set
			{
				table3Entry3 = Conversions.convertHexStringToNum(value, table3Entry3);
			}
		}
		[Category(table3CatName)]
		public string AbilityInfo4
		{
			get => Conversions.convertNumToHexString(table3Entry4);
			set
			{
				table3Entry4 = Conversions.convertHexStringToNum(value, table3Entry4);
			}
		}

		[Category(table4CatName)]
		public writeWordCmd ConvertParams1
		{
			get => table4Entry1;
		}
		[Category(table4CatName)]
		public writeWordCmd ConvertParams2
		{
			get => table4Entry2;
		}
		[Category(table4CatName)]
		public writeWordCmd ConvertParams3
		{
			get => table4Entry3;
		}
		[Category(table4CatName)]
		public writeWordCmd ConvertParams4
		{
			get => table4Entry4;
		}
	}

	public class KirbyHatManager
	{
		public const uint kirbyModuleID = 0x60;
		public const uint soraMeleeModuleID = 0x1B;

		public const uint maxHatCount = 0x100;
		const uint table1EntrySize = 0x4;
		const uint table2EntrySize = 0x4;
		const uint table3EntrySize = 0x10;
		const uint table4EntrySize = 0x10;
		const uint table1StartOffset = 0x0;
		const uint table1Length = maxHatCount * table1EntrySize;
		const uint table2StartOffset = table1StartOffset + table1Length;
		const uint table2Length = maxHatCount * table2EntrySize;
		const uint table3StartOffset = table2StartOffset + table2Length;
		const uint table3Length = maxHatCount * table3EntrySize;
		const uint table4StartOffset = table3StartOffset + table3Length;
		const uint table4Length = maxHatCount * table4EntrySize;
		const uint tablesEndOffset = table4StartOffset + table4Length;

		const string tableSectionName = "Section [7]";

		private byte[] _convBuf = new byte[4];

		HatInfoPack defaultInfoPack = new HatInfoPack();
		public SortedDictionary<uint, HatInfoPack> fighterIDToInfoPacks = new SortedDictionary<uint, HatInfoPack>();

		uint getTable1EntryOffset(uint charIDIn)
		{
			uint result = uint.MaxValue;

			if (charIDIn < maxHatCount)
			{
				result = (charIDIn * table1EntrySize) + table1StartOffset;
			}

			return result;
		}
		uint getTable2EntryOffset(uint charIDIn)
		{
			uint result = uint.MaxValue;

			if (charIDIn < maxHatCount)
			{
				result = (charIDIn * table2EntrySize) + table2StartOffset;
			}

			return result;
		}
		uint getTable3EntryOffset(uint charIDIn)
		{
			uint result = uint.MaxValue;

			if (charIDIn < maxHatCount)
			{
				result = (charIDIn * table3EntrySize) + table3StartOffset;
			}

			return result;
		}
		uint getTable4EntryOffset(uint charIDIn)
		{
			uint result = uint.MaxValue;

			if (charIDIn < maxHatCount)
			{
				result = (charIDIn * table4EntrySize) + table4StartOffset;
			}

			return result;
		}

		unsafe UnmanagedMemoryStream getHexStreamFromSectionNode(BrawlLib.SSBB.ResourceNodes.ModuleSectionNode sectionIn)
		{
			UnmanagedMemoryStream result = null;

			if (sectionIn != null)
			{
				result = new UnmanagedMemoryStream((byte*)sectionIn._dataBuffer.Address,
						sectionIn._dataBuffer.Length, sectionIn._dataBuffer.Length, FileAccess.ReadWrite);
			}

			return result;
		}
		uint readWordFromByteArr(UnmanagedMemoryStream hexStreamIn, uint offset)
		{
			uint result = uint.MaxValue;

			if (offset + 4 <= hexStreamIn.Length)
			{
				hexStreamIn.Seek(offset, SeekOrigin.Begin);
				hexStreamIn.Read(_convBuf, 0, 4);
				if (BitConverter.IsLittleEndian)
				{
					_convBuf = _convBuf.Reverse().ToArray();
				}
				result = BitConverter.ToUInt32(_convBuf, 0);
			}

			return result;
		}
		bool writeWordToByteArr(UnmanagedMemoryStream hexStreamIn, uint valueIn, uint offset)
		{
			bool result = false;

			if (offset + 4 <= hexStreamIn.Length)
			{
				_convBuf = BitConverter.GetBytes(valueIn);
				if (BitConverter.IsLittleEndian)
				{
					_convBuf = _convBuf.Reverse().ToArray();
				}
				hexStreamIn.Seek(offset, SeekOrigin.Begin);
				hexStreamIn.Write(_convBuf, 0, 4);

				result = true;
			}

			return result;
		}
		writeWordCmd readRELWriteCMD(BrawlLib.SSBB.ResourceNodes.ModuleSectionNode sectionIn, uint offset)
		{
			writeWordCmd result = new writeWordCmd();

			if (sectionIn != null && sectionIn._manager != null)
			{
				int convertedOffset = (int)(offset / 0x4);

				var cmd = sectionIn._manager.GetCommand(convertedOffset);
				if (cmd != null && cmd._command == BrawlLib.SSBB.ResourceNodes.RELCommandType.WriteWord)
				{
					result._targetOffset = cmd._addend;
					result._targetSection = (byte)cmd._targetSectionId;
					result._targetModuleID = cmd._moduleID;
				}
			}

			return result;
		}
		bool writeRELWriteCMD(BrawlLib.SSBB.ResourceNodes.ModuleSectionNode sectionIn, writeWordCmd commandIn, uint offset)
		{
			bool result = false;

			if (sectionIn != null && sectionIn._manager != null)
			{
				int convertedOffset = (int)(offset / 0x4);

				var cmd = new BrawlLib.SSBB.ResourceNodes.RelCommand(
					(sectionIn.Root as BrawlLib.SSBB.ResourceNodes.ModuleNode).ID,
					sectionIn,
					new BrawlLib.SSBB.Types.RELLink());

				cmd.Command = BrawlLib.SSBB.ResourceNodes.RELCommandType.WriteWord;
				cmd._addend = commandIn._targetOffset;
				cmd._targetSectionId = commandIn._targetSection;
				cmd._moduleID = commandIn._targetModuleID;

				if (sectionIn._manager.GetCommand(convertedOffset) != null)
				{
					sectionIn._manager.ClearCommand(convertedOffset);
				}

				sectionIn._manager.SetCommand(convertedOffset, cmd);
				result = true;
			}

			return result;
		}
		bool writeRELWriteCMDAndWord(BrawlLib.SSBB.ResourceNodes.ModuleSectionNode sectionIn, UnmanagedMemoryStream hexStreamIn, writeWordCmd commandIn, uint offset)
		{
			bool result = false;

			if (writeWordToByteArr(hexStreamIn, 0x00000000, offset))
			{
				if (commandIn.targetModuleIDValid())
				{
					result = writeRELWriteCMD(sectionIn, commandIn, offset);
				}
				else
				{
					result = true;
				}
			}

			return result;
		}

		HatInfoPack buildHatFromSection(uint fighterID, BrawlLib.SSBB.ResourceNodes.ModuleSectionNode sectionIn, UnmanagedMemoryStream hexStreamIn)
		{
			HatInfoPack result = new HatInfoPack();

			if (fighterID < maxHatCount && getTable4EntryOffset(fighterID) < hexStreamIn.Length)
			{
				result.table1Entry = readWordFromByteArr(hexStreamIn, getTable1EntryOffset(fighterID));

				result.table2Entry = readRELWriteCMD(sectionIn, getTable2EntryOffset(fighterID));

				uint table3EntriesStart = getTable3EntryOffset(fighterID);
				result.table3Entry1 = readRELWriteCMD(sectionIn, table3EntriesStart);
				result.table3Entry2 = readWordFromByteArr(hexStreamIn, table3EntriesStart + 0x4);
				result.table3Entry3 = readWordFromByteArr(hexStreamIn, table3EntriesStart + 0x8);
				result.table3Entry4 = readWordFromByteArr(hexStreamIn, table3EntriesStart + 0xC);

				uint table4EntriesStart = getTable4EntryOffset(fighterID);
				result.table4Entry1 = readRELWriteCMD(sectionIn, table4EntriesStart);
				result.table4Entry2 = readRELWriteCMD(sectionIn, table4EntriesStart + 0x4);
				result.table4Entry3 = readRELWriteCMD(sectionIn, table4EntriesStart + 0x8);
				result.table4Entry4 = readRELWriteCMD(sectionIn, table4EntriesStart + 0xC);
			}

			return result;
		}
		bool infoPackHasDefaultData(HatInfoPack sourcePack)
		{
			bool result = sourcePack.table1Entry == defaultInfoPack.table1Entry;
			result &= sourcePack.table2Entry.isSameAs(defaultInfoPack.table2Entry);
			result &= sourcePack.table3Entry1.isSameAs(defaultInfoPack.table3Entry1);
			result &= sourcePack.table3Entry2 == defaultInfoPack.table3Entry2;
			result &= sourcePack.table3Entry3 == defaultInfoPack.table3Entry3;
			result &= sourcePack.table3Entry4 == defaultInfoPack.table3Entry4;
			result &= sourcePack.table4Entry1.isSameAs(defaultInfoPack.table4Entry1);
			result &= sourcePack.table4Entry2.isSameAs(defaultInfoPack.table4Entry2);
			result &= sourcePack.table4Entry3.isSameAs(defaultInfoPack.table4Entry3);
			result &= sourcePack.table4Entry4.isSameAs(defaultInfoPack.table4Entry4);

			return result;
		}

		public bool loadHatEntriesFromREL(BrawlLib.SSBB.ResourceNodes.RELNode kirbyModule)
		{
			bool result = false;

			if (kirbyModule.ModuleID == kirbyModuleID)
			{
				Console.WriteLine("Kirby Module Loaded:");
				Console.WriteLine("Name: " + kirbyModule.Name + "\n");
				Console.WriteLine("Size: " + kirbyModule.UncompressedSize.ToString("X3") + " bytes\n");

				var tableSectionNode = kirbyModule.FindChild(tableSectionName) as BrawlLib.SSBB.ResourceNodes.ModuleSectionNode;
				UnmanagedMemoryStream sectionHexStream = getHexStreamFromSectionNode(tableSectionNode);
				if (sectionHexStream != null && sectionHexStream.Length >= tablesEndOffset)
				{
					fighterIDToInfoPacks.Clear();

					// Grab default Table 2 and 3 Entries from Mario's slot, since that's what vBrawl defaults to.
					defaultInfoPack.table2Entry = readRELWriteCMD(tableSectionNode, getTable2EntryOffset(0x00));
					defaultInfoPack.table3Entry1 = readRELWriteCMD(tableSectionNode, getTable3EntryOffset(0x00));

					for (uint i = 0x00; i < maxHatCount; i++)
					{
						HatInfoPack newInfoPack = buildHatFromSection(i, tableSectionNode, sectionHexStream);
						if (!infoPackHasDefaultData(newInfoPack))
						{
							fighterIDToInfoPacks[i] = newInfoPack;
						}
					}

					result = fighterIDToInfoPacks.Count > 0;
					if (result)
					{
						Console.WriteLine("Successfully loaded and parsed Hat Table!");
					}
				}
				else
				{
					Console.WriteLine("Failed to load Hat Table!");
				}
			}
			else
			{
				Console.WriteLine("Loaded Module file is not Kirby Module!");
				Console.WriteLine("Kirby Module must have ModuleID " + kirbyModuleID.ToString() + ", loaded Module has ID " + kirbyModule.ModuleID.ToString() + "!");
			}

			return result;
		}
		public bool loadHatEntriesFromREL(string filepathIn)
		{
			bool result = false;

			if (File.Exists(filepathIn))
			{
				BrawlLib.SSBB.ResourceNodes.RELNode kirbyModule = new BrawlLib.SSBB.ResourceNodes.RELNode();
				kirbyModule.Replace(filepathIn);
				result = loadHatEntriesFromREL(kirbyModule);
			}
			else
			{
				Console.WriteLine("Failed to load Module file!");
			}

			return result;
		}
		
		public bool copyHatToSlot(uint sourceFighterID, uint destinationFighterID, bool allowOverwrite)
		{
			bool result = false;

			if (fighterIDToInfoPacks.ContainsKey(sourceFighterID) && destinationFighterID < maxHatCount)
			{
				result = true;

				if (!fighterIDToInfoPacks.ContainsKey(destinationFighterID))
				{
					result = createNewHat(destinationFighterID);
				}
				else
				{
					result = allowOverwrite;
				}

				if (result)
				{
					fighterIDToInfoPacks[destinationFighterID].copyInfoFrom(fighterIDToInfoPacks[sourceFighterID]);
				}
			}

			return result;
		}
		public bool moveHatToSlot(uint sourceFighterID, uint destinationFighterID, bool allowOverwrite)
		{
			bool result = false;

			if (copyHatToSlot(sourceFighterID, destinationFighterID, allowOverwrite))
			{
				result = eraseHat(sourceFighterID);
			}

			return result;
		}
		public bool createNewHat(uint targetFighterID)
		{
			bool result = false;

			if (targetFighterID < maxHatCount && !fighterIDToInfoPacks.ContainsKey(targetFighterID))
			{
				fighterIDToInfoPacks[targetFighterID] = new HatInfoPack();
				result = true;
			}

			return result;
		}
		public bool eraseHat(uint targetFighterID)
		{
			bool result = false;

			if (targetFighterID < maxHatCount && fighterIDToInfoPacks.ContainsKey(targetFighterID))
			{
				fighterIDToInfoPacks.Remove(targetFighterID);
				result = true;
			}

			return result;
		}

		public void summarizeHatTable()
		{
			foreach (var currPair in fighterIDToInfoPacks)
			{
				Console.WriteLine("[FID 0x" + currPair.Key.ToString("X2") + "]");
				Console.WriteLine("  - Table 1: " + currPair.Value.TopStatusKind);
				Console.WriteLine("  - Table 2: " + currPair.Value.table2Entry.summaryString());
				Console.WriteLine("  - Table 3:");
				Console.WriteLine("    - Entry 1: " + currPair.Value.table3Entry1.summaryString());
				Console.WriteLine("    - Entry 2: " + currPair.Value.AbilityInfo2);
				Console.WriteLine("    - Entry 2: " + currPair.Value.AbilityInfo3);
				Console.WriteLine("    - Entry 2: " + currPair.Value.AbilityInfo4);
				Console.WriteLine("  - Table 4:");
				Console.WriteLine("    - Entry 1: " + currPair.Value.table4Entry1.summaryString());
				Console.WriteLine("    - Entry 2: " + currPair.Value.table4Entry2.summaryString());
				Console.WriteLine("    - Entry 3: " + currPair.Value.table4Entry3.summaryString());
				Console.WriteLine("    - Entry 4: " + currPair.Value.table4Entry4.summaryString());
				Console.WriteLine("");
			}
		}
		unsafe public bool writeTablesToREL(BrawlLib.SSBB.ResourceNodes.RELNode kirbyModule)
		{
			bool result = false;

			var tableSectionNode = kirbyModule.FindChild(tableSectionName) as BrawlLib.SSBB.ResourceNodes.ModuleSectionNode;
			UnmanagedMemoryStream sectionHexStream = getHexStreamFromSectionNode(tableSectionNode);
			if (sectionHexStream != null && sectionHexStream.Length >= tablesEndOffset)
			{
				result = true;

				tableSectionNode._manager.ClearCommands();

				for (uint i = 0; i < maxHatCount; i++)
				{
					HatInfoPack sourcePack = defaultInfoPack;

					if (fighterIDToInfoPacks.ContainsKey(i))
					{
						sourcePack = fighterIDToInfoPacks[i];
					}

					result &= writeWordToByteArr(sectionHexStream, sourcePack.table1Entry, getTable1EntryOffset(i));

					result &= writeRELWriteCMDAndWord(tableSectionNode, sectionHexStream, sourcePack.table2Entry, getTable2EntryOffset(i));

					uint table3EntryOffset = getTable3EntryOffset(i);
					result &= writeRELWriteCMDAndWord(tableSectionNode, sectionHexStream, sourcePack.table3Entry1, table3EntryOffset);
					result &= writeWordToByteArr(sectionHexStream, sourcePack.table3Entry2, table3EntryOffset + 0x4);
					result &= writeWordToByteArr(sectionHexStream, sourcePack.table3Entry3, table3EntryOffset + 0x8);
					result &= writeWordToByteArr(sectionHexStream, sourcePack.table3Entry4, table3EntryOffset + 0xC);

					uint table4EntryOffset = getTable4EntryOffset(i);
					result &= writeRELWriteCMDAndWord(tableSectionNode, sectionHexStream, sourcePack.table4Entry1, table4EntryOffset);
					result &= writeRELWriteCMDAndWord(tableSectionNode, sectionHexStream, sourcePack.table4Entry2, table4EntryOffset + 0x4);
					result &= writeRELWriteCMDAndWord(tableSectionNode, sectionHexStream, sourcePack.table4Entry3, table4EntryOffset + 0x8);
					result &= writeRELWriteCMDAndWord(tableSectionNode, sectionHexStream, sourcePack.table4Entry4, table4EntryOffset + 0xC);
				}

				BrawlLib.Internal.UnsafeBuffer newBuffer = new BrawlLib.Internal.UnsafeBuffer((int)sectionHexStream.Length);
				sectionHexStream.Seek(0, SeekOrigin.Begin);
				BrawlLib.Internal.Memory.Move(newBuffer.Address, sectionHexStream.PositionPointer, (uint)sectionHexStream.Length);

				tableSectionNode._dataBuffer.Dispose();
				tableSectionNode._dataBuffer = newBuffer;
				tableSectionNode.SignalPropertyChange();
				kirbyModule.UpdateItemIDs();
				kirbyModule.Rebuild();

				result = true;
			}

			return result;
		}
	}
}
