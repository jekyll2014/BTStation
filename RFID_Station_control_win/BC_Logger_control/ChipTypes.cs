using System;
using System.Collections.Generic;
using System.Linq;

namespace RFID_Station_control
{
    public class ChipTypeDto
    {
        public readonly string Name;
        public readonly ChipType Type;
        public readonly byte Id;
        public readonly int Pages;
        public readonly int Bytes;

        public ChipTypeDto(ChipType type)
        {
            Type = type;
            Id = ChipTypes.SystemIds[Type];
            Name = ChipTypes.Names[Type];
            Pages = ChipTypes.PageSizes[Type];
            Bytes = ChipTypes.ByteSizes[Type];
        }

        public ChipTypeDto(byte id)
        {
            Id = id;
            Type = ChipTypes.SystemIds.FirstOrDefault(n => n.Value == Id).Key;
            Name = ChipTypes.Names[Type];
            Pages = ChipTypes.PageSizes[Type];
            Bytes = ChipTypes.ByteSizes[Type];
        }

        public ChipTypeDto(string name)
        {
            Name = name;
            Type = ChipTypes.Names.FirstOrDefault(n => n.Value.Equals(Name, StringComparison.OrdinalIgnoreCase)).Key;
            Id = ChipTypes.SystemIds[Type];
            Pages = ChipTypes.PageSizes[Type];
            Bytes = ChipTypes.ByteSizes[Type];
        }
    }

    public static class ChipTypes
    {
        public const byte PageSize = 4;

        public static Dictionary<string, ChipType> Types = new Dictionary<string, ChipType>
        {
            {"NTAG213", ChipType.NTAG213},
            {"NTAG215", ChipType.NTAG215},
            {"NTAG216", ChipType.NTAG216}
        };

        public static readonly Dictionary<ChipType, string> Names = new Dictionary<ChipType, string>
        {
            { ChipType.NTAG213, "NTAG213" },
            { ChipType.NTAG215, "NTAG215" },
            { ChipType.NTAG216, "NTAG216" }
        };

        public static readonly Dictionary<ChipType, byte> PageSizes = new Dictionary<ChipType, byte>
        {
            { ChipType.NTAG213, 45 },
            { ChipType.NTAG215, 135 },
            { ChipType.NTAG216, 231 }
        };

        public static readonly Dictionary<ChipType, ushort> ByteSizes = new Dictionary<ChipType, ushort>
        {
            { ChipType.NTAG213, 144 },
            { ChipType.NTAG215, 496 },
            { ChipType.NTAG216, 872 }
        };

        public static readonly Dictionary<ChipType, byte> SystemIds = new Dictionary<ChipType, byte>
        {
            { ChipType.NTAG213, 0x12 },
            { ChipType.NTAG215, 0x3e },
            { ChipType.NTAG216, 0x6d }
        };
    }
}