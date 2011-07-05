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
    abstract class Copier : Synchronizer
    {
        protected Copier(IEnumerable<Exclusion> exclude) : base(exclude) { }

        public override bool Synchronize(string sourcedir, string targetdir) {
            if (sourcedir == null)
                throw new ArgumentNullException("sourcedir");
            if (targetdir == null)
                throw new ArgumentNullException("targetdir");
            if (!CheckSourceDirectory(sourcedir))
                return false;
            if (!FileSystem.EnsureDirectoryExistence(targetdir))
                return false;
            var result = true;
            result &= CopyFiles(sourcedir, targetdir);
            result &= ProcessDirectories(sourcedir, targetdir);
            return result;
        }

        bool CopyFiles(string sourcedir, string targetdir) {
            var files = FileSystem.ListFiles(sourcedir);
            if (files == null)
                return false;
            var result = true;
            foreach (var file in files) {
                var filename = Path.GetFileName(file);
                var sourcepath = Path.Combine(sourcedir, filename);
                var targetpath = Path.Combine(targetdir, filename);
                result &= CopyFile(sourcepath, targetpath);
            }
            return result;
        }

        bool CopyFile(string sourcepath, string targetpath) {

            var result = true;
            if (ShouldCopy(sourcepath, targetpath, ref result))
                result &= PerformCopy(sourcepath, targetpath);
            return result;
        }

        protected abstract bool ShouldCopy(string sourcepath, string targetpath, ref bool result);

        static bool PerformCopy(string sourcepath, string targetpath) {
            try {
                Console.WriteLine("Copying \"{0}\"...", sourcepath);
                File.Copy(sourcepath, targetpath, true);
                return true;
            } catch (Exception exception) {
                Console.WriteLine("Copying \"{0}\" failed: {1}", targetpath,
                    exception.GetChainedMessage(" "));
                return false;
            }
        }
    }
}
