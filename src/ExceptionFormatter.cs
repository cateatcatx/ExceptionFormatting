using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Decoherence.ExceptionFormatting
{
    /// <summary>
    /// Thread Safe
    /// </summary>
    public class ExceptionFormatter
    {
        private const string SOUR_STACK_PATTERN = ".*? in .*?\\.cs:line \\d*";

        public static ExceptionFormatter Default
        {
            get;
        }

        static ExceptionFormatter()
        {
            Default = new ExceptionFormatter(true, new IMessageFormatter[]
            {
                new WebExceptionMessageFormatter(),
            });
        }

        private readonly bool mRemoveNoSourceInfo;

        private readonly IReadOnlyDictionary<Type, IMessageFormatter> mMessageFormatters;

        public ExceptionFormatter(bool removeNoSrouceInfo = false, IEnumerable<IMessageFormatter> messageFormatters = null)
        {
            mRemoveNoSourceInfo = removeNoSrouceInfo;
            mMessageFormatters = new IndexCollection<Type, IMessageFormatter>(
                messageFormatter => messageFormatter.exceptionType,
                messageFormatters);
        }

        public string Format(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            _FormatImpl(ex, sb);
            sb.Remove(0, 1); // 删除开头的\n
            return sb.ToString();
        }

        private void _FormatImpl(Exception ex, StringBuilder sb)
        {
            if (ex is AggregateException ae && ae.InnerExceptions.Count != 1)
            {
                // AggregateException的数量不为1，是暂时没有考虑的情况
                throw new NotImplementedException();
            }

            if (ex.InnerException != null)
            {
                _FormatImpl(ex.InnerException, sb);
            }

            if (!mRemoveNoSourceInfo)
            {
                _AppendMessage(sb, ex);
                sb.AppendLine(ex.StackTrace);
            }
            else
            {
                int hasSourceFileCount = 0;
                string[] stackInfos = null;
                if (ex.StackTrace != null)
                {
                    stackInfos = ex.StackTrace.Split('\n');
                    for (int i = 0; i < stackInfos.Length; ++i)
                    {
                        var stackInfo = stackInfos[i];
                        stackInfo = stackInfo.Trim();

                        if (_HasSourceInfo(stackInfo))
                        {
                            stackInfos[hasSourceFileCount++] = stackInfo;
                        }
                    }
                }

                _AppendMessage(sb, ex);

                if (hasSourceFileCount > 0)
                {
                    for (int i = 0; i < hasSourceFileCount; ++i)
                    {
                        sb.AppendLine(stackInfos[i]);
                    }
                }
            }
        }

        private bool _HasSourceInfo(string stackInfo)
        {
            return Regex.IsMatch(stackInfo, SOUR_STACK_PATTERN);
        }

        private void _AppendMessage(StringBuilder sb, Exception ex)
        {
            if (!(ex is AggregateException))
            {
                string message = ex.Message;
                if (mMessageFormatters.TryGetValue(ex.GetType(), out var messageFormatter))
                {
                    message = messageFormatter.FormatterMessage(ex);
                }

                message = $"{message.TrimEnd('\n')}";

                // 前面加一个\n，为了把不同Exception分割开
                sb.AppendLine($"\n[{ex.GetType()}] {message}");
            }
        }
    }
}
