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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dirsync
{
    abstract class Synchronizer
    {
        public IEnumerable<Exclusion> Exclude { get; set; }

        public bool Recursive { get; set; }

        public int CurrentLevel { get; private set; }

        public event EventHandler<StartingOperationEventArgs> EnteringDirectory;

        public event EventHandler<BeforeOperationEventArgs> MissingDirectory;

        public event EventHandler<FinishedOperationEventArgs> LeavingDirectory;

        public event EventHandler<FinishedOperationEventArgs> SkippingDirectory;

        public event EventHandler<FailedOperationEventArgs> ListingChildrenFailed;

        public abstract void Synchronize(string sourcedir, string targetdir);

        protected bool CheckSourceDirectory(string sourcedir) {
            if (!Directory.Exists(sourcedir)) {
                FireMissingDirectory(sourcedir);
                return false;
            }
            return true;
        }

        protected void ProcessDirectories(string sourcedir, string targetdir) {
            var directories = ListFolders(sourcedir, targetdir);
            if (directories != null)
                foreach (var directory in directories) {
                    var name = Path.GetFileName(directory);
                    if (name != "." && name != "..")
                        ProcessDirectory(sourcedir, targetdir, name);
                }
        }

        void ProcessDirectory(string sourcedir, string targetdir, string name) {
            var sourcepath = Path.Combine(sourcedir, name);
            var targetpath = Path.Combine(targetdir, name);
            if (Exclude.Any(item => item.Applies(name, CurrentLevel))) {
                FireSkippingDirectory(sourcepath, targetpath);
                return;
            }
            ++CurrentLevel;
            try {
                Synchronize(sourcepath, targetpath);
            } finally {
                --CurrentLevel;
            }
        }

        protected string[] ListFiles(string sourcedir, string targetdir) {
            try {
                return Directory.GetFiles(sourcedir);
            } catch (Exception exception) {
                FireListingChildrenFailed(sourcedir, targetdir, exception);
                return null;
            }
        }

        protected string[] ListFolders(string sourcedir, string targetdir) {
            try {
                return Directory.GetDirectories(sourcedir);
            } catch (Exception exception) {
                FireListingChildrenFailed(sourcedir, targetdir, exception);
                return null;
            }
        }

        protected void FireEnteringDirectory(string sourcepath, string targetpath) {
            if (EnteringDirectory != null)
                EnteringDirectory(this, new StartingOperationEventArgs(sourcepath, targetpath));
        }

        protected void FireMissingDirectory(string sourcepath) {
            if (MissingDirectory != null)
                MissingDirectory(this, new BeforeOperationEventArgs(sourcepath));
        }

        protected void FireLeavingDirectory(string sourcepath, string targetpath) {
            if (LeavingDirectory != null)
                LeavingDirectory(this, new FinishedOperationEventArgs(sourcepath, targetpath));
        }

        protected void FireSkippingDirectory(string sourcepath, string targetpath) {
            if (SkippingDirectory != null)
                SkippingDirectory(this, new FinishedOperationEventArgs(sourcepath, targetpath));
        }

        protected void FireListingChildrenFailed(string sourcepath, string targetpath,
                                                 Exception exception) {
            if (ListingChildrenFailed != null)
                ListingChildrenFailed(this, new FailedOperationEventArgs(sourcepath, targetpath,
                                                                         exception));
        }
    }
}
