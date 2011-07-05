// Copyright (C) 2009-2011 Ferdinand Prantl <prantlf@gmail.com>
// All rights reserved.       
//
// This file is part of dirsync - directory content synchronization tool.
//
// dirsync is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// dirsync is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with dirsync.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace dirsync
{
    public static class ConsoleExtension
    {
        public static void WriteWrappedLine(IFormatProvider provider, string format,
            params object[] args) {
                WriteWrappedLine(provider, 0, format, args);
        }

        public static void WriteWrappedLine(string format, params object[] args) {
            WriteWrappedLine(null, 0, format, args);
        }

        public static void WriteWrappedLine(int indent, string format, params object[] args) {
            WriteWrappedLine(null, indent, format, args);
        }

        public static void WriteWrappedLine(IFormatProvider provider, int indent, string format,
            params object[] args) {
            WriteWrappedLine(indent, string.Format(provider, format, args));
        }

        public static void WriteWrappedLine(string value) {
            WriteWrappedLine(0, value);
        }

        public static void WriteWrappedLine(object value) {
            WriteWrappedLine(0, value);
        }

        public static void WriteWrappedLine(int indent, object value) {
            WriteWrappedLine(indent, value != null ? value.ToString() : null);
        }

        public static void WriteWrappedLine(int indent, string value) {
            if (value != null) {
                string prefix = null;
                var width = 79;
                while (value.Length > width) {
                    var index = value.LastIndexOf(' ', width);
                    if (index < 0) {
                        index = value.IndexOf(' ', width);
                        if (index < 0)
                            break;
                    }
                    Console.WriteLine(value.Substring(0, index));
                    value = value.Substring(index + 1);
                    if (prefix == null) {
                        prefix = new string(' ', indent);
                        width = 79 - indent;
                    }
                    Console.Write(prefix);
                }
                Console.Write(value);
            }
            Console.WriteLine();
        }
    }
}
