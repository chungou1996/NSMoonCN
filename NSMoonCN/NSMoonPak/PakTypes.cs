using System.Runtime.InteropServices;

namespace NSMoonPak.PakTypes
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Pak_Header
    {

        /// char[16]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string magic;

        /// char[32]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string description;

        /// unsigned short
        public ushort minor_version;

        /// unsigned short
        public ushort major_version;

        /// unsigned int
        public uint index_length;

        /// unsigned int
        public uint decode_key;

        /// unsigned int
        public uint index_entries;

        /// unsigned int
        public uint data_offset;

        /// unsigned int
        public uint index_offset;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Pak_Entry
    {

        /// char[64]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string name;

        /// unsigned int
        public uint offset;

        /// unsigned int
        public uint length;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Scw_Header
    {

        /// char[16]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string magic;

        /// unsigned short
        public ushort minor_version;

        /// unsigned short
        public ushort major_version;

        /// unsigned int
        public int is_compr;

        /// unsigned int
        public int uncomprlen;

        /// unsigned int
        public int comprlen;

        /// unsigned int
        public int always_1;

        /// unsigned int
        public int instruction_table_entries;

        /// unsigned int
        public int string_table_entries;

        /// unsigned int
        public int unknown_table_entries;

        /// unsigned int
        public int instruction_data_length;

        /// unsigned int
        public int string_data_length;

        /// unsigned int
        public int unknown_data_length;

        /// unsigned char[392]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 392)]
        public string pad;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Scw4_Entry
    {

        /// unsigned int
        public uint offset;

        /// unsigned int
        public uint length;
    }
}
