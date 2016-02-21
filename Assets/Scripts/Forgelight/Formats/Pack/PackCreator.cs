/******************************************************************************
*  Code used from psemu's ModLauncher <https://github.com/psemu/ModLauncher>  *
*  Copyright (c) 2015 psemu                                                   *
******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Forgelight.Utils;
using Forgelight.Utils.Cryptography;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace Forgelight.Pack
{
    class FileHeader
    {
        public uint name_len;
        public byte[] name;
        public uint offset;
        public uint length;
        public uint crc32;

        public byte[] Encode()
        {
            EndianBinaryWriter wr = new EndianBinaryWriter(EndianBitConverter.Big, new MemoryStream());
            wr.Write(name_len);
            wr.Write(name);
            wr.Write(offset);
            wr.Write(length);
            wr.Write(crc32);

            return ((MemoryStream)wr.BaseStream).ToArray();
        }
    }

    class ChunkHeader
    {
        public uint NextChunkOffset;
        public uint FileCount;
        public FileHeader[] files;

        public byte[] Encode()
        {
            EndianBinaryWriter wr = new EndianBinaryWriter(EndianBitConverter.Big, new MemoryStream());
            wr.Write(NextChunkOffset);
            wr.Write(FileCount);
            foreach (FileHeader h in files)
            {
                wr.Write(h.Encode());
            }

            return ((MemoryStream)wr.BaseStream).ToArray();
        }
    }

    public class PackCreator
    {
        public static void CreatePackFromDirectory()
        {
            string sourceFolder = DialogUtils.OpenDirectory(
                "Select folder that contains the assets you wish to pack",
                "",
                "", DialogUtils.DirectoryIsEmpty);

            if (sourceFolder == null)
            {
                return;
            }

            string[] files = Directory.GetFiles(sourceFolder);

            var destinationFile = DialogUtils.SaveFile(
                "Select destination to save created pack file",
                sourceFolder,
                "Assets_256",
                "pack");

            if (destinationFile != null)
            {
                CreatePackFromFiles(files, destinationFile);
                DialogUtils.DisplayDialog("Export Successful", "Successfully packed and saved " + files.Length + " assets to " + destinationFile);
            }
        }

        private static void CreatePackFromFiles(string[] files, string savePath)
        {
            int fileCount = files.Length;

            List<FileHeader> fheader = new List<FileHeader>();
            List<byte[]> fileData = new List<byte[]>();

            //Create File Headers for each file to pack
            foreach (string file in files)
            {
                byte[] fdata = File.ReadAllBytes(file);
                fileData.Add(fdata);
                Crc32 crc = new Crc32();
                byte[] c = crc.ComputeHash(fdata);
                string filename = file.Substring(file.LastIndexOf("\\") + 1);
                FileHeader h = new FileHeader()
                {
                    name_len = (uint)filename.Length,
                    name = Encoding.ASCII.GetBytes(filename),
                    offset = 0,
                    length = (uint)fdata.Length,
                    crc32 = (uint)BitConverter.ToInt32(c, 0),
                };
                fheader.Add(h);
            }

            //Create a chunk header for the pack file
            ChunkHeader header = new ChunkHeader()
            {
                NextChunkOffset = 0,
                FileCount = (uint)fileCount,
                files = fheader.ToArray(),
            };

            //Update the files with the offset of each data chunk of the file
            int length = header.Encode().Length;
            int offset = 0;
            for (int i = 0; i < fheader.Count; i++)
            {
                if (i != 0)
                {
                    offset += fileData[i - 1].Length;
                }
                fheader[i].offset = (uint)length + (uint)offset;
            }
            //Write the chunk to a file
            using (EndianBinaryWriter wr = new EndianBinaryWriter(EndianBitConverter.Big, File.Open(savePath, FileMode.OpenOrCreate)))
            {
                byte[] ph = header.Encode();
                wr.Write(ph);
                for (int i = 0; i < fileData.Count; i++)
                {
                    wr.Write(fileData[i]);
                }
            }

        }
    }
}
