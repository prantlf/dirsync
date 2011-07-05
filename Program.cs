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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace dirsync
{
    [Flags]
    enum Operation {
        None, Delete = 1, Update = 2, Create = 4, All = Delete | Update | Create
    }

    class Program
    {
        static int Main(string[] args) {
            PrintLogo();
            Console.WriteLine();
            CommandLine arguments;
            try {
                arguments = new CommandLine(args);
            } catch (Exception exception) {
                ConsoleExtension.WriteWrappedLine(exception.GetChainedMessage());
                Console.WriteLine();
                Console.WriteLine("Run \"{0} -h\" to get usage information.", Name);
                return 1;
            }
            if (arguments.Help) {
                PrintUsage();
                return 2;
            }
            try {
                var watch = new Stopwatch();
                watch.Start();
                var status = Synchronize(arguments.Source, arguments.Target, arguments.Operation,
                    arguments.Exclude);
                watch.Stop();
                Console.WriteLine("Directory content synchronization {0} in {1:g}.",
                    status ? "succeeded" : "failed", watch.Elapsed);
                return status ? 0 : 4;
            } catch (Exception exception) {
                Console.WriteLine();
                ConsoleExtension.WriteWrappedLine(exception.GetChainedMessage());
                return 3;
            }
        }

        static bool Synchronize(string sourcedir, string targetdir, Operation operation,
            IEnumerable<Exclusion> exclude) {
            var status = true;
            if ((operation & Operation.Delete) != 0)
                status &= Execute(() => new Deletor(exclude).Synchronize(sourcedir, targetdir),
                    "Deleting extraneous content");
            if ((operation & Operation.Update) != 0)
                status &= Execute(() => new Updater(exclude).Synchronize(sourcedir, targetdir),
                    "Updating modified content");
            if ((operation & Operation.Create) != 0)
                status &= Execute(() => new Creator(exclude).Synchronize(sourcedir, targetdir),
                    "Creating new content");
            return status;
        }

        static bool Execute(Func<bool> operation, string message) {
            Console.WriteLine("{0}...", message);
            var watch = new Stopwatch();
            watch.Start();
            var status = operation();
            watch.Stop();
            Console.WriteLine("{0} {1} in {2:g}.", message,
                status ? "succeeded" : "failed", watch.Elapsed);
            Console.WriteLine();
            return status;
        }

        static void PrintLogo() {
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(false);
            var title = ((AssemblyTitleAttribute) attributes.First(item =>
                item is AssemblyTitleAttribute)).Title;
            var version = ((AssemblyFileVersionAttribute) attributes.First(item =>
                item is AssemblyFileVersionAttribute)).Version;
            Console.WriteLine("{0} {1}", title, version);
            var copyright = ((AssemblyCopyrightAttribute) attributes.First(item =>
                item is AssemblyCopyrightAttribute)).Copyright;
            Console.WriteLine(copyright);
        }

        static void PrintUsage() {
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(false);
            var description = ((AssemblyDescriptionAttribute) attributes.First(item =>
                item is AssemblyDescriptionAttribute)).Description;
            ConsoleExtension.WriteWrappedLine(description);
            Console.WriteLine();
            Console.WriteLine("Usage: {0} [<options>] <source> <target>", Name);
            Console.WriteLine(" source: Path to the directory to read from.");
            Console.WriteLine(" target: Path to the directory to write to.");
            Console.WriteLine("Options:");
            var length = CommandLine.Options.Max(item => item.Usage.Length);
            var format = string.Format(" -{{0}}, --{{1,-{0}}} - {{2}}", length);
            length += 10;
            foreach (var option in CommandLine.Options.OrderBy(item => item.Name))
                ConsoleExtension.WriteWrappedLine(length, format, option.Abbreviation, option.Usage,
                    option.Description);
            Console.WriteLine("Operations to be performed in the target directory:");
            Console.WriteLine("  all:    All three operations (default).");
            Console.WriteLine("  delete: Delete items missing under the source directory.");
            Console.WriteLine("  update: Update files different from the source directory.");
            Console.WriteLine("  create: Create new files appeared in the source directory.");
            ConsoleExtension.WriteWrappedLine("An exclusion consists of a source file/folder " +
                "name and of an optional directory nested level. 0 is for the first level and " +
                "-1 for all levels (default).");
        }

        static string Name {
            get { return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase); }
        }
    }
}
