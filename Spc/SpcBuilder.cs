using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileEmulationFramework.Lib.IO.Struct;
using FileEmulationFramework.Lib.IO;
using FileEmulationFramework.Lib.Utilities;
using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using DRV3_Sharp_Library;
using DRV3_Sharp_Library.Formats.Archive.SPC;

namespace SPC.Stream.Emulator.Spc
{
    internal class SpcBuilder
    {
        private readonly Dictionary<string, FileSlice> _customFiles = new();

        /// <summary>
        /// Adds a file to the Virtual SPC builder.
        /// </summary>


        /// <summary>
        /// Adds a file to the Virtual SPC builder.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        ///  
        private const string CONST_FILE_MAGIC = "CPS.";
        private const string CONST_VITA_COMPRESSED_MAGIC = "$CMP";
        private const string CONST_TABLE_HEADER = "Root";
        public void AddOrReplaceFile(string filePath)
        {
            string[] filePathSplit = filePath.Split(Constants.SpcExtension + Path.DirectorySeparatorChar);
            //_customFiles[filePathSplit[^1].Replace("\\", "/")] = new(filePath);
            _customFiles[filePathSplit[^1]] = new(filePath);
        }
        public unsafe MultiStream Build(IntPtr handle, string wadFilepath, Logger? logger = null)
        {
            logger?.Info($"[{nameof(SpcBuilder)}] Building Spc File | {{0}}", wadFilepath);

            var stream = new FileStream(new SafeFileHandle(handle, false), FileAccess.Read);
            SpcData inputData;
            stream.Position = 0;
            SpcSerializer.Deserialize(stream, out inputData);
            //WadLib.Wad wad = new WadLib.Wad(stream);

            System.IO.Stream headerStream = new MemoryStream();
            //spcFile.
            //wad.WriteHeader(headerStream);
            using BinaryWriter writer = new(headerStream, Encoding.ASCII, true);

            writer.Write(Encoding.ASCII.GetBytes(CONST_FILE_MAGIC));
            // Write unknown data 1 (24 bytes, possibly padding?)
            writer.Write((int)0);
            byte[] paddingFF = new byte[8]; // 8 bytes of 0xFF padding
            Array.Fill<byte>(paddingFF, 0xFF);
            writer.Write(paddingFF);
            writer.Write(new byte[0x18]);   // Padding
            writer.Write(inputData.Files.Count);
            writer.Write(inputData.Unknown2C);
            writer.Write(new byte[0x10]);   // Padding
            writer.Write(Encoding.ASCII.GetBytes(CONST_TABLE_HEADER));
            writer.Write(new byte[0x0C]);   // Padding
            var pairs = new List<StreamOffsetPair<System.IO.Stream>>()
            {
                // Add Header
                new (headerStream, OffsetRange.FromStartAndLength(0, headerStream.Length))
            };


            long offset = headerStream.Length;

            // Write file entries
            foreach (var entry in inputData.Files)
            {
               
                string name = entry.Name;
                logger?.Debug("Name: " + name);
                if (_customFiles.ContainsKey(name)) // time for custom file!
                {
                    logger?.Debug("REPLACING CUSTOM FILE");
                    var customFile = _customFiles[name];
                    FileStream fs = new FileStream(new SafeFileHandle(customFile.Handle, false), FileAccess.Read);
                    byte[] dataNew = new byte[fs.Length];
                    fs.Read(dataNew);
                    fs.Close();
                    if (entry.IsCompressed)
                    {
                        dataNew = SpcCompressor.Compress(dataNew);
                    }
                    System.IO.Stream entrieStream = new MemoryStream();
                    using BinaryWriter entryWriter = new(entrieStream, Encoding.ASCII, true);
                    entryWriter.Write((short)(entry.IsCompressed ? 2 : 1));
                    entryWriter.Write(entry.UnknownFlag);
                    entryWriter.Write(dataNew.Length);
                    entryWriter.Write(customFile.Length);
                    entryWriter.Write(name.Length);
                    entryWriter.Write(new byte[0x10]);   // Padding

                    int namePadding = (0x10 - (name.Length + 1) % 0x10) % 0x10;
                    entryWriter.Write(Encoding.GetEncoding("shift-jis").GetBytes(name));
                    entryWriter.Write(new byte[namePadding + 1]);

                    int dataPadding = (0x10 - dataNew.Length % 0x10) % 0x10;
                    entryWriter.Write(dataNew);
                    entryWriter.Write(new byte[dataPadding]);

                    pairs.Add(new(entrieStream, OffsetRange.FromStartAndLength(offset, entrieStream.Length)));
                    offset += entrieStream.Length;
                }
                else
                {
                    System.IO.Stream entrieStream = new MemoryStream();
                    using BinaryWriter entryWriter = new(entrieStream, Encoding.ASCII, true);
                    entryWriter.Write((short)(entry.IsCompressed ? 2 : 1));
                    entryWriter.Write(entry.UnknownFlag);
                    entryWriter.Write(entry.Data.Length);
                    entryWriter.Write(entry.OriginalSize);
                    entryWriter.Write(name.Length);
                    entryWriter.Write(new byte[0x10]);   // Padding

                    int namePadding = (0x10 - (name.Length + 1) % 0x10) % 0x10;
                    entryWriter.Write(Encoding.GetEncoding("shift-jis").GetBytes(name));
                    entryWriter.Write(new byte[namePadding + 1]);

                    int dataPadding = (0x10 - entry.Data.Length % 0x10) % 0x10;
                    entryWriter.Write(entry.Data);
                    entryWriter.Write(new byte[dataPadding]);

                    pairs.Add(new(entrieStream, OffsetRange.FromStartAndLength(offset, entrieStream.Length)));
                    offset += entrieStream.Length;
                }
                
            }
            
            return new MultiStream(pairs, logger);
        }
    }
}
