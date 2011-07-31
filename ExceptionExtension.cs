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

namespace dirsync
{
    static class ExceptionExtension
    {
        public static IEnumerable<Exception> GetExceptionChain(this Exception exception) {
            while (exception != null) {
                yield return exception;
                exception = exception.InnerException;
            }
        }

        public static string GetChainedMessage(this Exception exception) {
            return exception.GetChainedMessage(Environment.NewLine);
        }

        public static string GetChainedMessage(this Exception exception, string separator) {
            return string.Join(separator, exception.GetExceptionChain().
                Select(item => item.Message).ToArray());
        }
    }
}
