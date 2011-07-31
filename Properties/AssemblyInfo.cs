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

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Directory Content Synchronization Tool")]
[assembly: AssemblyDescription(@"Makes the content of a target directory the same as of the " +
    "source one. Deletes extraneous, updates modified and creates new files from, in and to " +
    "the target directory in the optimal order. Works recursively in subdirectories.")]
[assembly: AssemblyConfiguration("")]

[assembly: ComVisible(false)]
[assembly: Guid("a5ce7993-7aa0-4f08-a3c3-197f8f2818d5")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AssemblyCompany("Ferdinand Prantl <prantlf@gmail.com>")]
[assembly: AssemblyProduct("dirsync")]
[assembly: AssemblyCopyright("Copyright © 2009-2011 Ferdinand Prantl <prantlf@gmail.com>")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("1.3.0.0")]
[assembly: AssemblyFileVersion("1.3.0.0")]
