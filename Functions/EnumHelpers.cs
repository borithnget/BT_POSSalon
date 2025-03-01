using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Functions
{
    public class EnumHelpers
    {
        public enum ErrorType
        {
            Warning,
            Error,
            Message
        }

        public enum UserRole
        {
            Administrator,
            SuperAdmin,
            Message
        }
    }
}