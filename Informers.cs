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
using System.Globalization;

namespace dirsync
{
    abstract class SynchronizerInformer
    {
        protected readonly SynchronizerMonitor monitor;

        public bool AllSucceeded { get; protected set; }

        public SynchronizerInformer(Synchronizer synchronizer, SynchronizerMonitor monitor) {
            if (synchronizer == null)
                throw new ArgumentNullException("synchronizer");
            if (monitor == null)
                throw new ArgumentNullException("monitor");
            this.monitor = monitor;
            AllSucceeded = true;
            synchronizer.EnteringDirectory += OnEnteringDirectory;
            synchronizer.LeavingDirectory += OnLeavingDirectory;
            synchronizer.MissingDirectory += OnMissingDirectory;
            synchronizer.SkippingDirectory += OnSkippingDirectory;
            synchronizer.ListingChildrenFailed += OnListingChildrenFailed;
        }

        protected abstract string Message { get; }

        void OnEnteringDirectory(object sender, StartingOperationEventArgs args) {
            if (((Synchronizer) sender).CurrentLevel == 0)
                Console.WriteLine("{0}...", Message);
            Console.WriteLine("Entering \"{0}\" (level {1})...", args.SourcePath,
                              ((Synchronizer) sender).CurrentLevel);
        }

        void OnLeavingDirectory(object sender, FinishedOperationEventArgs args) {
            var synchronizer = (Synchronizer) sender;
            if (synchronizer.CurrentLevel == 0) {
                Console.WriteLine("{0} {1} in {2}.", Message, AllSucceeded ? "succeeded" :
                                  "failed", Formatter.FormatTime(monitor.TotalTime));
                PrintStatistics();
                Console.WriteLine();
            }
        }

        protected virtual void PrintStatistics() {
            if (monitor.SucceededFolderCount > 0 || monitor.FailedFolderCount > 0) {
                Console.WriteLine("Succeeded folder count:  {0}.", monitor.SucceededFolderCount);
                Console.WriteLine("Failed folder count:     {0}.", monitor.FailedFolderCount);
                Console.WriteLine("Time spent on folders:   {0}.",
                                  Formatter.FormatTime(monitor.FolderOperationTime));
                if (monitor.FolderOperationTime.TotalSeconds > 0)
                    Console.WriteLine("Operation speed:         {0:0.###} folders/s.",
                        monitor.SucceededFolderCount / monitor.FolderOperationTime.TotalSeconds);
            }
            if (monitor.SucceededFileCount > 0 || monitor.FailedFileCount > 0) {
                Console.WriteLine("Succeeded file count:    {0}.", monitor.SucceededFileCount);
                Console.WriteLine("Failed file count:       {0}.", monitor.FailedFileCount);
                Console.WriteLine("Time spent on files:     {0}.",
                                  Formatter.FormatTime(monitor.FileOperationTime));
                if (monitor.FileOperationTime.TotalSeconds > 0)
                    Console.WriteLine("Operation speed:         {0:0.###} files/s.",
                        monitor.SucceededFileCount / monitor.FileOperationTime.TotalSeconds);
            }
        }

        void OnMissingDirectory(object sender, BeforeOperationEventArgs args) {
            Console.WriteLine("Source directory \"{0}\" does not exist.", args.SourcePath);
            AllSucceeded = false;
        }

        void OnSkippingDirectory(object sender, FinishedOperationEventArgs args) {
            Console.WriteLine("Skipping directory \"{0}\".", args.SourcePath);
        }

        void OnListingChildrenFailed(object sender, FailedOperationEventArgs args) {
            Console.WriteLine("Getting children of \"{0}\" failed: {1}", args.SourcePath,
                              args.Error.GetChainedMessage(" "));
            AllSucceeded = false;
        }
    }

    class DeletorInformer : SynchronizerInformer
    {
        public DeletorInformer(Deletor deletor, DeletorMonitor monitor) : base(deletor, monitor) {
            deletor.BeforeDeletingFile += OnBeforeProcessingItem;
            deletor.DeletingFileFailed += OnItemFailed;
            deletor.BeforeDeletingFolder += OnBeforeProcessingItem;
            deletor.DeletingFolderFailed += OnItemFailed;
        }

