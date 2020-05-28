using NSMoonPak.PakTypes;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace NSMoonPak
{
    public static class Pak
    {
        /// <summary>
        /// 压缩到路径。
        /// </summary>
        /// <param name="InDirectory">压缩路径</param>
        /// <param name="OutFile">文件路径</param>
        public static void Pack(string InDirectory, string OutFile)
        {
            string[] directories = Directory.GetDirectories(InDirectory);

            string[] files = Directory.GetFiles(InDirectory);

            //文件夹下存放Scw
            Pak_Header pak_header = new Pak_Header();
            MemoryStream pak_index = new MemoryStream();
            MemoryStream pak_data = new MemoryStream();

            foreach (string file in files)
            {
                // TODO 图片处理
            }
            foreach (string directory in directories)
            {
                Pak_Entry pak_entry = new Pak_Entry();
                Scw_Header scw_header = new Scw_Header();

                MemoryStream header_decompress = new MemoryStream();
                MemoryStream data_decompress = new MemoryStream();
                //先写数据区

                byte[] instruction = File.ReadAllBytes(Path.Combine(directory, "instruction.bin"));
                data_decompress.Write(instruction, 0, instruction.Length);
                byte[] instruction_entries = File.ReadAllBytes(Path.Combine(directory, "instruction_table.bin"));
                header_decompress.Write(instruction_entries, 0, instruction_entries.Length);

                long position = data_decompress.Position;
                StreamReader reader = new StreamReader(Path.Combine(directory, "script.txt"), Encoding.GetEncoding("GB2312"));
                int script_count = 0;
                while (!reader.EndOfStream)
                {
                    //跳过第一行
                    reader.ReadLine();
                    string text = reader.ReadLine().TrimStart(':').TrimEnd(' ').TrimEnd((char)0x20);
                    //跳过分割
                    reader.ReadLine();
                    byte[] data = Encoding.GetEncoding("gb2312").GetBytes(text);
                    if (data.Last() != 0)
                        data = data.Concat(new byte[] { 0 }).ToArray();
                    Scw4_Entry entry = new Scw4_Entry() { offset = (uint)(data_decompress.Position - position), length = (uint)data.Length };
                    data_decompress.Write(data, 0, data.Length);
                    byte[] header = PakUtils.ToByteArray(entry);
                    header_decompress.Write(header, 0, header.Length);
                    script_count++;
                }

                long script_length = data_decompress.Position - position;

                byte[] method = File.ReadAllBytes(Path.Combine(directory, "method.bin"));
                data_decompress.Write(method, 0, method.Length);

                byte[] method_entries = File.ReadAllBytes(Path.Combine(directory, "method_table.bin"));
                header_decompress.Write(method_entries, 0, method_entries.Length);

                byte[] scw_decompress = header_decompress.ToArray().Concat(data_decompress.ToArray()).ToArray();

                byte[] scw_compress = PakUtils.LZSS.Compress(scw_decompress);
                for (int i = 0; i < scw_compress.Length; i++)
                    scw_compress[i] ^= Convert.ToByte(i & 0xFF);

                scw_header.magic = "Scw4.x";
                scw_header.major_version = 1024;
                scw_header.minor_version = 263;
                scw_header.always_1 = 1;
                scw_header.is_compr = -1;
                scw_header.comprlen = scw_compress.Length;
                scw_header.uncomprlen = scw_decompress.Length;
                scw_header.instruction_table_entries = instruction_entries.Length / Marshal.SizeOf(typeof(Scw4_Entry));
                scw_header.instruction_data_length = instruction.Length;
                scw_header.string_table_entries = script_count;
                scw_header.string_data_length = (int)script_length;
                scw_header.unknown_table_entries = method_entries.Length / Marshal.SizeOf(typeof(Scw4_Entry));
                scw_header.unknown_data_length = method.Length;
                scw_header.pad = "";

                byte[] scw = PakUtils.ToByteArray(scw_header).Concat(scw_compress).ToArray();
                pak_entry.name = Path.GetFileName(directory);
                pak_entry.length = (uint)scw.Length;
                pak_entry.offset = (uint)pak_data.Position;
                pak_data.Write(scw, 0, scw.Length);
                byte[] temp = PakUtils.ToByteArray(pak_entry);
                pak_index.Write(temp, 0, temp.Length);
            }

            //压缩索引
            byte[] index = PakUtils.LZSS.Compress(pak_index.ToArray());
            var z = PakUtils.LZSS.Decompress(index);
            byte[] pad = new byte[1976];
            pak_header.magic = "GsPack4 abc";
            pak_header.description = "GsPackFile4";
            pak_header.major_version = 4;
            pak_header.minor_version = 0;
            pak_header.decode_key = 0;
            pak_header.data_offset = (uint)(Marshal.SizeOf(typeof(Pak_Header)) + pad.Length);
            pak_header.index_entries = (uint)(pak_index.ToArray().Length / Marshal.SizeOf(typeof(Pak_Entry)));
            pak_header.index_length = (uint)index.Length;
            pak_header.index_offset = (uint)(pak_header.data_offset + pak_data.Length);


            byte[] pak = PakUtils.ToByteArray(pak_header).Concat(pad).Concat(pak_data.ToArray()).Concat(index.ToArray()).ToArray();
            File.WriteAllBytes(OutFile, pak);
        }
        /// <summary>
        /// 解压到路径。
        /// </summary>
        /// <param name="InFile">文件路径</param>
        /// <param name="OutDirectory">解压路径</param>
        public static void Unpack(string InFile, string OutDirectory)
        {
            using (FileStream pak = File.OpenRead(InFile))
            {
                if (PakUtils.CheckMagic(pak, "GsPack4 abc"))
                {
                    if (OutDirectory == null || OutDirectory.Length == 0) OutDirectory = Path.GetFileName(InFile);
                    Directory.CreateDirectory(OutDirectory);

                    Pak_Header pak_header = PakUtils.ToStruct<Pak_Header>(pak);

                    pak.Seek(pak_header.index_offset, SeekOrigin.Begin);
                    byte[] pak_compress = new byte[pak_header.index_length];
                    pak.Read(pak_compress, 0, pak_compress.Length);
                    byte[] pak_decompress = PakUtils.LZSS.Decompress(pak_compress);

                    Pak_Entry[] pak_entries = PakUtils.ToStructArray<Pak_Entry>(pak_decompress);
                    for (int i = 0; i < pak_entries.Length; i++)
                    {
                        Pak_Entry pak_entry = pak_entries[i];

                        pak.Seek(pak_header.data_offset, SeekOrigin.Begin);
                        pak.Seek(pak_entry.offset, SeekOrigin.Current);
                        byte[] data = new byte[pak_entry.length];
                        pak.Read(data, 0, (int)pak_entry.length);

                        using (MemoryStream data_stream = new MemoryStream(data))
                        {
                            if (PakUtils.CheckMagic(data_stream, "Scw4.x"))
                            {
                                UnpackScw4(OutDirectory, pak_entry, data_stream);
                            }
                            else
                            {
                                // TODO 处理图片文件
                            }
                        }
                    }
                }
                else
                {
                    throw new PakException.MagicMissMatching(Path.GetFileName(InFile));
                }
            }
        }

        private static void UnpackScw4(string OutDirectory, Pak_Entry pak_entry, MemoryStream data_stream)
        {
            //解压缩
            Scw_Header scw4_header = PakUtils.ToStruct<Scw_Header>(data_stream);
            var x = PakUtils.ToByteArray(scw4_header);
            byte[] scw_compress = new byte[scw4_header.comprlen];
            data_stream.Read(scw_compress, 0, scw_compress.Length);

            if (scw4_header.is_compr == -1)
            {
                for (int i = 0; i < scw_compress.Length; i++)
                    scw_compress[i] ^= Convert.ToByte(i & 0xFF);
            }

            byte[] scw_decompress = PakUtils.LZSS.Decompress(scw_compress);

            using (MemoryStream decompress_stream = new MemoryStream(scw_decompress))
            {
                string directory = Path.Combine(OutDirectory, pak_entry.name);
                Directory.CreateDirectory(directory);

                int size = Marshal.SizeOf(typeof(Scw4_Entry));

                byte[] instruction_table = new byte[scw4_header.instruction_table_entries * size];
                decompress_stream.Read(instruction_table, 0, instruction_table.Length);
                File.WriteAllBytes(Path.Combine(directory, "instruction_table.bin"), instruction_table);
                Scw4_Entry[] instruction_entries = PakUtils.ToStructArray<Scw4_Entry>(instruction_table);


                byte[] script_table = new byte[scw4_header.string_table_entries * size];
                decompress_stream.Read(script_table, 0, script_table.Length);
                Scw4_Entry[] script_entries = PakUtils.ToStructArray<Scw4_Entry>(script_table);

                byte[] method_table = new byte[scw4_header.unknown_table_entries * size];
                decompress_stream.Read(method_table, 0, method_table.Length);
                File.WriteAllBytes(Path.Combine(directory, "method_table.bin"), method_table);
                Scw4_Entry[] method_entries = PakUtils.ToStructArray<Scw4_Entry>(method_table);

                using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(directory, "instruction.bin"), FileMode.Create)))
                {
                    foreach (Scw4_Entry scw_entry in instruction_entries)
                    {
                        byte[] data = new byte[scw_entry.length];
                        decompress_stream.Read(data, 0, data.Length);
                        writer.Write(data);
                    }
                }

                using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "script.txt"), false, Encoding.GetEncoding("GB2312")))
                {
                    foreach (Scw4_Entry scw_entry in script_entries)
                    {
                        byte[] data = new byte[scw_entry.length];
                        decompress_stream.Read(data, 0, data.Length);

                        string text = Encoding.GetEncoding("Shift_JIS").GetString(data);
                        writer.WriteLine($":{text}");
                        writer.WriteLine($":{text}");
                        File.AppendAllText("total.txt", text + Environment.NewLine, Encoding.GetEncoding("GB2312"));
                        writer.WriteLine();
                    }
                }
                using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(directory, "method.bin"), FileMode.Create)))
                {
                    long length = decompress_stream.Length - decompress_stream.Position;
                    byte[] data = new byte[length];
                    decompress_stream.Read(data, 0, data.Length);
                    writer.Write(data);
                }
            }
        }
    }
}
