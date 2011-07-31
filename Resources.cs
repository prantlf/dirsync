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
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace dirsync
{
    static class Resources
    {
        public static string GetString(string name) {
            return GetString(null, name);
        }

        public static string GetString(string name, params object[] args) {
            return GetString(null, name, args);
        }

        public static string GetString(CultureInfo culture, string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            var text = strings.GetString(name, culture);
            if (text == null)
                throw new ArgumentException(string.Format(
                    "Invalid resource string name: \"{0}\".", name), "name");
            return text;
        }

        public static string GetString(CultureInfo culture, string name, params object[] args) {
            var format = GetString(name, culture);
            return string.Format(format, args);
        }

        static ResourceManager strings = new ResourceManager("dirsync.Strings",
                                                             Assembly.GetCallingAssembly());
    }

    class ResourceString
    {
        public ResourceString(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            this.name = name;
        }

        public ResourceString(string name, params object[] args) : this(name) {
            this.args = args;
        }

        public override string ToString() {
            return args != null ? Resources.GetString(name, args) : Resources.GetString(name);
        }

        string name;
        object[] args;
    }
}

