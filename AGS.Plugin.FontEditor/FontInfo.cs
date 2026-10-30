using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace AGS.Plugin.FontEditor
{
    public class CCharInfo
    {
        public UInt16 Width;
        public UInt16 Height;
        public byte[] ByteLines;

        // Pixel depth: 1 for legacy WFN/SCI, 4 for extended WFN.
        public int BitDepth = 1;

        // Whether this character has an entry in the WFN File 2 character map.
        // A mapped glyph may legitimately have zero width or height.
        public bool HasGlyphMapping = false;

        public UInt16 WidthOriginal;
        public UInt16 HeightOriginal;
        public byte[] ByteLinesOriginal;

        public Image UnscaledImage;
        public Int32 Index;

        private class UndoState
        {
            public UInt16 Width;
            public UInt16 Height;
            public int BitDepth;

            public byte[] ByteLines;
        }

        private List<UndoState> UndoRedoList = new List<UndoState>();
        public Int32 UndoRedoPosition = 0;

        public void UndoRedoListAdd(byte[] bytearray, bool force = false)
        {
            UndoState newState = new UndoState();

            newState.Width = Width;
            newState.Height = Height;
            newState.BitDepth = BitDepth;

            if (bytearray != null)
                newState.ByteLines = (byte[])bytearray.Clone();
            else
                newState.ByteLines = null;

            if (UndoRedoList.Count == 0)
            {
                UndoRedoList.Add(newState);
            }
            else
            {
                UndoState current = UndoRedoList[UndoRedoPosition];

                if (!force &&
                    current.Width == newState.Width &&
                    current.Height == newState.Height &&
                    current.BitDepth == newState.BitDepth &&
                    ArraysEqual(current.ByteLines, newState.ByteLines))
                {
                    return;
                }

                UndoRedoList.Add(newState);
                UndoRedoPosition++;
            }
        }
        public void UndoRedoListClear()
        {
            UndoRedoList.Clear();
        }
        public void UndoRedoListTidyUp()
        {
            while (UndoRedoList.Count - 1 > UndoRedoPosition)
            {
                UndoRedoList.RemoveAt(UndoRedoList.Count - 1);
            }
        }

        public bool RedoPossible
        {
            get { return (UndoRedoList.Count - 1 > UndoRedoPosition); }
        }
        public bool UndoPossible
        {
            get { return (UndoRedoPosition > 0); }
        }

        public void Undo()
        {
            if (UndoRedoPosition > 0)
            {
                UndoRedoPosition--;

                UndoState state = UndoRedoList[UndoRedoPosition];

                Width = state.Width;
                Height = state.Height;
                BitDepth = state.BitDepth;

                if (state.ByteLines != null)
                    ByteLines = (byte[])state.ByteLines.Clone();
                else
                    ByteLines = null;
            }
        }
        public void Redo()
        {
            if (UndoRedoPosition < UndoRedoList.Count - 1)
            {
                UndoRedoPosition++;

                UndoState state = UndoRedoList[UndoRedoPosition];

                Width = state.Width;
                Height = state.Height;
                BitDepth = state.BitDepth;

                if (state.ByteLines != null)
                    ByteLines = (byte[])state.ByteLines.Clone();
                else
                    ByteLines = null;
            }
        }

        private static bool ArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }
            return true;
        }

        public bool HasBitmapData()
        {
            if (ByteLines == null || ByteLines.Length == 0)
                return false;

            for (int i = 0; i < ByteLines.Length; i++)
            {
                if (ByteLines[i] != 0)
                    return true;
            }
            return false;
        }

        public bool IsEmptyGlyph()
        {
            return !HasBitmapData();
        }
    }

    public abstract class CFontInfo
    {
        public string FontPath;
        public string FontName;
        public string WFNName;
        public CCharInfo[] Character;
        public int NumberOfCharacters;
        public UInt16 TextHeight; // only SCI fonts

        protected byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }
        protected string ByteArrayToString(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }
        public abstract void Read(System.IO.BinaryReader binaryReader);
        public abstract void Write(System.IO.BinaryWriter binaryWriter);

        public void Read(string filename)
        {
            System.IO.FileStream file = System.IO.File.Open(filename, System.IO.FileMode.Open);
            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(file);

            Read(binaryReader);

            binaryReader.Close();
            file.Close();
        }
        public void Write(string filename)
        {
            // (Note: This method is not used by your Save flow, but Create is safer than Open)
            System.IO.FileStream file = System.IO.File.Open(filename, System.IO.FileMode.Create);
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(file);

            Write(binaryWriter);

            binaryWriter.Close();
            file.Close();
        }
    }

    public static class WFNBitDepthConverter
    {
        public static void ConvertGlyph(CCharInfo glyph, int targetBitDepth)
        {
            if (glyph == null || glyph.BitDepth == targetBitDepth)
                return;

            if (targetBitDepth != 1 && targetBitDepth != 4)
                throw new ArgumentOutOfRangeException(nameof(targetBitDepth));

            int width = glyph.Width;
            int height = glyph.Height;

            int sourceStride = glyph.BitDepth == 4
                ? (width + 1) / 2
                : (width + 7) / 8;

            int targetStride = targetBitDepth == 4
                ? (width + 1) / 2
                : (width + 7) / 8;

            byte[] converted = new byte[targetStride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sourceIndex = y * sourceStride +
                        (glyph.BitDepth == 4 ? x / 2 : x / 8);

                    byte source = glyph.ByteLines != null &&
                        sourceIndex < glyph.ByteLines.Length
                        ? glyph.ByteLines[sourceIndex]
                        : (byte)0;

                    int shade;

                    if (glyph.BitDepth == 4)
                        shade = (x % 2 == 0)
                            ? (source >> 4)
                            : (source & 0x0F);
                    else
                        shade = (source & (0x80 >> (x % 8))) != 0
                            ? (targetBitDepth == 4 ? 8 : 15)
                            : 0;

                    int targetIndex = y * targetStride +
                        (targetBitDepth == 4 ? x / 2 : x / 8);

                    if (targetBitDepth == 4)
                    {
                        if (x % 2 == 0)
                            converted[targetIndex] |= (byte)(shade << 4);
                        else
                            converted[targetIndex] |= (byte)shade;
                    }
                    else if (shade > 0)
                    {
                        converted[targetIndex] |= (byte)(0x80 >> (x % 8));
                    }
                }
            }

            glyph.ByteLines = converted;
            glyph.BitDepth = targetBitDepth;
        }
    }

    public class CWFNFontInfo : CFontInfo
    {
        public bool Is4Bit { get; private set; }

        // Font-wide base shade for 4-bit WFN fonts.
        public uint BaseShadeIndex { get; set; } = 8;

        public void SetBitDepth(bool use4Bit)
        {
            Is4Bit = use4Bit;
            WFNName = use4Bit
                ? "WGT Font File 2"
                : "WGT Font File  ";
        }

        public void ConvertBitDepth(bool use4Bit)
        {
            if (Is4Bit == use4Bit)
                return;

            if (Character == null)
                throw new InvalidOperationException("No font data to convert.");

            int targetBitDepth = use4Bit ? 4 : 1;

            foreach (CCharInfo glyph in Character)
            {
                if (glyph != null)
                    WFNBitDepthConverter.ConvertGlyph(glyph, targetBitDepth);
            }

            SetBitDepth(use4Bit);
        }

        public override void Read(System.IO.BinaryReader binaryReader)
        {
            byte[] name = binaryReader.ReadBytes(15);
            WFNName = ByteArrayToString(name);

            Is4Bit = (WFNName == "WGT Font File 2");

            if (!Is4Bit && WFNName != "WGT Font File  ")
                throw new System.IO.InvalidDataException("Unsupported WFN font signature.");

            if (Is4Bit)
            {
                ReadWFNFile2(binaryReader);
                return;
            }

            long offset = binaryReader.ReadUInt16();

            // Offsets table starts at "offset"
            binaryReader.BaseStream.Position = offset;

            long bytesLeft = binaryReader.BaseStream.Length - offset;
            if (bytesLeft < 0) bytesLeft = 0;

            int offsetSize = Is4Bit ? 4 : 2;

            int numChars = (int)(bytesLeft / offsetSize);
            NumberOfCharacters = numChars;

            UInt32[] positionArray = new UInt32[numChars];
            Character = new CCharInfo[numChars];

            for (int i = 0; i < numChars; i++)
            {
                positionArray[i] = Is4Bit
                    ? binaryReader.ReadUInt32()
                    : binaryReader.ReadUInt16();
            }

            for (int i = 0; i < numChars; i++)
            {
                UInt32 pos = positionArray[i];

                // offset==0 means "missing/placeholder glyph"
                if (pos == 0)
                {
                    Character[i] = new CCharInfo
                    {
                        Index = i,
                        BitDepth = Is4Bit ? 4 : 1,
                        Width = 0,
                        Height = 0,
                        WidthOriginal = 0,
                        HeightOriginal = 0,
                        ByteLines = null,
                        ByteLinesOriginal = null
                    };
                    continue;
                }

                binaryReader.BaseStream.Position = pos;

                var ch = new CCharInfo();
                ch.Index = i;
                ch.BitDepth = Is4Bit ? 4 : 1;

                ch.Width = binaryReader.ReadUInt16();
                ch.Height = binaryReader.ReadUInt16();
                ch.WidthOriginal = ch.Width;
                ch.HeightOriginal = ch.Height;

                // If zero-size, treat as placeholder and skip data
                if (ch.Width == 0 || ch.Height == 0)
                {
                    ch.ByteLines = null;
                    ch.ByteLinesOriginal = null;
                    Character[i] = ch;
                    continue;
                }

                int bytesPerLine = Is4Bit
                    ? (ch.Width + 1) / 2
                    : (ch.Width + 7) / 8;

                int totalBytes = ch.Height * bytesPerLine;

                // Defensive: don't read beyond stream
                long remaining = binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;
                if (remaining < totalBytes) totalBytes = (int)Math.Max(0, remaining);

                ch.ByteLines = binaryReader.ReadBytes(totalBytes);
                ch.ByteLinesOriginal = new byte[ch.ByteLines.Length];
                Array.Copy(ch.ByteLines, ch.ByteLinesOriginal, ch.ByteLines.Length);

                Character[i] = ch;
            }
        }

        private void ReadWFNFile2(System.IO.BinaryReader reader)
        {
            System.IO.Stream stream = reader.BaseStream;
            long fileLength = stream.Length;

            if (fileLength < 43)
                throw new System.IO.InvalidDataException(
                    "WFN File 2 header is incomplete.");

            uint headerSize = reader.ReadUInt32();
            uint flags = reader.ReadUInt32();
            uint bitsPerPixel = reader.ReadUInt32();
            uint glyphTableAddress = reader.ReadUInt32();
            uint glyphTableSize = reader.ReadUInt32();
            uint characterTableAddress = reader.ReadUInt32();
            uint characterTableSize = reader.ReadUInt32();

            if ((headerSize != 28 && headerSize != 32) ||
                15L + headerSize > fileLength)
            {
                throw new System.IO.InvalidDataException(
                    "Invalid WFN File 2 header size.");
            }

            // Older 28-byte headers have no BaseShadeIndex.
            // Use the default shade 8 when loading these fonts.
            uint baseShadeIndex = 8;

            if (headerSize == 32)
            {
                baseShadeIndex = reader.ReadUInt32();
            }

            if (baseShadeIndex < 1 || baseShadeIndex > 15)
                throw new System.IO.InvalidDataException(
                    "Invalid WFN File 2 base shade index.");

            BaseShadeIndex = baseShadeIndex;

            if (flags != 0)
                throw new System.IO.InvalidDataException(
                    "Unsupported WFN File 2 flags.");

            if (bitsPerPixel != 4)
                throw new System.IO.InvalidDataException(
                    "Unsupported WFN File 2 pixel depth.");

            if (characterTableSize % 8 != 0)
                throw new System.IO.InvalidDataException(
                    "Invalid WFN File 2 character table size.");

            long glyphStart = glyphTableAddress;
            long glyphEnd = glyphStart + glyphTableSize;
            long mapStart = characterTableAddress;
            long mapEnd = mapStart + characterTableSize;
            long headerEnd = 15L + headerSize;

            if (glyphStart < headerEnd || glyphEnd > fileLength ||
                mapStart < headerEnd || mapEnd > fileLength ||
                (glyphStart < mapEnd && mapStart < glyphEnd))
            {
                throw new System.IO.InvalidDataException(
                    "Invalid or overlapping WFN File 2 tables.");
            }

            int entryCount = checked((int)(characterTableSize / 8));
            var mappings = new List<KeyValuePair<int, uint>>(entryCount);

            stream.Position = mapStart;

            int previousCode = -1;
            int highestCode = 0;

            for (int i = 0; i < entryCount; i++)
            {
                uint code = reader.ReadUInt32();
                uint offset = reader.ReadUInt32();

                if (code > 65535 || code <= previousCode)
                    throw new System.IO.InvalidDataException(
                        "Invalid or unsorted WFN File 2 character code.");

                if (offset >= glyphTableSize)
                    throw new System.IO.InvalidDataException(
                        "Invalid WFN File 2 glyph offset.");

                previousCode = (int)code;
                highestCode = (int)code;

                mappings.Add(
                    new KeyValuePair<int, uint>((int)code, offset));
            }

            Character = new CCharInfo[highestCode + 1];
            NumberOfCharacters = Character.Length;

            // Keep missing character slots distinct from mapped glyphs.
            for (int i = 0; i < Character.Length; i++)
            {
                Character[i] = new CCharInfo
                {
                    Index = i,
                    BitDepth = 4,
                    Width = 0,
                    Height = 0,
                    WidthOriginal = 0,
                    HeightOriginal = 0,
                    ByteLines = null,
                    ByteLinesOriginal = null
                };
            }

            foreach (var mapping in mappings)
            {
                long glyphPosition = glyphStart + mapping.Value;

                if (glyphPosition + 4 > glyphEnd)
                    throw new System.IO.InvalidDataException(
                        "Incomplete WFN File 2 glyph record.");

                stream.Position = glyphPosition;

                ushort width = reader.ReadUInt16();
                ushort height = reader.ReadUInt16();

                long stride = (width + 1L) / 2;
                long byteCount = stride * height;

                if (stream.Position + byteCount > glyphEnd)
                    throw new System.IO.InvalidDataException(
                        "WFN File 2 glyph data exceeds its table.");

                byte[] pixels = reader.ReadBytes((int)byteCount);

                CCharInfo glyph = Character[mapping.Key];

                glyph.HasGlyphMapping = true;
                glyph.Width = width;
                glyph.Height = height;
                glyph.WidthOriginal = width;
                glyph.HeightOriginal = height;
                glyph.ByteLines = pixels;
                glyph.ByteLinesOriginal = (byte[])pixels.Clone();
            }
        }

        private void WriteWFNFile2(System.IO.BinaryWriter writer)
        {
            if (Character == null)
                throw new InvalidOperationException("No font data to save.");

            if (Character.Length > 65536)
                throw new InvalidOperationException(
                    "WFN File 2 currently supports character codes 0–65535.");

            // Collect actual glyph mappings in ascending character-code order.
            var mappings = new List<KeyValuePair<uint, uint>>();

            // Signature: 15 bytes.
            writer.Write(StringToByteArray("WGT Font File 2"));

            // Reserve the 32-byte WFN File 2 header.
            long headerPosition = writer.BaseStream.Position;

            for (int i = 0; i < 8; i++)
                writer.Write((uint)0);

            // Glyph-data table begins immediately after the header.
            long glyphTableStart = writer.BaseStream.Position;

            for (int i = 0; i < Character.Length; i++)
            {
                CCharInfo glyph = Character[i];

                if (glyph == null)
                    continue;

                // Preserve explicitly mapped zero-sized glyphs.
                // Also include newly drawn glyphs that have bitmap dimensions.
                bool hasMapping = glyph.HasGlyphMapping ||
                                  (glyph.Width > 0 && glyph.Height > 0);

                if (!hasMapping)
                    continue;

                if (glyph.BitDepth != 4)
                    throw new InvalidOperationException(
                        "Cannot save WFN File 2 containing non-4-bit glyphs.");

                long relativeOffset =
                    writer.BaseStream.Position - glyphTableStart;

                if (relativeOffset > uint.MaxValue)
                    throw new InvalidOperationException(
                        "WFN File 2 glyph table exceeds its offset limit.");

                mappings.Add(new KeyValuePair<uint, uint>(
                    (uint)i, (uint)relativeOffset));

                writer.Write(glyph.Width);
                writer.Write(glyph.Height);

                int stride = (glyph.Width + 1) / 2;
                int byteCount = stride * glyph.Height;

                byte[] pixels = new byte[byteCount];

                if (glyph.ByteLines != null)
                {
                    Array.Copy(
                        glyph.ByteLines,
                        pixels,
                        Math.Min(glyph.ByteLines.Length, pixels.Length));
                }

                writer.Write(pixels);
            }

            long glyphTableEnd = writer.BaseStream.Position;
            long characterTableStart = glyphTableEnd;

            // Sparse character map: code (uint32), offset (uint32).
            foreach (var mapping in mappings)
            {
                writer.Write(mapping.Key);
                writer.Write(mapping.Value);
            }

            long characterTableEnd = writer.BaseStream.Position;

            // Validate 32-bit address and size limits.
            if (glyphTableStart > uint.MaxValue ||
                characterTableStart > uint.MaxValue ||
                glyphTableEnd - glyphTableStart > uint.MaxValue ||
                characterTableEnd - characterTableStart > uint.MaxValue)
            {
                throw new InvalidOperationException(
                    "WFN File 2 exceeds its 32-bit table limits.");
            }

            // Fill in the reserved header.
            writer.BaseStream.Position = headerPosition;

            writer.Write((uint)32); // HeaderSize
            writer.Write((uint)0);  // Flags
            writer.Write((uint)4);  // BitsPerPixel

            writer.Write((uint)glyphTableStart);
            writer.Write((uint)(glyphTableEnd - glyphTableStart));

            writer.Write((uint)characterTableStart);
            writer.Write((uint)(characterTableEnd - characterTableStart));

            if (BaseShadeIndex < 1 || BaseShadeIndex > 15)
                throw new InvalidOperationException(
                    "Base shade index must be between 1 and 15.");

            writer.Write(BaseShadeIndex); // BaseShadeIndex

            // Restore the stream position.
            writer.BaseStream.Position = characterTableEnd;
        }

        private void Write4Bit(System.IO.BinaryWriter writer)
        {
            if (Character == null)
                throw new InvalidOperationException("No font data to save.");

            writer.Write(StringToByteArray("WGT Font 4bit  "));
            writer.Write((UInt32)0); // Offset table position, patched later.

            int lastUsedIndex = -1;

            for (int i = Character.Length - 1; i >= 0; i--)
            {
                CCharInfo glyph = Character[i];

                if (glyph != null && glyph.Width > 0 && glyph.Height > 0)
                {
                    lastUsedIndex = i;
                    break;
                }
            }

            if (lastUsedIndex < 0)
                lastUsedIndex = 0;

            UInt32[] offsets = new UInt32[lastUsedIndex + 1];

            for (int i = 0; i <= lastUsedIndex; i++)
            {
                CCharInfo glyph = i < Character.Length ? Character[i] : null;

                if (glyph == null || glyph.Width == 0 || glyph.Height == 0)
                    continue;

                if (glyph.BitDepth != 4)
                    throw new InvalidOperationException(
                        "Cannot save a 4-bit WFN containing non-4-bit glyphs.");

                long position = writer.BaseStream.Position;

                if (position > UInt32.MaxValue)
                    throw new InvalidOperationException("4-bit WFN exceeds its offset limit.");

                offsets[i] = (UInt32)position;

                writer.Write(glyph.Width);
                writer.Write(glyph.Height);

                int stride = (glyph.Width + 1) / 2;
                int byteCount = stride * glyph.Height;

                byte[] pixels = new byte[byteCount];

                if (glyph.ByteLines != null)
                    Array.Copy(glyph.ByteLines, pixels,
                        Math.Min(glyph.ByteLines.Length, pixels.Length));

                writer.Write(pixels);
            }

            long tablePosition = writer.BaseStream.Position;

            if (tablePosition > UInt32.MaxValue)
                throw new InvalidOperationException("4-bit WFN exceeds its offset limit.");

            foreach (UInt32 offset in offsets)
                writer.Write(offset);

            writer.BaseStream.Position = 15;
            writer.Write((UInt32)tablePosition);
        }

        private static void ThrowWfnTooLarge()
        {
            throw new Exception("Font exceeds 64KB. The WFN format used by AGS does not support files larger than 65,535 bytes.\n\nEither reduce the range or make glyphs smaller; refer to the red text above the font preview.");
        }

        public long EstimateSize()
        {
            if (Character == null)
                return 0;

            long estimatedSize = 17; // header (15 + 2)

            foreach (CCharInfo item in Character)
            {
                if (item == null)
                    continue;

                // Skip placeholder glyphs
                if (item.Height <= 0 || item.Width <= 0)
                    continue;

                estimatedSize += 2; // width
                estimatedSize += 2; // height

                int bytesPerLine = (item.Width - 1) / 8 + 1;
                estimatedSize += item.Height * bytesPerLine;
            }

            // ❌ DO NOT add offset table here

            return estimatedSize;
        }

        public override void Write(System.IO.BinaryWriter binaryWriter)
        {
            if (Is4Bit)
            {
                WriteWFNFile2(binaryWriter);
                return;
            }

            if (Character == null)
                throw new Exception("No font data to save (Character array is null).");

            if (string.IsNullOrEmpty(WFNName))
                WFNName = "WGT Font File  ";

            // Header
            binaryWriter.Write(StringToByteArray(WFNName));
            binaryWriter.Write((UInt16)1); // dummy; will patch later

            // 🔵 Find last non-placeholder glyph
            int lastUsedIndex = -1;

            for (int i = Character.Length - 1; i >= 0; i--)
            {
                CCharInfo ch = Character[i];

                if (ch != null && ch.Width > 0 && ch.Height > 0)
                {
                    lastUsedIndex = i;
                    break;
                }
            }

            // If everything is placeholder, still keep index 0
            if (lastUsedIndex < 0)
                lastUsedIndex = 0;

            // 🔵 Only allocate offset table up to last used glyph
            UInt16[] positionArray = new UInt16[lastUsedIndex + 1];

            // Write glyph data
            for (int i = 0; i <= lastUsedIndex; i++)
            {
                CCharInfo item = Character[i];

                // Placeholder
                if (item == null || item.Width == 0 || item.Height == 0)
                {
                    positionArray[i] = 0;
                    continue;
                }

                // Ensure ByteLines exists
                if (item.ByteLines == null)
                {
                    int bytesPerLine0 = (item.Width - 1) / 8 + 1;
                    item.ByteLines = new byte[item.Height * bytesPerLine0];
                }

                long pos = binaryWriter.BaseStream.Position;

                if (pos > UInt16.MaxValue)
                    throw new Exception("WFN glyph data exceeded 64KB. Reduce glyph count/size.");

                positionArray[i] = (UInt16)pos;

                binaryWriter.Write(item.Width);
                binaryWriter.Write(item.Height);

                int bytesPerLine = (item.Width - 1) / 8 + 1;
                int totalBytes = item.Height * bytesPerLine;

                if (item.ByteLines.Length != totalBytes)
                {
                    byte[] fixedBytes = new byte[totalBytes];
                    int copy = Math.Min(totalBytes, item.ByteLines.Length);
                    Array.Copy(item.ByteLines, fixedBytes, copy);
                    item.ByteLines = fixedBytes;
                }

                binaryWriter.Write(item.ByteLines);

                // Originals bookkeeping
                item.WidthOriginal = item.Width;
                item.HeightOriginal = item.Height;

                item.ByteLinesOriginal = new byte[item.ByteLines.Length];
                Array.Copy(item.ByteLines, item.ByteLinesOriginal, item.ByteLines.Length);
            }

            // Offset table position
            long offsetTablePos = binaryWriter.BaseStream.Position;

            if (offsetTablePos > UInt16.MaxValue)
                throw new Exception("WFN offset table pointer exceeded 64KB. Reduce glyph data size.");

            // Write offset table (ONLY up to lastUsedIndex)
            for (int i = 0; i < positionArray.Length; i++)
                binaryWriter.Write(positionArray[i]);

            // Patch header pointer
            binaryWriter.BaseStream.Position = 15;
            binaryWriter.Write((UInt16)offsetTablePos);
        }
    }

    public class CSCIFontInfo : CFontInfo
    {
        public override void Read(System.IO.BinaryReader binaryReader)
        {
            UInt16 resourcename = binaryReader.ReadUInt16();
            UInt16 alwayszero = binaryReader.ReadUInt16();
            NumberOfCharacters = binaryReader.ReadInt16();
            TextHeight = binaryReader.ReadUInt16();

            UInt16[] positionArray = new UInt16[NumberOfCharacters];
            Character = new CCharInfo[NumberOfCharacters];

            for (int counter = 0; counter < NumberOfCharacters; counter++)
            {
                positionArray[counter] = (UInt16)(binaryReader.ReadUInt16() + 2);
            }

            for (int counter = 0; counter < NumberOfCharacters; counter++)
            {
                binaryReader.BaseStream.Position = positionArray[counter];

                Character[counter] = new CCharInfo();

                Character[counter].Index = counter;
                Character[counter].Width = binaryReader.ReadByte();
                Character[counter].Height = binaryReader.ReadByte();

                Character[counter].WidthOriginal = Character[counter].Width;
                Character[counter].HeightOriginal = Character[counter].Height;

                if (counter == NumberOfCharacters - 1)
                {
                    Character[NumberOfCharacters - 1].ByteLines = binaryReader.ReadBytes((UInt16)(binaryReader.BaseStream.Length - (positionArray[counter] + 2)));
                    Character[NumberOfCharacters - 1].ByteLinesOriginal = new byte[Character[NumberOfCharacters - 1].ByteLines.Length];
                    Array.Copy(Character[NumberOfCharacters - 1].ByteLines, Character[NumberOfCharacters - 1].ByteLinesOriginal, Character[NumberOfCharacters - 1].ByteLines.Length);
                }
                else
                {
                    Character[counter].ByteLines = binaryReader.ReadBytes(positionArray[counter + 1] - (positionArray[counter] + 2));
                    Character[counter].ByteLinesOriginal = new byte[Character[counter].ByteLines.Length];
                    Array.Copy(Character[counter].ByteLines, Character[counter].ByteLinesOriginal, Character[counter].ByteLines.Length);
                }
            }
        }

        public override void Write(System.IO.BinaryWriter binaryWriter)
        {
            int glyphLimit = 256;

            if (Character != null && Character.Length > glyphLimit)
            {
                DialogResult result = MessageBox.Show(
                    $"Warning!\n\nSCI fonts are limited to 256 glyphs (0–255).\n\nThe current font contains {Character.Length} glyphs.\n\nAll glyphs beyond index 255 will NOT be saved.\n\nContinue?",
                    "SCI Glyph Limit",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                    return;

                Array.Resize(ref Character, glyphLimit);
                NumberOfCharacters = glyphLimit;
            }

            binaryWriter.Write((UInt16)0x87);
            binaryWriter.Write((UInt16)0x00);
            binaryWriter.Write((UInt16)Character.Length);

            if (TextHeight == 0)
            {
                binaryWriter.Write((UInt16)9); // assume some value
            }
            else
            {
                binaryWriter.Write((UInt16)TextHeight);
            }

            Int64 offset = binaryWriter.BaseStream.Position;

            foreach (CCharInfo item in Character)
            {
                binaryWriter.Write((UInt16)0);
            }

            UInt16[] positionArray = new UInt16[Character.Length];

            foreach (CCharInfo item in Character)
            {
                if (item == null)
                    continue;

                if (item.ByteLines == null)
                    item.ByteLines = new byte[0];

                positionArray[item.Index] = (UInt16)binaryWriter.BaseStream.Position;

                binaryWriter.Write((byte)item.Width);
                binaryWriter.Write((byte)item.Height);
                binaryWriter.Write(item.ByteLines);

                item.WidthOriginal = item.Width;
                item.HeightOriginal = item.Height;

                item.ByteLinesOriginal = new byte[item.ByteLines.Length];
                Array.Copy(item.ByteLines, item.ByteLinesOriginal, item.ByteLines.Length);
            }

            binaryWriter.BaseStream.Position = offset;

            for (int counter = 0; counter < Character.Length; counter++)
            {
                binaryWriter.Write((UInt16)(positionArray[counter] - 2));
            }
        }
    }

    public static class CFontUtils
    {
        public static void RecreateCharacter(CCharInfo character, UInt16 newwidth, UInt16 newheight)
        {
            // If glyph is empty, just initialize fresh bitmap
            if (character.Width == 0 || character.Height == 0 || character.UnscaledImage == null)
            {
                character.Width = newwidth;
                character.Height = newheight;

                Bitmap blank = new Bitmap(newwidth, newheight, PixelFormat.Format1bppIndexed);
                SaveByteLinesFromPicture(character, blank);

                character.UnscaledImage = blank;
                return;
            }

            Bitmap oldbitmap;
            Bitmap tmpbitmap = new Bitmap(newwidth, newheight);
            Bitmap newbitmap = new Bitmap(newwidth, newheight, PixelFormat.Format1bppIndexed);

            CreateBitmap(character, out oldbitmap);

            Graphics g = Graphics.FromImage(tmpbitmap);
            g.DrawImage(oldbitmap, 0, 0, new Rectangle(0, 0, Math.Min(character.Width, newwidth), Math.Min(character.Height, newheight)), GraphicsUnit.Pixel);

            if ((character.Width - newwidth) < 1)
            {
                g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(character.Width, 0, newwidth, newheight));
            }

            if ((character.Height - newheight) < 1)
            {
                g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, character.Height, newwidth, newheight));
            }

            g.Dispose();

            newbitmap = Indexed.Image.CopyToBpp(tmpbitmap, 1);

            character.Height = newheight;
            character.Width = newwidth;
            SaveByteLinesFromPicture(character, newbitmap);
        }

        public static void CreateBitmap(CCharInfo character, out Bitmap bmp)
        {
            if (character.BitDepth == 4)
            {
                if (character.Width == 0 || character.Height == 0)
                {
                    bmp = null;
                    return;
                }

                bmp = new Bitmap(
                    character.Width,
                    character.Height,
                    PixelFormat.Format32bppArgb);

                int bytesPerLine = (character.Width + 1) / 2;

                for (int y = 0; y < character.Height; y++)
                {
                    for (int x = 0; x < character.Width; x++)
                    {
                        int byteIndex = y * bytesPerLine + x / 2;

                        int shade = 0;

                        if (character.ByteLines != null &&
                            byteIndex < character.ByteLines.Length)
                        {
                            byte packed = character.ByteLines[byteIndex];

                            shade = (x % 2 == 0)
                                ? (packed >> 4)
                                : (packed & 0x0F);
                        }

                        int intensity = shade * 17;

                        bmp.SetPixel(
                            x,
                            y,
                            Color.FromArgb(intensity, intensity, intensity));
                    }
                }

                return;
            }

            if (character.Width == 0 || character.Height == 0)
            {
                bmp = null;
                return;
            }
            else
            {
                bmp = new System.Drawing.Bitmap(character.Width, character.Height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                IntPtr ptr = bmpData.Scan0;

                Int32 bytesPerLine = (bmpData.Width - 1) / 8 + 1;
                Int32 startPos = 0;

                if (character.ByteLines != null && character.ByteLines.Length > 0)
                {
                    startPos = character.ByteLines.Length / bmpData.Height;

                    for (int heightcounter = 0; heightcounter < bmp.Height; heightcounter++)
                    {
                        if ((startPos * heightcounter) + bytesPerLine <= character.ByteLines.Length)
                        {
                            System.Runtime.InteropServices.Marshal.Copy(
                                character.ByteLines,
                                startPos * heightcounter,
                                ptr,
                                bytesPerLine);

                            ptr = (IntPtr)((int)ptr + bmpData.Stride);
                        }
                    }
                }

                bmp.UnlockBits(bmpData);
            }
        }

        public static void SaveByteLinesFromPicture(CCharInfo character, Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            Int32 bytesPerLine = (bmpData.Width - 1) / 8 + 1;

            character.ByteLines = new byte[bmp.Height * bytesPerLine];

            for (int heightcounter = 0; heightcounter < bmp.Height; heightcounter++)
            {
                System.Runtime.InteropServices.Marshal.Copy(
                    ptr,
                    character.ByteLines,
                    bytesPerLine * heightcounter,
                    bytesPerLine);

                ptr = (IntPtr)((int)ptr + bmpData.Stride);
            }

            bmp.UnlockBits(bmpData);
        }

        public static void SaveOneFont(CFontInfo font)
        {
            if (font == null)
                return;

            if (font is CWFNFontInfo wfn)
            {
                if (wfn.Character == null)
                    throw new Exception("Cannot save: Character array is null.");

                foreach (CCharInfo item in wfn.Character)
                {
                    if (item == null)
                        throw new Exception("Cannot save: Character array contains null glyph entries.");
                }

                if (!wfn.Is4Bit && wfn.EstimateSize() > 65535)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Font exceeds 64KB. The WFN format used by AGS does not support files larger than 65,535 bytes.",
                        "Save Error",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);

                    return;
                }
            }

            using (var fs = System.IO.File.Open(font.FontPath, System.IO.FileMode.Create))
            using (var bw = new System.IO.BinaryWriter(fs))
            {
                font.Write(bw);
            }
        }



        public static void ScaleBitmap(Bitmap bitmap, out Bitmap newbmp, Int32 scale)
        {
            if (bitmap == null)
            {
                newbmp = null;
                return;
            }

            newbmp = new Bitmap(bitmap.Width * scale, bitmap.Height * scale);

            using (Graphics g = Graphics.FromImage(newbmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(bitmap, 0, 0, newbmp.Width, newbmp.Height);
            }
        }
    }
}