        void OnBeforeProcessingItem(object sender, StartingOperationEventArgs args) {
            Console.WriteLine("Deleting \"{0}\"...", args.TargetPath);
        }

        void OnItemFailed(object sender, FailedOperationEventArgs args) {
            Console.WriteLine("Deleting \"{0}\" failed: {1}", args.TargetPath,
                              args.Error.GetChainedMessage(" "));
            AllSucceeded = false;
        }

        protected override string Message {
            get { return "Deleting extraneous content"; }
        }
    }

    abstract class CopierInformer : SynchronizerInformer
    {
        public CopierInformer(Copier copier, CopierMonitor monitor) : base(copier, monitor) {
            copier.BeforeCopyingFile += OnBeforeProcessingFile;
            copier.CopyingFileFailed += OnFileCopyingFailed;
            copier.UpdatingCopiedFileFailed += OnFileUpdatingFailed;
        }

        protected override void PrintStatistics() {
            base.PrintStatistics();
            if (monitor.SucceededFileCount > 0) {
                var volume = ((CopierMonitor) monitor).FileVolume;
                Console.WriteLine("Copied volume:           {0}.",
                                  Formatter.FormatVolume(volume));
                if (monitor.FileOperationTime.TotalSeconds > 0)
                    Console.WriteLine("Transfer speed:          {0:0.###}/s.",
                        Formatter.FormatVolume(volume / monitor.FileOperationTime.TotalSeconds));
            }
        }

        void OnBeforeProcessingFile(object sender, StartingOperationEventArgs args) {
            Console.WriteLine("Copying \"{0}\"...", args.SourcePath);
        }

        void OnFileCopyingFailed(object sender, FailedOperationEventArgs args) {
            Console.WriteLine("Copying \"{0}\" failed: {1}", args.TargetPath,
                              args.Error.GetChainedMessage(" "));
            AllSucceeded = false;
        }

        void OnFileUpdatingFailed(object sender, FailedOperationEventArgs args) {
            Console.WriteLine("Updating \"{0}\" failed: {1}", args.TargetPath,
                              args.Error.GetChainedMessage(" "));
            AllSucceeded = false;
        }
    }

    class UpdaterInformer : CopierInformer
    {
        public UpdaterInformer(Updater updater, UpdaterMonitor monitor) : base(updater, monitor) {
            updater.CheckingFileFailed += OnCheckingFileFailed;
            updater.PreparationStarted += OnPreparationStarted;
            updater.PreparationFailed += OnPreparationFailed;
        }

        void OnCheckingFileFailed(object sender, FailedOperationEventArgs args) {
            Console.WriteLine("Checking \"{0}\" failed: {1}", args.TargetPath,
                args.Error.GetChainedMessage(" "));
            AllSucceeded = false;
        }

        void OnPreparationStarted(object sender, PreparationStartedEventArgs args) {
            Console.WriteLine(Resources.GetString(CultureInfo.InvariantCulture, args.Message) +
                              "...", args.SourceInfo.FullName, args.TargetInfo.FullName);
        }

        void OnPreparationFailed(object sender, PreparationFailedEventArgs args) {
            Console.WriteLine(Resources.GetString(CultureInfo.InvariantCulture, args.Message) +
                              " failed: {2}", args.SourceInfo.FullName, args.TargetInfo.FullName,
                              args.Error.GetChainedMessage(" "));
        }

        protected override string Message {
            get { return "Updating modified content"; }
        }
    }

    class CreatorInformer : CopierInformer
    {
        public CreatorInformer(Creator creator, CreatorMonitor monitor) : base(creator, monitor) {
            creator.BeforeCreatingFolder += OnBeforeProcessingFolder;
            creator.CreatingFolderFailed += OnFolderFailed;
        }

        void OnBeforeProcessingFolder(object sender, StartingOperationEventArgs args) {
            Console.WriteLine("Creating \"{0}\"...", args.TargetPath);
        }

        void OnFolderFailed(object sender, FailedOperationEventArgs args) {
            Console.WriteLine("Creating \"{0}\" failed: {1}", args.TargetPath,
                              args.Error.GetChainedMessage(" "));
            AllSucceeded = false;
        }

        protected override string Message {
            get { return "Creating new content"; }
        }
    }
}
