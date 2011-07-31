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
using System.Diagnostics;
using System.IO;

namespace dirsync
{
    class Deletor : Synchronizer
    {
        public event EventHandler<StartingOperationEventArgs> BeforeDeletingFile;

        public event EventHandler<FinishedOperationEventArgs> DeletingFileSucceeded;

        public event EventHandler<FailedOperationEventArgs> DeletingFileFailed;

        public event EventHandler<StartingOperationEventArgs> BeforeDeletingFolder;

        public event EventHandler<FinishedOperationEventArgs> DeletingFolderSucceeded;

        public event EventHandler<FailedOperationEventArgs> DeletingFolderFailed;

        public override void Synchronize(string sourcedir, string targetdir) {
            if (sourcedir == null)
                throw new ArgumentNullException("sourcedir");
            if (targetdir == null)
                throw new ArgumentNullException("targetdir");
            if (!CheckSourceDirectory(sourcedir))
                return;
            if (Directory.Exists(targetdir)) {
                FireEnteringDirectory(sourcedir, targetdir);
                DeleteFiles(sourcedir, targetdir);
                DeleteDirectories(sourcedir, targetdir);
                ProcessDirectories(sourcedir, targetdir);
                FireLeavingDirectory(sourcedir, targetdir);
            }
        }

        void DeleteFiles(string sourcedir, string targetdir) {
            var files = ListFiles(targetdir, targetdir);
            if (files != null)
                foreach (var file in files) {
                    var filename = Path.GetFileName(file);
                    var sourcepath = Path.Combine(sourcedir, filename);
                    var targetpath = Path.Combine(targetdir, filename);
                    DeleteFile(sourcepath, targetpath);
                }
        }

        void DeleteFile(string sourcepath, string targetpath) {
            if (!File.Exists(sourcepath))
                try {
                    FireBeforeDeletingFile(sourcepath, targetpath);
                    File.Delete(targetpath);
                    FireDeletingFileSucceeded(sourcepath, targetpath);
                } catch (Exception exception) {
                    FireDeletingFileFailed(sourcepath, targetpath, exception);
                }
        }

        void DeleteDirectories(string sourcedir, string targetdir) {
            var directories = ListFolders(targetdir, targetdir);
            if (directories != null)
                foreach (var directory in directories) {
                    var filename = Path.GetFileName(directory);
                    if (filename == "." || filename == "..")
                        continue;
                    var sourcepath = Path.Combine(sourcedir, filename);
                    var targetpath = Path.Combine(targetdir, filename);
                    DeleteDitrectory(sourcepath, targetpath);
                }
        }

        void DeleteDitrectory(string sourcepath, string targetpath) {
            if (!Directory.Exists(sourcepath))
                try {
                    FireBeforeDeletingFolder(sourcepath, targetpath);
                    Directory.Delete(targetpath, true);
                    FireDeletingFolderSucceeded(sourcepath, targetpath);
                } catch (Exception exception) {
                    FireDeletingFolderFailed(sourcepath, targetpath, exception);
                }
        }

        void FireBeforeDeletingFile(string sourcepath, string targetpath) {
            if (BeforeDeletingFile != null)
                BeforeDeletingFile(this, new StartingOperationEventArgs(sourcepath, targetpath));
        }

        void FireDeletingFileSucceeded(string sourcepath, string targetpath) {
            if (DeletingFileSucceeded != null)
                DeletingFileSucceeded(this, new FinishedOperationEventArgs(sourcepath,
                                                                            targetpath));
        }

        void FireDeletingFileFailed(string sourcepath, string targetpath, Exception exception) {
            if (DeletingFileFailed != null)
                DeletingFileFailed(this, new FailedOperationEventArgs(sourcepath, targetpath,
                                                                      exception));
        }

        void FireBeforeDeletingFolder(string sourcepath, string targetpath) {
            if (BeforeDeletingFolder != null)
                BeforeDeletingFolder(this, new StartingOperationEventArgs(sourcepath, targetpath));
        }

        void FireDeletingFolderSucceeded(string sourcepath, string targetpath) {
            if (DeletingFolderSucceeded != null)
                DeletingFolderSucceeded(this, new FinishedOperationEventArgs(sourcepath,
                                                                              targetpath));
        }

        void FireDeletingFolderFailed(string sourcepath, string targetpath, Exception exception) {
            if (DeletingFolderFailed != null)
                DeletingFolderFailed(this, new FailedOperationEventArgs(sourcepath, targetpath,
                                                                        exception));
        }
    }
}
