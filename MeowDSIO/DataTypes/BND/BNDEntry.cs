﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowDSIO.DataTypes.BND
{
    public class BNDEntry : IDisposable
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int? Unknown1 = null;
        public int? BND4_Unknown2 = null;
        public int? BND4_Unknown3 = null;
        public int? BND4_Unknown4 = null;
        public int? BND4_Unknown5 = null;
        public int? BND4_Unknown6 = null;
        public int? BND4_Unknown7 = null;
        private byte[] Data;

        public BNDEntry(int ID, string Name, int? Unknown1, byte[] FileBytes)
        {
            this.ID = ID;
            this.Name = Name;
            this.Unknown1 = Unknown1;
            Data = FileBytes;
        }

        public T ReadDataAs<T>()
            where T : DataFile, new()
        {
            return DataFile.LoadFromBytes<T>(Data, Name, null);
        }

        public T ReadDataAs<T>(IProgress<(int, int)> prog)
            where T : DataFile, new()
        {
            return DataFile.LoadFromBytes<T>(Data, Name, prog);
        }

        public void ReplaceData<T>(T data)
            where T : DataFile, new()
        {
            Data = DataFile.SaveAsBytes(data, Name, null);
        }

        public void ReplaceData<T>(T data, IProgress<(int, int)> prog)
            where T : DataFile, new()
        {
            Data = DataFile.SaveAsBytes(data, Name, prog);
        }

        public int Size => (Data?.Length ?? 0);

        public byte[] GetBytes()
        {
            return Data;
        }

        public void SetBytes(byte[] newBytes)
        {
            Data = newBytes;
        }

        public void Dispose()
        {
            Data = null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
