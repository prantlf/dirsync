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
using System.IO;

namespace dirsync
{
    class BeforeOperationEventArgs : EventArgs
    {
        public string SourcePath { get; private set; }

        public BeforeOperationEventArgs(string sourcepath) {
            if (sourcepath == null)
                throw new ArgumentNullException("sourcepath");
            SourcePath = sourcepath;
        }
    }

    class StartingOperationEventArgs : BeforeOperationEventArgs
    {
        public string TargetPath { get; private set; }

        public StartingOperationEventArgs(string sourcepath, string targetpath) :
            base(sourcepath) {
            if (targetpath == null)
                throw new ArgumentNullException("targetpath");
            TargetPath = targetpath;
        }
    }

    class FinishedOperationEventArgs : StartingOperationEventArgs
    {
        public FinishedOperationEventArgs(string sourcepath, string targetpath) :
            base(sourcepath, targetpath) {}
    }

    class FailedOperationEventArgs : StartingOperationEventArgs
    {
        public Exception Error { get; private set; }

        public FailedOperationEventArgs(string sourcepath, string targetpath, Exception error) :
            base(sourcepath, targetpath) {
            if (error == null)
                throw new ArgumentNullException("error");
            Error = error;
        }
    }

    class CheckingFileSucceededEventArgs : FinishedOperationEventArgs
    {
        public bool Result { get; set; }

        public CheckingFileSucceededEventArgs(string sourcepath, string targetpath, bool result) :
            base(sourcepath, targetpath) {
            Result = result;
        }
    }

    class BeforePreparationEventArgs : EventArgs
    {
        public FileInfo SourceInfo { get; private set; }
        public FileInfo TargetInfo { get; private set; }

        public BeforePreparationEventArgs(FileInfo sourceinfo, FileInfo targetinfo) {
            if (sourceinfo== null)
                throw new ArgumentNullException("sourceinfo");
            if (targetinfo == null)
                throw new ArgumentNullException("targetinfo");
            SourceInfo = sourceinfo;
            TargetInfo = targetinfo;
        }
    }

    class PreparationStartedEventArgs : BeforePreparationEventArgs
    {
        public string Message { get; private set; }

        public PreparationStartedEventArgs(FileInfo sourceinfo, FileInfo targetinfo,
                                           string message) : base(sourceinfo, targetinfo) {
            if (message == null)
                throw new ArgumentNullException("messagename");
            Message = message;
        }
    }

    class PreparationSucceededEventArgs : PreparationStartedEventArgs
    {
        public PreparationSucceededEventArgs(FileInfo sourceinfo, FileInfo targetinfo,
            string message) : base(sourceinfo, targetinfo, message) {}
    }

    class PreparationFailedEventArgs : PreparationStartedEventArgs
    {
        public Exception Error { get; private set; }

        public PreparationFailedEventArgs(FileInfo sourceinfo, FileInfo targetinfo,
            string message, Exception error) : base(sourceinfo, targetinfo, message) {
            if (error == null)
                throw new ArgumentNullException("error");
            Error = error;
        }
    }
}

