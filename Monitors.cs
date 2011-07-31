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
using System.Diagnostics;
using System.IO;

namespace dirsync
{
    abstract class SynchronizerMonitor
    {
        protected readonly Stopwatch totalWatch = new Stopwatch();
        protected readonly Stopwatch watch = new Stopwatch();

        public SynchronizerMonitor(Synchronizer synchronizer) {
            synchronizer.EnteringDirectory += OnEnteringDirectory;
            synchronizer.LeavingDirectory += OnLeavingDirectory;
        }

        public long SynchronizedDirectoryCount { get; protected set; }

        public TimeSpan TotalTime { get; protected set; }

        public long SucceededFileCount { get; protected set; }

        public long FailedFileCount { get; protected set; }

        public TimeSpan FileOperationTime { get; protected set; }

        public long SucceededFolderCount { get; protected set; }

        public long FailedFolderCount { get; protected set; }

        public TimeSpan FolderOperationTime { get; protected set; }

        void OnEnteringDirectory(object sender, StartingOperationEventArgs args) {
            if (((Synchronizer) sender).CurrentLevel == 0)
                totalWatch.Start();
            ++SynchronizedDirectoryCount;
        }

        void OnLeavingDirectory(object sender, FinishedOperationEventArgs args) {
            if (((Synchronizer) sender).CurrentLevel == 0) {
                totalWatch.Stop();
                TotalTime = totalWatch.Elapsed;
            }
        }

        protected void OnBeforeProcessingFolder(object sender, StartingOperationEventArgs args) {
            watch.Start();
        }

        protected void OnFolderSucceeded(object sender, FinishedOperationEventArgs args) {
            watch.Stop();
            FolderOperationTime += watch.Elapsed;
            ++SucceededFolderCount;
        }

        protected void OnFolderFailed(object sender, FailedOperationEventArgs args) {
            ++FailedFolderCount;
        }
    }

    class DeletorMonitor : SynchronizerMonitor
    {
        public DeletorMonitor(Deletor deletor) : base(deletor) {
            deletor.BeforeDeletingFile += OnBeforeProcessingFile;
            deletor.DeletingFileSucceeded += OnFileSucceeded;
            deletor.DeletingFileFailed += OnFileFailed;
            deletor.BeforeDeletingFolder += OnBeforeProcessingFolder;
            deletor.DeletingFolderSucceeded += OnFolderSucceeded;
            deletor.DeletingFolderFailed += OnFolderFailed;
        }

        void OnBeforeProcessingFile(object sender, StartingOperationEventArgs args) {
            watch.Start();
        }

        void OnFileSucceeded(object sender, FinishedOperationEventArgs args) {
            ++SucceededFileCount;
            FileOperationTime += watch.Elapsed;
        }

        void OnFileFailed(object sender, FailedOperationEventArgs args) {
            ++FailedFileCount;
        }
    }

    abstract class CopierMonitor : SynchronizerMonitor
    {
        public CopierMonitor(Copier copier) : base(copier) {
            copier.BeforeCopyingFile += OnBeforeProcessingFile;
            copier.CopyingFileSucceeded += OnFileSucceeded;
            copier.CopyingFileFailed += OnFileFailed;
        }

        public long FileVolume { get; protected set; }

        void OnBeforeProcessingFile(object sender, StartingOperationEventArgs args) {
            watch.Start();
        }

        void OnFileSucceeded(object sender, FinishedOperationEventArgs args) {
            watch.Stop();
            FileOperationTime += watch.Elapsed;
            ++SucceededFileCount;
            if (args.SourcePath != null)
                FileVolume += new FileInfo(args.SourcePath).Length;
        }

        void OnFileFailed(object sender, FailedOperationEventArgs args) {
            ++FailedFileCount;
        }
    }

    class UpdaterMonitor : CopierMonitor
    {
        public UpdaterMonitor(Updater updater) : base(updater) {}
    }

    class CreatorMonitor : CopierMonitor
    {
        public CreatorMonitor(Creator creator) : base(creator) {
            creator.BeforeCreatingFolder += OnBeforeProcessingFolder;
            creator.CreatingFolderSucceeded += OnFolderSucceeded;
            creator.CreatingFolderFailed += OnFolderFailed;
        }
    }
}
