﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.IO
{
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes a string prefixed with its length as a 32-bit integer using the default platform encoding.
        /// The string is not null terminated.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="str"></param>
        public static void WritePrefixedString(this BinaryWriter writer, string str)
        {
            var bytes = Encoding.Default.GetBytes(str);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public static void WriteTileLocation(this BinaryWriter writer, in locXY location)
        {
            writer.Write(location.locx);
            writer.Write(location.locy);
        }

        /// <summary>
        /// Writes a ToEE object id.
        /// </summary>
        public static void WriteObjectId(this BinaryWriter writer, in ObjectId id)
        {
            Span<byte> buffer = stackalloc byte[24];
            var payload = buffer.Slice(8); // ObjectId seems to use 8-byte packed

            var success = true;

            switch (id.Type)
            {
                case ObjectIdKind.Null:
                    success &= BitConverter.TryWriteBytes(buffer, (ushort) 0);
                    break;
                case ObjectIdKind.Prototype:
                    success &= BitConverter.TryWriteBytes(buffer, (ushort) 1);
                    ushort protoId = id.PrototypeId;
                    success &= BitConverter.TryWriteBytes(payload, protoId);
                    break;
                case ObjectIdKind.Permanent:
                    success &= BitConverter.TryWriteBytes(buffer, (ushort) 2);
                    var guid = id.PermanentId;
                    MemoryMarshal.Write(payload, ref guid);
                    break;
                case ObjectIdKind.Positional:
                    success &= BitConverter.TryWriteBytes(buffer, (ushort) 3);
                    var positional = id.PositionalId;
                    MemoryMarshal.Write(payload, ref positional);
                    break;
                case ObjectIdKind.Handle:
                    throw new ArgumentException("Cannot write an objectid containing a live object!");
                case ObjectIdKind.Blocked:
                    success &= BitConverter.TryWriteBytes(buffer, (ushort) 0xFFFF);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!success)
            {
                throw new Exception("Failed to serialize ObjectId");
            }

            writer.Write(buffer);
        }
    }
}