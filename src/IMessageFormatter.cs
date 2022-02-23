using System;
using System.Collections.Generic;
using System.Text;

namespace Decoherence.ExceptionFormatting
{
    /// <summary>
    /// 实现者需要保证Thread Safe
    /// </summary>
    public interface IMessageFormatter
    {
        Type exceptionType { get; }

        string FormatterMessage(Exception ex);
    }
}
