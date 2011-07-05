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
using System.Collections.Generic;
using System.IO;

namespace dirsync
{
    class Updater : Copier
    {
        public Updater(IEnumerable<Exclusion> exclude) : base(exclude) {}

        protected override bool ShouldCopy(string sourcepath, string targetpath, ref bool result) {
            if (File.Exists(targetpath))
                try {
                    return ShouldUpdate(sourcepath, targetpath);
                } catch (Exception exception) {
                    Console.WriteLine("Checking \"{0}\" failed: {1}", targetpath,
                        exception.GetChainedMessage(" "));
                    return true;
                }
            return false;
        }

        static bool ShouldUpdate(string sourcepath, string targetpath) {
            var sourceinfo = new FileInfo(sourcepath);
            var targetinfo = new FileInfo(targetpath);
            if (sourceinfo.Length == targetinfo.Length &&
                Math.Abs(sourceinfo.LastWriteTime.ToFileTime() -
                    targetinfo.LastWriteTime.ToFileTime()) <= 20000000)
                return false;

            if (targetinfo.IsReadOnly)
                try {
                    Console.WriteLine("Making \"{0}\" writable...", targetpath);
                    targetinfo.IsReadOnly = false;
                } catch (Exception exception) {
                    Console.WriteLine("Making \"{0}\" writable failed: {1}", targetpath,
                        exception.GetChainedMessage(" "));
                }
            return true;
        }
    }
}
