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
using System.Linq;

namespace dirsync
{
    abstract class Synchronizer
    {
        protected readonly IEnumerable<Exclusion> exclude;
        protected int level;

        protected Synchronizer(IEnumerable<Exclusion> exclude) {
            if (exclude == null)
                throw new ArgumentNullException("exclude");
            this.exclude = exclude;
        }

        public abstract bool Synchronize(string sourcedir, string targetdir);

        protected bool CheckSourceDirectory(string sourcedir) {
            if (!Directory.Exists(sourcedir)) {
                Console.WriteLine("Source directory \"{0}\" does not exist.", sourcedir);
                return false;
            }
            Console.WriteLine("Entering \"{0}\" (level {1})...", sourcedir, level);
            return true;
        }

        protected bool ProcessDirectories(string sourcedir, string targetdir) {
            var directories = FileSystem.ListDirectories(sourcedir);
            if (directories == null)
                return false;
            var result = true;
            foreach (var directory in directories) {
                var name = Path.GetFileName(directory);
                if (name != "." && name != "..")
                    result &= ProcessDirectory(sourcedir, targetdir, name);
            }
            return result;
        }

        bool ProcessDirectory(string sourcedir, string targetdir, string name) {
            var sourcepath = Path.Combine(sourcedir, name);
            var targetpath = Path.Combine(targetdir, name);
            if (exclude.Any(item => item.Applies(name, level))) {
                Console.WriteLine("Skipping directory \"{0}\".", sourcepath);
                return true;
            }
            ++level;
            try {
                return Synchronize(sourcepath, targetpath);
            } finally {
                --level;
            }
        }
    }
}
