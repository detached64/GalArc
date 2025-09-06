using GalArc.I18n;
using System;

namespace GalArc.Models.Utils;

internal class InvalidArchiveException : Exception
{
    public InvalidArchiveException() : base(MsgStrings.ErrorInvalidArchive)
    {
    }

    public InvalidArchiveException(string message) : base(string.Format(MsgStrings.ErrorInvalidArchiveCaption, message))
    {
    }

    public InvalidArchiveException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

internal class InvalidVersionException : Exception
{
    public InvalidVersionException() : base(MsgStrings.ErrorInvalidVersion)
    {
    }

    public InvalidVersionException(InvalidVersionType type) : base(GetErrorMessage(type))
    {
    }

    public InvalidVersionException(InvalidVersionType type, object version) : base(GetErrorMessage(type, version))
    {
    }

    public InvalidVersionException(string message) : base(message)
    {
    }

    public InvalidVersionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private static string GetErrorMessage(InvalidVersionType type, object ver = null)
    {
        return ver == null
            ? type switch
            {
                InvalidVersionType.Unknown => MsgStrings.ErrorInvalidVersionUnknown,
                InvalidVersionType.NotSupported => MsgStrings.ErrorInvalidVersionNotSupported,
                _ => MsgStrings.ErrorInvalidVersion,
            }
            : type switch
            {
                InvalidVersionType.Unknown => string.Format(MsgStrings.ErrorInvalidVersionUnknownWithMsg, ver),
                InvalidVersionType.NotSupported => string.Format(MsgStrings.ErrorInvalidVersionNotSupportedWithMsg, ver),
                _ => string.Format(MsgStrings.ErrorInvalidVersionWithMsg, ver),
            };
    }
}

internal class InvalidSchemeException : Exception
{
    public InvalidSchemeException() : base(MsgStrings.ErrorInvalidScheme)
    {
    }

    public InvalidSchemeException(string message) : base(MsgStrings.ErrorInvalidScheme + message)
    {
    }

    public InvalidSchemeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

internal enum InvalidVersionType
{
    Unknown,
    NotSupported
}
