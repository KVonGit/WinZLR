/*  MIT License

    Copyright(c) 2021-2025 Henrik Åsman

    Permission Is hereby granted, free Of charge, to any person obtaining a copy
    of this software And associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
    copies of the Software, And to permit persons to whom the Software Is
    furnished to do so, subject to the following conditions:

    The above copyright notice And this permission notice shall be included In all
    copies Or substantial portions of the Software.

    THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
    IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
    LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
    OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
    SOFTWARE.                                                                         */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// Description of XML debug-format: https://gist.github.com/EmacsUser/3960569
// Description of DBG debug-format: https://inform-fiction.org/source/tm/chapter12.txt

namespace winZLR
{
    internal class DebugWriter
    {
        public static void FixActualFile(string fromDBG, string toDBG, List<string> filenames)
        {
            Stream fromStream = new FileStream(fromDBG, FileMode.Open, FileAccess.Read);
            DebugInfo di = new(fromStream);

            // open streams (fromStream need reopening, DebugInfo closed it)
            fromStream = new FileStream(fromDBG, FileMode.Open, FileAccess.Read);
            Stream toStream = System.IO.File.OpenWrite(toDBG);
            
            int n = 0;
            for (int i = 0; i <= fromStream.Length; i++)
            {
                if (di.ActualFilePositions.Contains(i))
                {
                    WriteDebugString(toStream, filenames[n]);
                    n++;
                    string name = di.Filenames[(byte)n];
                    i += name.Length + 1;
                    fromStream.Position += name.Length + 1;
                }
                else 
                {
                    toStream.WriteByte((byte)fromStream.ReadByte());
                }
            }
            fromStream.Close();
            toStream.Close();  
        }

        public static void XmlToDbg(string filenameXML, string filenameDBG)
        {
            // See "Inform 6 Technical Manual", ch 12.5

            // TODO: <fake-action> as <action> also?
            // TODO: <class> objects as <object> too?
            // TODO: <content-creator-version> --> creator version
            int a;
            byte b;
            ushort w, rtn = 0;
            string str;

            string debugFile = System.IO.Path.GetTempFileName();
            Stream stream = System.IO.File.OpenWrite(debugFile);

            XmlDocument doc = new();
            try
            {
                doc.Load(filenameXML);
            }
            catch
            {
                throw new InvalidDataException("File not in expected format.");
            }
            if (doc.SelectSingleNode("/inform-story-file") == null) throw new InvalidDataException("File is not an Inform6 dbg-file.");

            // Get some useful constants
            int arrayStart = 0;         // #array__start
            int globalsStart = 0;       // #globals_array
            XmlNodeList? nodes = doc.SelectNodes("/inform-story-file/constant");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    str = node.SelectSingleNode("identifier")!.InnerText;
                    if (str == "#array__start") arrayStart = Convert.ToInt32(node.SelectSingleNode("value")!.InnerText); ;
                    if (str == "#globals_array") globalsStart = Convert.ToInt32(node.SelectSingleNode("value")!.InnerText); ;
                }
            }

            // A debugging information file begins with a six-byte header:
            //  0,1  the bytes $DE and then $BF (DEBF = "Debugging File")
            //  2,3  a word giving the version number of the format used(currently 0)
            //  4,5  a word giving the current Inform version number, in its traditional decimal form: e.g. 1612 means "6.12"
            WriteDebugWord(stream, 0xDEBF);     // magic number
            WriteDebugWord(stream, 0);          // file format
            WriteDebugWord(stream, 1643);       // creator version

            // FILE_DBR             (byte: 1)
            //  <file number>       1 byte, counting from 1         <source index>
            //  <include name>      string                          <source>/<given-path>
            //  <actual file name>  string                          <source>/<resolved-path>
            // One of these records always appears before any reference to the source code file in question.
            nodes = doc.SelectNodes("/inform-story-file/source");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    b = Convert.ToByte(node.Attributes!["index"]!.Value);
                    str = node.SelectSingleNode("given-path")!.InnerText;

                    string path = "";
                    XmlNode? resolvedPath = node.SelectSingleNode("resolved-path");
                    if (resolvedPath != null) 
                        path = resolvedPath.InnerText; 
                    else 
                        path = str;

