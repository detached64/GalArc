// File: Utility/Exceptions.cs
// Date: 2025/3/11
// Description: Custom exceptions.
//
// Copyright (C) 2025 detached64
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using GalArc.Strings;
using System;

namespace Utility.Exceptions
{
    public class InvalidArchiveException : Exception
    {
        public InvalidArchiveException() : base(LogStrings.ErrorInvalidArchive)
        {
        }

        public InvalidArchiveException(string message) : base(string.Format(LogStrings.ErrorInvalidArchiveCaption, message))
        {
        }
    }

    public class InvalidVersionException : Exception
    {
        public InvalidVersionException() : base(LogStrings.ErrorInvalidVersion)
        {
        }

        public InvalidVersionException(InvalidVersionType type) : base(GetErrorMessage(type))
        {
        }

        public InvalidVersionException(InvalidVersionType type, string version) : base(GetErrorMessage(type, version))
        {
        }

        private static string GetErrorMessage(InvalidVersionType type, string ver = null)
        {
            if (ver == null)
            {
                switch (type)
                {
                    case InvalidVersionType.Unknown:
                        return LogStrings.ErrorInvalidVersionUnknown;
                    case InvalidVersionType.NotSupported:
                        return LogStrings.ErrorInvalidVersionNotSupported;
                    default:
                        return LogStrings.ErrorInvalidVersion;
                }
            }
            else
            {
                switch (type)
                {
                    case InvalidVersionType.Unknown:
                        return string.Format(LogStrings.ErrorInvalidVersionUnknownWithMsg, ver);
                    case InvalidVersionType.NotSupported:
                        return string.Format(LogStrings.ErrorInvalidVersionNotSupportedWithMsg, ver);
                    default:
                        return string.Format(LogStrings.ErrorInvalidVersionWithMsg, ver);
                }
            }
        }
    }

    public class InvalidSchemeException : Exception
    {
        public InvalidSchemeException() : base(LogStrings.ErrorInvalidScheme)
        {
        }

        public InvalidSchemeException(string message) : base(LogStrings.ErrorInvalidScheme + message)
        {
        }
    }

    public enum InvalidVersionType
    {
        Unknown,
        NotSupported
    }
}
