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
using System.IO;

namespace dirsync
{
    static class FileSystem
    {
        public static string[] ListFiles(string directory) {
            try {
                return Directory.GetFiles(directory);
            } catch (Exception exception) {
                Console.WriteLine("Getting files from \"{0}\" failed: {1}", directory,
                    exception.GetChainedMessage(" "));
                return null;
            }
        }

        public static string[] ListDirectories(string directory) {
            try {
                return Directory.GetDirectories(directory);
            } catch (Exception exception) {
                Console.WriteLine("Getting directories from \"{0}\" failed: {1}", directory,
                    exception.GetChainedMessage(" "));
                return null;
            }
        }

        public static bool EnsureDirectoryExistence(string directory) {
            if (Directory.Exists(directory))
                return true;
            try {
                Console.WriteLine("Creating \"{0}\"...", directory);
                Directory.CreateDirectory(directory);
                return true;
            } catch (Exception exception) {
                Console.WriteLine("Creating directory \"{0}\" failed: {1}", directory,
                    exception.GetChainedMessage(" "));
                return false;
            }
        }
    }
}
