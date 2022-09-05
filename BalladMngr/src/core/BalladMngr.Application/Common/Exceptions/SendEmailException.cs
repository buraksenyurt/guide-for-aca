using System;

namespace BalladMngr.Application.Common.Exceptions
{
    public class SendEmailException
        : Exception
    {
        public SendEmailException(string message)
            : base(message)
        {
        }
    }
}