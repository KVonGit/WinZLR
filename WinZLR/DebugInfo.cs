﻿using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace winZLR
{
    /* The DebugInfo class is a modified version of the same class in ZLR, by Tara McGrew */

    public class DebugInfo
    {
        public List<long> ActualFilePositions = [];

        public int Version = -1;
        private struct LineRef
        {
            public byte FileNum;
            public ushort LineNum;
            public byte Column;

            public readonly bool IsValid => FileNum != 0 && FileNum != 255;
        }

        private readonly byte[]? matchingHeader;

        public DebugInfo([NotNull] Stream fromStream)
        {
            using var br = new BinaryReader(fromStream);

            if (ReadWord(br) != 0xDEBF)
                throw new ArgumentException("Invalid debug file header", nameof(fromStream));
            if (ReadWord(br) != 0)
                throw new ArgumentException("Unrecognized debug file version", nameof(fromStream));
            Version = ReadWord(br);

            var codeArea = 0;
            RoutineInfo? routine = null;
            var localList = new List<string>();
            var lineList = new List<LineInfo>();
            var offsetList = new List<ushort>();

            while (fromStream.Position < fromStream.Length)
            {
                var type = br.ReadByte();

                try
                {
                    int i;
                    byte b;
                    ushort w;
                    string str;
                    LineRef line;

                    /**
                     * Within each case, calls that may throw (like <see cref="DoubleMap{TKey, TValue}.Add(TKey, TValue)"/>)
                     * should happen after reads, to allow for error recovery.
                     */
                    switch (type)
                    {
                        case 0:
                            // EOF_DBR
                            fromStream.Seek(0, SeekOrigin.End);
                            break;

                        case 1:
                            // FILE_DBR
                            b = br.ReadByte();
                            ReadString(br); // skip include name
                            ActualFilePositions.Add(fromStream.Position);
                            str = ReadString(br);
                            Filenames[b] = str;
                            break;

                        case 2:
                            // CLASS_DBR
                            ReadString(br);
                            ReadLineRef(br);
                            ReadLineRef(br);
                            break;

                        case 3:
                            // OBJECT_DBR
                            var obj = new ObjectInfo();
                            this.Objects.Add(obj);
                            obj.Number = ReadWord(br);
                            obj.Name = ReadString(br);
                            line = ReadLineRef(br);
                            if (line.IsValid)
                            {
                                obj.DefinedAt = new LineInfo(
                                    Filenames[line.FileNum],
                                    line.LineNum,
                                    line.Column);
                            }

                            ReadLineRef(br);
                            break;

                        case 4:
                            // GLOBAL_DBR
                            b = br.ReadByte();
                            str = ReadString(br);
                            this.Globals.Add(str, b);
                            break;

                        case 12:
                            // ARRAY_DBR
                            w = ReadWord(br);
                            str = ReadString(br);
                            this.Arrays.Add(str, w);
                            break;

                        case 5:
                            // ATTR_DBR
                            w = ReadWord(br);
                            str = ReadString(br);
                            this.Attributes.Add(str, w);
                            break;

                        case 6: // PROP_DBR
                            w = ReadWord(br);
                            str = ReadString(br);
                            this.Properties.Add(str, w);
                            break;

                        case 7: // FAKE_ACTION_DBR
                        case 8: // ACTION_DBR
                            w = ReadWord(br);
                            str = ReadString(br);
                            this.Actions.Add(str, w);
                            break;

                        case 9:
                            // HEADER_DBR
                            matchingHeader = br.ReadBytes(64);
                            break;

                        case 11:
                            // ROUTINE_DBR
                            routine = new RoutineInfo();
                            ReadWord(br);
                            line = ReadLineRef(br);
                            if (line.IsValid)
                            {
                                routine.DefinedAt = new LineInfo(
                                    Filenames[line.FileNum],
                                    line.LineNum,
                                    line.Column);
                            }

                            routine.CodeStart = ReadAddress(br);
                            routine.Name = ReadString(br);
                            localList.Clear();
                            while ((str = ReadString(br)) != "")
                            {
                                localList.Add(str);
                            }

                            routine.Locals = [.. localList];
                            lineList.Clear();
                            offsetList.Clear();
                            this.Routines.Add(routine);
                            break;

                        case 10:
                            // LINEREF_DBR
                            ReadWord(br);
                            w = ReadWord(br);
                            while (w-- > 0)
                            {
                                line = ReadLineRef(br);
                                var w2 = ReadWord(br);
                                if (line.IsValid)
                                {
                                    lineList.Add(new LineInfo(
                                        Filenames[line.FileNum],
                                        line.LineNum,
                                        line.Column));
                                    offsetList.Add(w2);
                                }
                            }

                            break;

                        case 14:
                            // ROUTINE_END_DBR
                            // assume routine is still set from earlier...
                            ReadWord(br);    // skip routine number
                            ReadLineRef(br); // skip defn end
                            i = ReadAddress(br);
                            if (routine != null)
                            {
                                routine.CodeLength = i - routine.CodeStart;
                                routine.LineInfos = [.. lineList];
                                routine.LineOffsets = [.. offsetList];
                            }

                            break;

                        case 13:
                            // MAP_DBR
                            for (str = ReadString(br); str != ""; str = ReadString(br))
                            {
                                i = ReadAddress(br);
                                if (str == "code area")
                                    codeArea = i;
                            }

                            break;

                        default:
                            // unknown
                            throw new InvalidDataException(
                                $"Unrecognized DBR block type '{type}' at offset {fromStream.Position - 1}");
                    }
                }
                catch (InvalidOperationException)
                {
                    // recover
                    System.Diagnostics.Debug.WriteLine("");
                }
            }

            // patch routine addresses
            foreach (var ri in this.Routines)
                ri.CodeStart += codeArea;

            this.Routines.Sort((r1, r2) => r1.CodeStart - r2.CodeStart);
        }

        private static ushort ReadWord([NotNull] BinaryReader rdr)
        {
            var b1 = rdr.ReadByte();
            var b2 = rdr.ReadByte();
            return (ushort)((b1 << 8) + b2);
        }

        private static int ReadAddress([NotNull] BinaryReader rdr)
        {
            var b1 = rdr.ReadByte();
            var b2 = rdr.ReadByte();
            var b3 = rdr.ReadByte();
            return (b1 << 16) + (b2 << 8) + b3;
        }

        private static string ReadString([NotNull] BinaryReader rdr)
        {
            var sb = new StringBuilder();
            var b = rdr.ReadByte();
            while (b != 0)
            {
                sb.Append((char)b);
                b = rdr.ReadByte();
            }
            return sb.ToString();
        }

        private static LineRef ReadLineRef([NotNull] BinaryReader rdr)
        {
            LineRef result;
            result.FileNum = rdr.ReadByte();
            result.LineNum = ReadWord(rdr);
            result.Column = rdr.ReadByte();
            return result;
        }

        public bool MatchesGameFile(Stream gameFile)
        {
            if (matchingHeader == null)
                return true;

            var gameHeader = new byte[64];
            gameFile.Seek(0, SeekOrigin.Begin);
            var len = gameFile.Read(gameHeader, 0, 64);
            if (len < 64)
                return false;

            for (var i = 0; i < 64; i++)
                if (gameHeader[i] != matchingHeader[i])
                    return false;

            return true;
        }

        [NotNull]
        public List<RoutineInfo> Routines { get; } = [];

        [NotNull]
        public List<ObjectInfo> Objects { get; } = [];

        [NotNull]
        public DoubleMap<string, byte> Globals { get; } = [];

        [NotNull]
        public DoubleMap<string, ushort> Arrays { get; } = [];

        [NotNull]
        public DoubleMap<string, ushort> Attributes { get; } = [];

        [NotNull]
        public DoubleMap<string, ushort> Properties { get; } = [];

        [NotNull]
        public DoubleMap<string, ushort> Actions { get; } = [];

        public Dictionary<byte, string> Filenames { get; } = new Dictionary<byte, string>(5);

        public RoutineInfo? FindRoutine(int pc)
        {
            int start = 0, end = Routines.Count;

            while (start < end)
            {
                var mid = (start + end) / 2;

                var ri = Routines[mid];
                if (pc >= ri.CodeStart && pc < ri.CodeStart + ri.CodeLength)
                    return ri;

                if (pc > ri.CodeStart)
                    start = mid + 1;
                else
                    end = mid;
            }

            return null;
        }

        public RoutineInfo? FindRoutine([NotNull] string name) => Routines.FirstOrDefault(t => t.Name == name);

        public ObjectInfo? FindObject(int number) => Objects.FirstOrDefault(t => t.Number == number);

        public ObjectInfo? FindObject([NotNull] string name) => Objects.FirstOrDefault(t => t.Name == name);

        public LineInfo? FindLine(int pc)
        {
            var rtn = FindRoutine(pc);
            if (rtn == null)
                return null;

            var offset = (ushort)(pc - rtn.CodeStart);
            var idx = Array.BinarySearch(rtn.LineOffsets, offset);
            if (idx >= 0)
                return rtn.LineInfos[idx];

            idx = ~idx - 1;
            if (idx >= 0 && idx < rtn.LineInfos.Length)
                return rtn.LineInfos[idx];

            return null;
        }

        public int FindCodeAddress([NotNull] string filename, int line)
        {
            foreach (var rtn in Routines)
            {
                for (var j = 0; j < rtn.LineInfos.Length; j++)
                    if (rtn.LineInfos[j].File == filename && rtn.LineInfos[j].Line == line)
                        return rtn.CodeStart + rtn.LineOffsets[j];
            }

            return -1;
        }
    }

    public class RoutineInfo
    {
        public string Name = "";
        public int CodeStart;
        public int CodeLength;
        public LineInfo DefinedAt;
        public string[] Locals = [];
        public ushort[] LineOffsets=[];
        public LineInfo[] LineInfos = [];
    }

    public class ObjectInfo
    {
        public string? Name;
        public int Number;
        public LineInfo DefinedAt;
    }

    public readonly struct LineInfo(string file, int line, int position)
    {
        public readonly string File = file;
        public readonly int Line = line;
        public readonly int Position = position;

        public override int GetHashCode()
        {
            return HashCode.Combine(Line, Position, File);
        }

        public override bool Equals(object? obj) => obj is LineInfo li && Equals(li);

        public bool Equals(LineInfo other) =>
            File == other.File &&
            Line == other.Line &&
            Position == other.Position;

        public static bool operator ==(LineInfo a, LineInfo b) => a.Equals(b);

        public static bool operator !=(LineInfo a, LineInfo b) => !a.Equals(b);
    }

    public class DoubleMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> 
        where TKey : notnull 
        where TValue : notnull
    {
        public readonly Dictionary<TKey, TValue> forward = [];
        public readonly Dictionary<TValue, TKey> backward = [];

        public int Count => forward.Count;

        [NotNull]
        public IEnumerable<TKey> Keys => forward.Keys;

        [NotNull]
        public IEnumerable<TValue> Values => forward.Values;

        public void Add([NotNull] TKey key, [NotNull] TValue value)
        {
            if (forward.ContainsKey(key))
                throw new InvalidOperationException("key already present");
            if (backward.ContainsKey(value))
                throw new InvalidOperationException("value already present");
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            forward.Add(key, value);
            backward.Add(value, key);
        }

        public void Remove([NotNull] TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!forward.TryGetValue(key, out var value))
                return;

            forward.Remove(key);
            backward.Remove(value);
        }

        public void Remove([NotNull] TValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!backward.TryGetValue(value, out var key))
                return;

            forward.Remove(key);
            backward.Remove(value);
        }

        public bool Contains([NotNull] TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return forward.ContainsKey(key);
        }

        public bool Contains([NotNull] TValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return backward.ContainsKey(value);
        }

        public TValue this[[NotNull] TKey key] => forward[key];

        public TKey this[[NotNull] TValue value] => backward[value];

        // ReSharper disable once AnnotateNotNullTypeMember
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => forward.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
