﻿using System;
using System.IO;
using SavestateFormat.Snes9x.Extensions;
using SavestateFormat.Snes9x.Models.Structs;

namespace SavestateFormat.Snes9x.Helpers
{
	public enum SavestateLoadMode
	{
		All,
		SRAM
	}

	public class SavestateManager
	{
		public Savestate Load(in string filepath) => Load(filepath, SavestateLoadMode.SRAM);
		public Savestate Load(in string filepath, in SavestateLoadMode mode)
		{
			using var file = new FileStream(filepath, FileMode.Open, FileAccess.Read);
			Savestate result = default;

			if (file.CanRead)
			{
				result = Load(file, mode);

				file.Close();
			}
			else
				Console.WriteLine("SavestateEmpty");

			return result;
		}

		public Savestate Load(in Stream stream) => Load(stream, SavestateLoadMode.SRAM);
		public Savestate Load(in Stream stream, in SavestateLoadMode mode)
		{
			var uncompressed = GzipHelper.Decompress(stream);
			var ms = new MemoryStream(uncompressed);

			var header = ms.Read<Header>();
			if (!header.IsValidSnes9xHeader())
			{
				Console.WriteLine("SavestateInvalidFile");
				return default;
			}

			Savestate result = new()
			{
				Header = header,
				NAM = ms.ReadFileBlock(),
				CPU = ms.ReadFileBlock(),
				REG = ms.ReadFileBlock(),
				PPU = ms.ReadFileBlock(),
				DMA = ms.ReadFileBlock(),
				VRA = ms.ReadFileBlock(),
				RAM = ms.ReadFileBlock(),
				SRA = ms.ReadFileBlock() // SRAM
			};

			if(mode == SavestateLoadMode.SRAM)
				return result;

			result.FIL = ms.ReadFileBlock();
			result.APU = ms.ReadFileBlock();
			result.ARE = ms.ReadFileBlock(); // (if emulating the APU)
			result.ARA = ms.ReadFileBlock(); // (if emulating the APU)
			result.SOU = ms.ReadFileBlock(); // (if emulating the APU)
			result.SA1 = ms.ReadFileBlock(); // (if emulating the SA-1)
			result.SAR = ms.ReadFileBlock(); // (if emulating the SA-1)
			result.SP7 = ms.ReadFileBlock(); // (if emulating the SPC7110)
			result.RTC = ms.ReadFileBlock(); // (if emulating the SPC7110 RTC)

			return result;
		}
	}
}
