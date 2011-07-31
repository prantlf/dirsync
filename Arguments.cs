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
    class Option
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }
    }

    class Arguments
    {
        public Arguments(string line) : this(line, null) {}

        public Arguments(string line, IEnumerable<Option> options) : this(line, line.Length, options) {}

        public Arguments(string line, int end) : this(Parse(line, end), null) {}

        public Arguments(string line, int end, IEnumerable<Option> options) :
            this(Parse(line, end), options) {}

        public Arguments(IEnumerable<string> args) : this(args, null) {}

        public Arguments(IEnumerable<string> args, IEnumerable<Option> options) {
            if (args == null)
                throw new ArgumentNullException("args");
            Switches = new Dictionary<string, bool?>(StringComparer.CurrentCultureIgnoreCase);
            Variables = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            Parameters = args;
            Options = options;
            foreach (var argument in Parameters.TakeWhile(item => item.StartsWith("-") &&
                                                          item.Length > 1)) {
                var name = argument.TrimStart('-');
                int colon = name.IndexOf(':');
                if (colon > 0) {
                    var value = name.Substring(colon + 1);
                    name = name.Substring(0, colon - 1);
                    name = GetOptionName(name);
                    Variables[name] = value;
                } else {
                    name = GetOptionName(name);
                    bool? value = null;
                    if (name.EndsWith("+")) {
                        name = argument.Substring(0, name.Length - 1);
                        value = true;
                    } else if (name.EndsWith("-")) {
                        name = name.Substring(0, name.Length - 1);
                        value = false;
                    }
                    Switches[name] = value;
                }
            }
            Parameters = Parameters.Skip(Switches.Count + Variables.Count);
        }

        public IEnumerable<Option> Options { get; private set; }

        public IDictionary<string, bool?> Switches { get; private set; }

        public IDictionary<string, string> Variables { get; private set; }

        public IEnumerable<string> Parameters { get; private set; }

        public bool HasSwitch(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            return Switches.ContainsKey(name);
        }

        public bool IsSwitchEnabled(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            bool? value;
            return Switches.TryGetValue(name, out value) && value != false;
        }

        public bool IsSwitchDisabled(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            bool? value;
            return Switches.TryGetValue(name, out value) && value == false;
        }

        public string GetVariable(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            string value;
            return Variables.TryGetValue(name, out value) ? value : null;
        }

        [Flags]
        enum State { Normal = 0, Escaped = 1, Quoted = 2 }

        protected static IEnumerable<string> Parse(string line, int end) {
            if (line == null)
                throw new ArgumentNullException("line");
            if (end < 0 || end > line.Length)
                throw new ArgumentOutOfRangeException("end");
            var arg = new StringBuilder();
            var state = State.Normal;
            for (var i = 0; i < end; ++i) {
                var ch = line[i];
                if ((state & State.Escaped) != 0) {
                    arg.Append(ch);
                    state &= ~State.Escaped;
                } else if (ch == '\\') {
                    state |= State.Escaped;
                } else if ((state & State.Quoted) != 0) {
                    if (ch == '\"') {
                        yield return arg.ToString();
                        arg.Length = 0;
                    } else {
                        arg.Append(ch);
                    }
                } else {
                    if (ch == '\"') {
                        state |= State.Quoted;
                    } else if (ch == ' ') {
                        if (arg.Length > 0) {
                            yield return arg.ToString();
                            arg.Length = 0;
                        }
                    } else {
                        arg.Append(ch);
                    }
                }
            }
            if (arg.Length > 0)
                yield return arg.ToString();
        }

        string GetOptionName(string abbreviation) {
            if (Options == null)
                return abbreviation;
            var name = Options.Where(item => abbreviation == item.Abbreviation).
                Select(item => item.Name).FirstOrDefault();
            return name ?? abbreviation;
        }
    }
}

