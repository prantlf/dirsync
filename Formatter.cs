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
using System.Text;

namespace dirsync
{
    static class Formatter
    {
        public static object FormatVolume(double volume) {
            if (volume > 1024 * 1024 * 1024)
                return string.Format("{0:0.###} GiB", volume / 1024 / 1024 / 1024);
            if (volume > 1024 * 1024)
                return string.Format("{0:0.###} MiB", volume / 1024 / 1024);
            if (volume > 1024)
                return string.Format("{0:0.###} KiB", volume / 1024);
            return string.Format("{0} B", volume);
        }

        public static object FormatTime(TimeSpan time) {
            var result = new StringBuilder();
            var seconds = time.TotalSeconds;
            if (seconds > 86400) {
                result.AppendFormat("{0} d ", (int) seconds / 86400);
                seconds -= (int) seconds / 86400;
            }
            if (seconds > 3600 || result.Length > 0) {
                result.AppendFormat("{0} h ", (int) seconds / 3600);
                seconds -= (int) seconds / 3600;
            }
            if (seconds > 60 || result.Length > 0) {
                result.AppendFormat("{0} m ", (int) seconds / 60);
                seconds -= (int) seconds / 60;
            }
            return string.Format("{0:0.###} s", seconds);
        }
    }
}

