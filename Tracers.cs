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
using System.Diagnostics;

namespace dirsync
{
    abstract class SynchronizerTracer
    {
        protected readonly TraceSource traceSource = new TraceSource("dirsync");

        public SynchronizerTracer(Synchronizer synchronizer) {
            if (synchronizer == null)
                throw new ArgumentNullException("synchronizer");
            synchronizer.MissingDirectory += OnMissingDirectory;
            synchronizer.ListingChildrenFailed += OnListingChildrenFailed;
        }

        void OnMissingDirectory(object sender, BeforeOperationEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Warning, 0, Resources.GetString(
                                     "MissingDirectory"), args.SourcePath);
        }

        void OnListingChildrenFailed(object sender, FailedOperationEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Error, 0, Resources.GetString(
                                     "ListingChildrenFailed"), args.SourcePath, args.Error);
        }
    }

    class DeletorTracer : SynchronizerTracer
    {
        public DeletorTracer(Deletor deletor) : base(deletor) {
            deletor.DeletingFileFailed += OnItemFailed;
            deletor.DeletingFolderFailed += OnItemFailed;
        }

        void OnItemFailed(object sender, FailedOperationEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Error, 0, Resources.GetString(
                                     "DeletingItemFailed"), args.TargetPath, args.Error);
        }
    }

    abstract class CopierTracer : SynchronizerTracer
    {
        public CopierTracer(Copier copier) : base(copier) {
            copier.CopyingFileFailed += OnFileCopyingFailed;
            copier.UpdatingCopiedFileFailed += OnFileUpdatingFailed;
        }

        void OnFileCopyingFailed(object sender, FailedOperationEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Error, 0, Resources.GetString(
                              "CopyingFileFailed"), args.SourcePath, args.TargetPath, args.Error);
        }

        void OnFileUpdatingFailed(object sender, FailedOperationEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Warning, 0, Resources.GetString(
                              "UpdatingFileFailed"), args.SourcePath, args.TargetPath, args.Error);
        }
    }

    class UpdaterTracer : CopierTracer
    {
        public UpdaterTracer(Updater updater) : base(updater) {
            updater.CheckingFileFailed += OnCheckingFileFailed;
            updater.PreparationFailed += OnPreparationFailed;
        }

        void OnCheckingFileFailed(object sender, FailedOperationEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Error, 0, Resources.GetString(
                "CheckingFileFailed"), args.SourcePath, args.TargetPath, args.Error);
        }

        void OnPreparationFailed(object sender, PreparationFailedEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Warning, 0, Resources.GetString(
                "PreparationFailed"), new ResourceString(args.Message, args.SourceInfo.FullName,
                    args.TargetInfo.FullName), args.Error);
        }
    }

    class CreatorTracer : CopierTracer
    {
        public CreatorTracer(Creator creator) : base(creator) {
            creator.CreatingFolderFailed += OnFolderFailed;
        }

        void OnFolderFailed(object sender, FailedOperationEventArgs args) {
            traceSource.TraceEvent(TraceEventType.Error, 0, Resources.GetString(
                                     "CreatingFolderFailed"), args.TargetPath, args.Error);
        }
    }
}
