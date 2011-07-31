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

namespace dirsync
{
    abstract class Copier : Synchronizer
    {
        public event EventHandler<StartingOperationEventArgs> BeforeCopyingFile;

        public event EventHandler<FinishedOperationEventArgs> CopyingFileSucceeded;

        public event EventHandler<FailedOperationEventArgs> CopyingFileFailed;

        public event EventHandler<FailedOperationEventArgs> UpdatingCopiedFileFailed;

        public override void Synchronize(string sourcedir, string targetdir) {
            if (sourcedir == null)
                throw new ArgumentNullException("sourcedir");
            if (targetdir == null)
                throw new ArgumentNullException("targetdir");
            if (!ShouldEnter(sourcedir, targetdir))
                return;
            FireEnteringDirectory(sourcedir, targetdir);
            if (!EnsureTargetFolder(sourcedir, targetdir)) {
                FireLeavingDirectory(sourcedir, targetdir);
                return;
            }
            CopyFiles(sourcedir, targetdir);
            ProcessDirectories(sourcedir, targetdir);
            FireLeavingDirectory(sourcedir, targetdir);
        }

        protected virtual bool ShouldEnter(string sourcepath, string targetpath) {
            return CheckSourceDirectory(sourcepath);
        }

        protected virtual bool EnsureTargetFolder(string sourcepath, string targetpath) {
            return Directory.Exists(targetpath);
        }

        void CopyFiles(string sourcedir, string targetdir) {
            var files = ListFiles(sourcedir, targetdir);
            if (files != null)
                foreach (var file in files) {
                    var filename = Path.GetFileName(file);
                    var sourcepath = Path.Combine(sourcedir, filename);
                    var targetpath = Path.Combine(targetdir, filename);
                    if (ShouldCopy(sourcepath, targetpath))
                        CopyFile(sourcepath, targetpath);
                }
        }

        protected abstract bool ShouldCopy(string sourcepath, string targetpath);

        void CopyFileAttributes(string sourcepath, string targetpath)
        {
            try {
                var sourceinfo = new FileInfo(sourcepath);
                var targetinfo = new FileInfo(targetpath);
                targetinfo.Attributes = sourceinfo.Attributes;
                targetinfo.CreationTime = sourceinfo.CreationTime;
                targetinfo.LastWriteTime = sourceinfo.LastWriteTime;
            } catch (Exception exception) {
                FireUpdatingCopiedFileFailed(sourcepath, targetpath, exception);
            }
        }

        void CopyFile(string sourcepath, string targetpath) {
            try {
                FireBeforeCopyingFile(sourcepath, targetpath);
                File.Copy(sourcepath, targetpath, true);
                CopyFileAttributes(sourcepath, targetpath);
                FireCopyingFileSucceeded(sourcepath, targetpath);
            } catch (Exception exception) {
                FireCopyingFileFailed(sourcepath, targetpath, exception);
            }
        }

        void FireBeforeCopyingFile(string sourcepath, string targetpath) {
            if (BeforeCopyingFile != null)
                BeforeCopyingFile(this, new StartingOperationEventArgs(sourcepath, targetpath));
        }

        void FireCopyingFileSucceeded(string sourcepath, string targetpath) {
            if (CopyingFileSucceeded != null)
                CopyingFileSucceeded(this, new FinishedOperationEventArgs(sourcepath,
                                                                           targetpath));
        }

        void FireCopyingFileFailed(string sourcepath, string targetpath, Exception exception) {
            if (CopyingFileFailed != null)
                CopyingFileFailed(this, new FailedOperationEventArgs(sourcepath, targetpath,
                                                                     exception));
        }

        void FireUpdatingCopiedFileFailed(string sourcepath, string targetpath,
                                          Exception exception) {
            if (UpdatingCopiedFileFailed != null)
                UpdatingCopiedFileFailed(this, new FailedOperationEventArgs(sourcepath, targetpath,
                                                                            exception));
        }
    }
}
