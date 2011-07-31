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
    class Creator : Copier
    {
        public event EventHandler<StartingOperationEventArgs> BeforeCreatingFolder;

        public event EventHandler<FinishedOperationEventArgs> CreatingFolderSucceeded;

        public event EventHandler<FailedOperationEventArgs> CreatingFolderFailed;

        protected override bool ShouldCopy(string sourcepath, string targetpath) {
            return !File.Exists(targetpath);
        }

        protected override bool EnsureTargetFolder(string sourcepath, string targetpath) {
            if (!base.EnsureTargetFolder(sourcepath, targetpath))
                try {
                    FireBeforeCreatingFolder(sourcepath, targetpath);
                    Directory.CreateDirectory(targetpath);
                    FireCreatingFolderSucceeded(sourcepath, targetpath);
                } catch (Exception exception) {
                    FireCreatingFolderFailed(sourcepath, targetpath, exception);
                    return false;
                }
            return true;
        }

        void FireBeforeCreatingFolder(string sourcepath, string targetpath) {
            if (BeforeCreatingFolder != null)
                BeforeCreatingFolder(this, new StartingOperationEventArgs(sourcepath, targetpath));
        }

        void FireCreatingFolderSucceeded(string sourcepath, string targetpath) {
            if (CreatingFolderSucceeded != null)
                CreatingFolderSucceeded(this, new FinishedOperationEventArgs(sourcepath,
                                                                              targetpath));
        }

        void FireCreatingFolderFailed(string sourcepath, string targetpath, Exception exception) {
            if (CreatingFolderFailed != null)
                CreatingFolderFailed(this, new FailedOperationEventArgs(sourcepath, targetpath,
                                                                        exception));
        }
    }
}
