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
using System.Linq;
using System.Reflection;

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
                WrappingConsole.WriteLine(exception.GetChainedMessage());
                Console.WriteLine();
                Console.WriteLine("Run \"{0} -h\" to get usage information.", ExecutableName);
                return 1;
            }
            if (arguments.Help) {
                PrintUsage();
                return 2;
            }
            var traceSource = new TraceSource("dirsync");
            traceSource.TraceEvent(TraceEventType.Information, 0,
                                   Resources.GetString("SynchronizationStarted"),
                                   arguments.Source, arguments.Target);
            try {
                var watch = new Stopwatch();
                watch.Start();
                var status = Synchronize(arguments.Source, arguments.Target, arguments.Operation,
                                         arguments.Exclude, arguments.Recursive);
                watch.Stop();
                Console.WriteLine("Directory content synchronization {0} in {1}.", status ?
                                  "succeeded" : "failed", Formatter.FormatTime(watch.Elapsed));
                traceSource.TraceEvent(status ? TraceEventType.Information : TraceEventType.Warning,
                    0, Resources.GetString(status ? "SynchronizationSucceeded" :
                        "SynchronizationFailed"), arguments.Source, arguments.Target);
                return status ? 0 : 4;
            } catch (Exception exception) {
                Console.WriteLine();
                WrappingConsole.WriteLine(exception.GetChainedMessage());
                traceSource.TraceEvent(TraceEventType.Error, 0,
                                       Resources.GetString("SynchronizationAborted"),
                                       arguments.Source, arguments.Target, exception);
                return 3;
            }
        }

        static bool Synchronize(string sourcedir, string targetdir, Operation operation,
                                IEnumerable<Exclusion> exclude, bool recursive) {
            if (!Directory.Exists(sourcedir))
                throw new Exception(string.Format("Source directory \"{0}\" does not exist.",
                                                  sourcedir));
            var status = true;
            foreach (var descriptor in OperationDescriptors)
                if ((operation & descriptor.Operation) != 0) {
                    var synchronizer = (Synchronizer) Activator.CreateInstance(
                        descriptor.Synchronizer);
                    synchronizer.Exclude = exclude;
                    synchronizer.Recursive = recursive;
                    var monitor = (SynchronizerMonitor) Activator.CreateInstance(
                        descriptor.Monitor, synchronizer);
                    var informer = (SynchronizerInformer) Activator.CreateInstance(
                        descriptor.Informer, synchronizer, monitor);
                    Activator.CreateInstance(descriptor.Tracer, synchronizer);
                    synchronizer.Synchronize(sourcedir, targetdir);
                    status &= informer.AllSucceeded;
                }
            return status;
        }

        class OperationDescriptor
        {
            public Operation Operation;
            public Type Synchronizer;
            public Type Monitor;
            public Type Informer;
            public Type Tracer;
        }

        static IEnumerable<OperationDescriptor> OperationDescriptors {
            get {
                yield return new OperationDescriptor { Operation = Operation.Delete,
                    Synchronizer = typeof(Deletor), Monitor = typeof(DeletorMonitor),
                    Informer = typeof(DeletorInformer), Tracer = typeof(DeletorTracer)
                };
                yield return new OperationDescriptor { Operation = Operation.Update,
                    Synchronizer = typeof(Updater), Monitor = typeof(UpdaterMonitor),
                    Informer = typeof(UpdaterInformer), Tracer = typeof(UpdaterTracer)
                };
                yield return new OperationDescriptor { Operation = Operation.Create,
                    Synchronizer = typeof(Creator), Monitor = typeof(CreatorMonitor),
                    Informer = typeof(CreatorInformer), Tracer = typeof(CreatorTracer)
                };
            }
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
            WrappingConsole.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine("Usage: {0} [<options>] <source> <target>", ExecutableName);
            Console.WriteLine(" source: Path to the directory to read from.");
            Console.WriteLine(" target: Path to the directory to write to.");
            Console.WriteLine("Options:");
            var length = CommandLine.Options.Max(item => item.Usage.Length);
            var format = string.Format(" -{{0}}, --{{1,-{0}}} - {{2}}", length);
            length += 10;
            foreach (var option in CommandLine.Options.OrderBy(item => item.Name))
                WrappingConsole.WriteLine(length, format, option.Abbreviation,
                                                  option.Usage, option.Description);
            Console.WriteLine("Operations to be performed in the target directory:");
            Console.WriteLine("  all:    All three operations (default).");
            Console.WriteLine("  delete: Delete items missing under the source directory.");
            Console.WriteLine("  update: Update files different from the source directory.");
            Console.WriteLine("  create: Create new files appeared in the source directory.");
            WrappingConsole.WriteLine("An exclusion consists of a source folder name and of " +
                "an optional directory nested level. 0 is for the first level and -1 for all " +
                "levels (default).");
        }

        static string ExecutableName {
            get { return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase); }
        }
    }
}
