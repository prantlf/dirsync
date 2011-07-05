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
    class Deletor : Synchronizer
    {
        public Deletor(IEnumerable<Exclusion> exclude) : base(exclude) {}

        public override bool Synchronize(string sourcedir, string targetdir) {
            if (sourcedir == null)
                throw new ArgumentNullException("sourcedir");
            if (targetdir == null)
                throw new ArgumentNullException("targetdir");
            if (!CheckSourceDirectory(sourcedir))
                return false;
            if (!Directory.Exists(targetdir))
                return true;
            var result = true;
            result &= DeleteExtraFiles(sourcedir, targetdir);
            result &= DeleteExtraDirectories(sourcedir, targetdir);
            result &= ProcessDirectories(sourcedir, targetdir);
            return result;
        }

        static bool DeleteExtraFiles(string sourcedir, string targetdir) {
            var files = FileSystem.ListFiles(targetdir);
            if (files == null)
                return false;
            var result = true;
            foreach (var file in files) {
                var filename = Path.GetFileName(file);
                var sourcepath = Path.Combine(sourcedir, filename);
                var targetpath = Path.Combine(targetdir, filename);
                result &= DeleteExtraFile(sourcepath, targetpath);
            }
            return result;
        }

        static bool DeleteExtraFile(string sourcepath, string targetpath) {
            if (!File.Exists(sourcepath))
                try {
                    Console.WriteLine("Deleting \"{0}\"...", targetpath);
                    File.Delete(targetpath);
                } catch (Exception exception) {
                    Console.WriteLine("Deleting \"{0}\" failed: {1}", targetpath,
                        exception.GetChainedMessage(" "));
                    return false;
                }
            return true;
        }

        static bool DeleteExtraDirectories(string sourcedir, string targetdir) {
            var directories = FileSystem.ListDirectories(targetdir);
            if (directories == null)
                return false;
            var result = true;
            foreach (var directory in directories) {
                var filename = Path.GetFileName(directory);
                if (filename == "." || filename == "..")
                    continue;
                var sourcepath = Path.Combine(sourcedir, filename);
                var targetpath = Path.Combine(targetdir, filename);
                result &= DeleteExtraDitrectory(sourcepath, targetpath);
            }
            return result;
        }

        static bool DeleteExtraDitrectory(string sourcepath, string targetpath) {
            if (!Directory.Exists(sourcepath))
                try {
                    Console.WriteLine("Deleting \"{0}\"...", targetpath);
                    Directory.Delete(targetpath, true);
                } catch (Exception exception) {
                    Console.WriteLine("Deleting \"{0}\" failed: {1}", targetpath,
                        exception.GetChainedMessage(" "));
                    return false;
                }
            return true;
        }
    }
}