                    WriteDebugByte(stream, 1);
                    WriteDebugByte(stream, (byte)(b + 1));       // zero-based in xml
                    WriteDebugString(stream, str);
                    WriteDebugString(stream, path);
                }
            }

            // CLASS_DBR            (byte: 2)
            //  <name>              string                          <class>/<identifier>     
            //  <defn start>        line                            0:0:0 or <class>/<source-code-location>/<file-index>, <line> & <character>
            //  <defn end>          line                            0:0:0 or <class>/<source-code-location>/<file-index>, <end-line> & <end-character>
            nodes = doc.SelectNodes("/inform-story-file/class");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    byte file = 0, character = 0, end_character = 0;
                    ushort line = 0, end_line = 0;
                    str = node.SelectSingleNode("identifier")!.InnerText;
                    XmlNode? childNode = node.SelectSingleNode("source-code-location");
                    if (childNode != null)
                    {
                        file = Convert.ToByte(childNode.SelectSingleNode("file-index")!.InnerText);
                        line = Convert.ToUInt16(childNode.SelectSingleNode("line")!.InnerText);
                        character = Convert.ToByte(childNode.SelectSingleNode("character")!.InnerText);
                        end_line = Convert.ToUInt16(childNode.SelectSingleNode("end-line")!.InnerText);
                        end_character = Convert.ToByte(childNode.SelectSingleNode("end-character")!.InnerText);
                    }

                    WriteDebugByte(stream, 2);
                    WriteDebugString(stream, str);
                    WriteDebugLine(stream, file, line, character);
                    WriteDebugLine(stream, file, end_line, end_character);
                }
            }

            // OBJECT_DBR           (byte: 3)
            //  <number>            word                            <object>/<value>
            //  <name>              string                          <object>/<identifier>     
            //  <defn start>        line                            0:0:0 or <object>/<source-code-location>/<file-index>, <line> & <character>
            //  <defn end>          line                            0:0:0 or <object>/<source-code-location>/<file-index>, <end-line> & <end-character>
            nodes = doc.SelectNodes("/inform-story-file/object");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    byte file = 0, character = 0, end_character = 0;
                    ushort line = 0, end_line = 0;
                    w = Convert.ToUInt16(node.SelectSingleNode("value")!.InnerText);
                    str = node.SelectSingleNode("identifier")!.InnerText;
                    XmlNode? childNode = node.SelectSingleNode("source-code-location");
                    if (childNode != null)
                    {
                        file = Convert.ToByte(childNode.SelectSingleNode("file-index")!.InnerText);
                        line = Convert.ToUInt16(childNode.SelectSingleNode("line")!.InnerText);
                        character = Convert.ToByte(childNode.SelectSingleNode("character")!.InnerText);
                        end_line = Convert.ToUInt16(childNode.SelectSingleNode("end-line")!.InnerText);
                        end_character = Convert.ToByte(childNode.SelectSingleNode("end-character")!.InnerText);
                    }

                    WriteDebugByte(stream, 3);
                    WriteDebugWord(stream, w);
                    WriteDebugString(stream, str);
                    WriteDebugLine(stream, file, line, character);
                    WriteDebugLine(stream, file, end_line, end_character);
                }
            }

            // GLOBAL_DBR           (byte: 4)
            //  <number>            byte                            <global-variable>/((<address>-min-address)/2)
            //  <name>              string                          <global-variable>/<identifier>
            nodes = doc.SelectNodes("/inform-story-file/global-variable");
            if (nodes != null)
            {
                bool redundantSys__Glob = false;

                // Find start of globals
                if (globalsStart == 0) 
                {
                    foreach (XmlNode node in nodes)
                    {
                        w = Convert.ToUInt16(node.SelectSingleNode("address")!.InnerText);
                        if (globalsStart == w) redundantSys__Glob = true;
                        if (globalsStart == 0 || w < globalsStart) globalsStart = w;
                    }
                }

                foreach (XmlNode node in nodes)
                {
                    b = (byte)((Convert.ToUInt16(node.SelectSingleNode("address")!.InnerText) - globalsStart) / 2);
                    str = node.SelectSingleNode("identifier")!.InnerText;

                    // Don't write sys__glob0, sys__glob1 & sys__glob2 if version > 3
                    if (!redundantSys__Glob || !str.Contains("sys__glob", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteDebugByte(stream, 4);
                        WriteDebugByte(stream, b);
                        WriteDebugString(stream, str);
                    }
                }
            }

            // ARRAY_DBR            (byte: 12)
            //  <byte address>      word                    <array>/<value> - codebaseGlobals
            //  <name>              string                  <array>/<identifier>
            // The byte address is an offset within the "array space" area, which always begins with the 480 bytes
            // storing the values of the global variables.
            nodes = doc.SelectNodes("/inform-story-file/array");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    int actualArrayOffset = 480;
                    if (arrayStart > 0) actualArrayOffset = arrayStart - globalsStart;
                    w = Convert.ToUInt16(node.SelectSingleNode("value")!.InnerText);
                    str = node.SelectSingleNode("identifier")!.InnerText;

                    WriteDebugByte(stream, 12);
                    WriteDebugWord(stream, (ushort)(w - globalsStart - (480 - actualArrayOffset)));     // Inform6 always assumes 480, but we need to compensate for ZCODE_COMPACT_GLOBALS 
                    WriteDebugString(stream, str);
                }
            }

            // ATTR_DBR             (byte: 5)
            //  <number>            word                    <attribute>/<value>
            //  <name>              string                  <attribute>/<identifier>
            nodes = doc.SelectNodes("/inform-story-file/attribute");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    w = Convert.ToUInt16(node.SelectSingleNode("value")!.InnerText);
                    str = node.SelectSingleNode("identifier")!.InnerText;

                    WriteDebugByte(stream, 5);
                    WriteDebugWord(stream, w);
                    WriteDebugString(stream, str);
                }
            }

            // PROP_DBR             (byte: 6)
            //  <number>            word                    <property>/<value>
            //  <name>              string                  <property>/<identifier>
            nodes = doc.SelectNodes("/inform-story-file/property");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    w = Convert.ToUInt16(node.SelectSingleNode("value")!.InnerText);
                    str = node.SelectSingleNode("identifier")!.InnerText;

                    WriteDebugByte(stream, 6);
                    WriteDebugWord(stream, w);
                    WriteDebugString(stream, str);
                }
            }

            // FAKE_ACTION_DBR      (byte: 7)
            //  <number>            word                    <fake-action>/<value>
            //  <name>              string                  <fake-action>/<identifier>
            nodes = doc.SelectNodes("/inform-story-file/fake-action");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    w = Convert.ToUInt16(node.SelectSingleNode("value")!.InnerText);
                    str = node.SelectSingleNode("identifier")!.InnerText;

                    WriteDebugByte(stream, 7);
                    WriteDebugWord(stream, w);
                    WriteDebugString(stream, str);
                }
            }

            // ACTION_DBR           (byte: 8)
            //  <number>            word                    <action>/<value>
            //  <name>              string                  <action>/<identifier>
            nodes = doc.SelectNodes("/inform-story-file/action");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    w = Convert.ToUInt16(node.SelectSingleNode("value")!.InnerText);
                    str = node.SelectSingleNode("identifier")!.InnerText;

                    WriteDebugByte(stream, 8);
                    WriteDebugWord(stream, w);
                    WriteDebugString(stream, str);
                }
            }

            // HEADER_DBR           (byte: 9)
            //  <the header>        64 bytes                <story-file-prefix>
            nodes = doc.SelectNodes("/inform-story-file/story-file-prefix");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    str = node.InnerText;

                    WriteDebugByte(stream, 9);
                    stream.Write(Convert.FromBase64String(str), 0, 64);
                }
            }

            // ROUTINE_DBR          (byte: 11)
            //  <routine number>    word                    <routine>/<value> [Should be 0.., but are packed address]
            //  <defn start>        line                    0:0:0 or <routine>/<source-code-location>/<file-index>, <line> & <character>
            //  <PC start>          address                 <routine>/<address>
            //  <name>              string                  <routine>/<identifier>
            //  then for each local variable:
            //  <local name>        string                  <routine>/<local-variable>/<identifier>
            //  terminated by a zero byte.
            //  Note that the PC start address is in bytes, relative to the start of the story file's code area.
            //  Routines are numbered upward from 0, and in each case the ROUTINE_DBR, LINEREF_DBR and
            //  ROUTINE_END_DBR records occur in order.
            nodes = doc.SelectNodes("/inform-story-file/routine");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    byte file = 0, character = 0, end_character = 0;
                    ushort line = 0, end_line = 0, byte_count = 0;
                    w = Convert.ToUInt16(node.SelectSingleNode("value")!.InnerText);        // this will be ignored, routines are numbered 0.. instead
                    XmlNode? childNode = node.SelectSingleNode("source-code-location");
                    if (childNode != null)
                    {
                        file = Convert.ToByte(childNode.SelectSingleNode("file-index")!.InnerText);
                        line = Convert.ToUInt16(childNode.SelectSingleNode("line")!.InnerText);
                        character = Convert.ToByte(childNode.SelectSingleNode("character")!.InnerText);
                        if (childNode.SelectSingleNode("end-line") == null)
                        {
                            // No end-pont defined, it's the same as the starting point
                            end_line = line;
                            end_character = character;
                        }
                        else
                        {
                            end_line = Convert.ToUInt16(childNode.SelectSingleNode("end-line")!.InnerText);
                            end_character = Convert.ToByte(childNode.SelectSingleNode("end-character")!.InnerText);
                        }
                    }
                    a = Convert.ToInt32(node.SelectSingleNode("address")!.InnerText);
                    str = node.SelectSingleNode("identifier")!.InnerText;
                    byte_count = Convert.ToUInt16(node.SelectSingleNode("byte-count")!.InnerText);

                    WriteDebugByte(stream, 11);
                    WriteDebugWord(stream, rtn);
                    WriteDebugLine(stream, file, line, character);
                    WriteDebugAddress(stream, a);
                    WriteDebugString(stream, str);

                    XmlNodeList? locals = node.SelectNodes("local-variable");
                    if (locals != null)
                    {
                        foreach (XmlNode local in locals)
                        {
                            str = local.SelectSingleNode("identifier")!.InnerText;
                            WriteDebugString(stream, str);
                        }
                    }
                    WriteDebugByte(stream, 0);

                    // LINEREF_DBR                      (byte: 10)
                    //  <routine number>                word        same as above (w)
                    //  <number of sequence points>     word        calculate
                    //  and then, for each sequence point:
                    //  <source code position>          line        0:0:0 or <sequence-point>/<source-code-location>/<file-index>, <line> & <character>
                    //  <PC offset>                     word        <sequence-point>/<address> - a
                    //  The PC offset for each sequence point is in bytes, from the start of the routine.  (Note that the initial
                    //  byte of the routine, giving the number of local variables for that routine, is at PC offset 0: thus the actual
                    //  code begins at PC offset 1.)  It is possible for a routine to have no sequence points (as in the veneer, or
                    //  in the case of code reading simply "[; ];").
                    WriteDebugByte(stream, 10);
                    WriteDebugWord(stream, rtn);
                    XmlNodeList? seqPoints = node.SelectNodes("sequence-point");
                    if (seqPoints == null)
                        WriteDebugWord(stream, 0);
                    else
                    {
                        WriteDebugWord(stream, (ushort)seqPoints.Count);
                        foreach (XmlNode seqPoint in seqPoints)
                        {
                            byte seqFile = 0, seqCharacter = 0;
                            ushort seqLine = 0, offset = 0;
                            int address = 0;
                            childNode = seqPoint.SelectSingleNode("source-code-location");
                            if (childNode != null)
                            {
                                seqFile = Convert.ToByte(childNode.SelectSingleNode("file-index")!.InnerText);
                                seqLine = Convert.ToUInt16(childNode.SelectSingleNode("line")!.InnerText);
                                seqCharacter = Convert.ToByte(childNode.SelectSingleNode("character")!.InnerText);
                            }
                            address = Convert.ToInt32(seqPoint.SelectSingleNode("address")!.InnerText);
                            offset = (ushort)(address - a);

                            WriteDebugLine(stream, seqFile, seqLine, seqCharacter);
                            WriteDebugWord(stream, offset);
                        }
                    }

                    // ROUTINE_END_DBR                  (byte: 14)
                    //  <routine number>                word        same as above (w)
                    //  <defn end>                      line        <file>, <end-line> & <end-character>
                    //  <next PC value>                 address     a + <byte-count>
                    WriteDebugByte(stream, 14);
                    WriteDebugWord(stream, rtn);
                    WriteDebugLine(stream, file, end_line, end_character);
                    WriteDebugAddress(stream, a + byte_count);

                    rtn++;      // advance routine number
                }
            }

            // MAP_DBR                  (byte: 13)
            //  A sequence of records consisting of:
            //  <name of structure>     string          <story-file-section>/<type>
            //  <location>              address         <story-file-section>/<address>
            nodes = doc.SelectNodes("/inform-story-file/story-file-section");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    str = node.SelectSingleNode("type")!.InnerText;
                    a = Convert.ToInt32(node.SelectSingleNode("address")!.InnerText);

                    WriteDebugByte(stream, 13);
                    WriteDebugString(stream, str);
                    WriteDebugAddress(stream, a);
                }
                WriteDebugByte(stream, 0);
            }

            // EOF_DBR              (byte: 0)
            // End of the debugging file.
            WriteDebugByte(stream, 0);

            stream.Close();

            // Finish up
            File.Delete(filenameDBG);
            File.Move(debugFile, filenameDBG);    
        }

        private static void WriteDebugByte(Stream stream, byte b)
        {
            stream.WriteByte(b);
        }

        private static void WriteDebugWord(Stream stream, ushort w)
        {
            stream.WriteByte((byte)(w >> 8));
            stream.WriteByte((byte)w);
        }

        private static void WriteDebugAddress(Stream stream, int a)
        {
            stream.WriteByte((byte)(a >> 16));
            stream.WriteByte((byte)(a >> 8));
            stream.WriteByte((byte)a);
        }

        private static void WriteDebugLine(Stream stream, byte file, ushort line, byte character)
        {
            if (file != 0 || line != 0 || character != 0) file++;       // compensate for zero-based in xml
            stream.WriteByte(file);
            stream.WriteByte((byte)(line >> 8));
            stream.WriteByte((byte)line);
            stream.WriteByte(character);
        }

        private static void WriteDebugString(Stream stream, string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }

    }
}
