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
    class Updater : Copier
    {
        public event EventHandler<StartingOperationEventArgs> BeforeCheckingFile;

        public event EventHandler<CheckingFileSucceededEventArgs> CheckingFileSucceeded;

        public event EventHandler<FailedOperationEventArgs> CheckingFileFailed;

        public event EventHandler<BeforePreparationEventArgs> BeforePreparation;

        public event EventHandler<PreparationStartedEventArgs> PreparationStarted;

        public event EventHandler<PreparationSucceededEventArgs> PreparationSucceeded;

        public event EventHandler<PreparationFailedEventArgs> PreparationFailed;

        protected override bool ShouldEnter(string sourcepath, string targetpath) {
            return base.ShouldEnter(sourcepath, targetpath) && Directory.Exists(targetpath);
        }

        protected override bool ShouldCopy(string sourcepath, string targetpath) {
            if (File.Exists(targetpath))
                try {
                    FireBeforeCheckingFile(sourcepath, targetpath);
                    var should = ShouldUpdate(sourcepath, targetpath);
                    FireCheckingFileSucceeded(sourcepath, targetpath, should);
                    return should;
                } catch (Exception exception) {
                    FireCheckingFileFailed(sourcepath, targetpath, exception);
                    return true;
                }
            return false;
        }

        bool ShouldUpdate(string sourcepath, string targetpath) {
            var sourceinfo = new FileInfo(sourcepath);
            var targetinfo = new FileInfo(targetpath);
            if (sourceinfo.Length == targetinfo.Length &&
                Math.Abs(sourceinfo.LastWriteTime.ToFileTime() -
                    targetinfo.LastWriteTime.ToFileTime()) <= 20000000)
                return false;

            FireBeforePreparation(sourceinfo, targetinfo);
            if (targetinfo.IsReadOnly)
                try {
                    FirePreparationStarted(sourceinfo, targetinfo, "MakingWritable");
                    targetinfo.IsReadOnly = false;
                    FirePreparationSucceeded(sourceinfo, targetinfo, "MakingWritable");
                } catch (Exception exception) {
                    FirePreparationFailed(sourceinfo, targetinfo, "MakingWritable", exception);
                }
            return true;
        }

        void FireBeforeCheckingFile(string sourcepath, string targetpath) {
            if (BeforeCheckingFile != null)
                BeforeCheckingFile(this, new StartingOperationEventArgs(sourcepath, targetpath));
        }

        void FireCheckingFileSucceeded(string sourcepath, string targetpath, bool result) {
            if (CheckingFileSucceeded != null)
                CheckingFileSucceeded(this, new CheckingFileSucceededEventArgs(sourcepath,
                                                                            targetpath, result));
        }

        void FireCheckingFileFailed(string sourcepath, string targetpath, Exception exception) {
            if (CheckingFileFailed != null)
                CheckingFileFailed(this, new FailedOperationEventArgs(sourcepath, targetpath,
                                                                      exception));
        }

        void FireBeforePreparation(FileInfo sourceinfo, FileInfo targetinfo) {
            if (BeforePreparation != null)
                BeforePreparation(this, new BeforePreparationEventArgs(sourceinfo, targetinfo));
        }

        void FirePreparationStarted(FileInfo sourceinfo, FileInfo targetinfo, string message) {
            if (PreparationStarted != null)
                PreparationStarted(this, new PreparationStartedEventArgs(sourceinfo, targetinfo,
                                                                          message));
        }

        void FirePreparationSucceeded(FileInfo sourceinfo, FileInfo targetinfo, string message) {
            if (PreparationSucceeded != null)
                PreparationSucceeded(this, new PreparationSucceededEventArgs(sourceinfo,
                                                                             targetinfo, message));
        }

        void FirePreparationFailed(FileInfo sourceinfo, FileInfo targetinfo, string message,
                                   Exception exception) {
            if (PreparationFailed != null)
                PreparationFailed(this, new PreparationFailedEventArgs(sourceinfo, targetinfo,
                                                                       message, exception));
        }
    }
}
