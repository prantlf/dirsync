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
using System.Linq;
using System.Text;

namespace dirsync
{
    class Exclusion
    {
        public string Name { get; set; }
        public int Level { get; set; }

        public bool Applies(string name, int level) {
            return Name == name && (Level < 0 || Level == level);
        }
    }

    class CommandLine : Arguments
    {
        public CommandLine(IEnumerable<string> args) : base(args, Options) {
            if (!Help) {
                Source = GetSource(this);
                Target = GetTarget(this);
                Recursive = !IsSwitchDisabled("recursive");
                Operation = GetOperation(this);
                Exclude = GetExclude(this).ToList();
            }
        }

        public bool Help { get { return HasSwitch("help"); } }

        public string Source { get; private set; }

        public string Target { get; private set; }

        public bool Recursive { get; private set; }

        public Operation Operation { get; private set; }

        public IEnumerable<Exclusion> Exclude { get; private set; }

        public static new IEnumerable<Option> Options {
            get {
                if (options == null)
                    options = new[] {
                        new Option {
                            Name = "help", Abbreviation = "h", Usage = "help",
                            Description = "Prints this usage information."
                        },
                        new Option {
                            Name = "recursive", Abbreviation = "r", Usage = "recursive[+|-]",
                            Description = "Chooses recursive processing of subdirectories " +
                                "(default) or just the specified source directory."
                        },
                        new Option {
                            Name = "operations", Abbreviation = "o", Usage = "operations:<names>",
                            Description = "Comma-delimited list of operations to execute; " +
                                "all is default."
                        },
                        new Option {
                            Name = "exclude", Abbreviation = "e", Usage = "exclude:<names>",
                            Description = "Comma-delimited list of exclusions; folder names " +
                                "with optional nested level to exclude from the operations."
                        },
                    };
                return options;
            }
        }
        static IEnumerable<Option> options;

        static string GetSource(Arguments arguments) {
            if (arguments.Parameters.Count() < 1)
                throw new Exception("Source directory parameter is missing.");
            return arguments.Parameters.ElementAt(0);
        }

        static string GetTarget(Arguments arguments) {
            if (arguments.Parameters.Count() < 2)
                throw new Exception("Target directory parameter is missing.");
            return arguments.Parameters.ElementAt(1);
        }

        static Operation GetOperation(Arguments arguments) {
            Operation result = Operation.All;
            string operations;
            if (arguments.Variables.TryGetValue("operation", out operations)) {
                result = Operation.None;
                foreach (var operation in operations.Split(','))
                    try {
                        result |= (Operation) Enum.Parse(typeof(Operation), operation, true);
                    } catch (Exception exception) {
                        throw new Exception(string.Format("Invalid operation: {0}.", operation),
                            exception);
                    }
            }
            if (result == Operation.None)
                throw new Exception("No operation has been specified.");
            return result;
        }

        static IEnumerable<Exclusion> GetExclude(Arguments arguments) {
            string exclusions;
            if (arguments.Variables.TryGetValue("exclude", out exclusions)) {
                foreach (var exclusion in exclusions.Split(',')) {
                    string name;
                    int level;
                    try {
                        var parts = exclusion.Split(':');
                        name = parts[0];
                        level = parts.Length > 1 ? int.Parse(parts[1]) : -1;
                    } catch (Exception exception) {
                        throw new Exception(string.Format("Invalid exclusion: {0}.", exclusion),
                            exception);
                    }
                    yield return new Exclusion { Name = name, Level = level };
                }
            }
        }
    }
}

