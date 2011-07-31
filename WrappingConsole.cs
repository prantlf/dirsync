// Copyright (C) 2009-2011 Ferdinand Prantl <prantlf@gmail.com>
// All rights reserved.       
//
// This file is part of dirsync - directory content synchronization tool.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace dirsync
{
    static class WrappingConsole
    {
        public static void Write(IFormatProvider provider, string format,
            params object[] args) {
                Write(provider, 0, format, args);
        }

        public static void Write(string format, params object[] args) {
            Write(null, 0, format, args);
        }

        public static void Write(int indent, string format, params object[] args) {
            Write(null, indent, format, args);
        }

        public static void Write(IFormatProvider provider, int indent, string format,
            params object[] args) {
            Write(indent, string.Format(provider, format, args));
        }

        public static void Write(string value) {
            Write(0, value);
        }

        public static void Write(object value) {
            Write(0, value);
        }

        public static void Write(int indent, object value) {
            Write(indent, value != null ? value.ToString() : null);
        }

        public static void Write(int indent, string value) {
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
        }

        public static void WriteLine(IFormatProvider provider, string format,
            params object[] args) {
            Write(provider, format, args);
            Console.WriteLine();
        }

        public static void WriteLine(string format, params object[] args) {
            Write(format, args);
            Console.WriteLine();
        }

        public static void WriteLine(int indent, string format, params object[] args) {
            Write(indent, format, args);
            Console.WriteLine();
        }

        public static void WriteLine(IFormatProvider provider, int indent, string format,
            params object[] args) {
            Write(provider, indent, format, args);
            Console.WriteLine();
        }

        public static void WriteLine(string value) {
            Write(value);
            Console.WriteLine();
        }

        public static void WriteLine(object value) {
            Write(value);
            Console.WriteLine();
        }

        public static void WriteLine(int indent, object value) {
            Write(indent, value);
            Console.WriteLine();
        }

        public static void WriteLine(int indent, string value) {
            Write(indent, value);
            Console.WriteLine();
        }
    }
}
