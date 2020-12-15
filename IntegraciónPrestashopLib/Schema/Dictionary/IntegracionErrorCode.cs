using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.Dictionary
{
    internal class IntegracionErrorCode
    {
        internal const int UNCONTROLATED = -1;
        internal const int NONE = 0;
        internal const int PRESTASHOP_DISCONNECTED = 1;
        internal const int BAD_INITIALIZED = 2;

        internal const int BAD_ATTRIBUTE = 3;

        public const int PRESTASHOP_INTERNAL_ERROR = 4;
    }
}
