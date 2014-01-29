using System;
using System.Collections.Generic;
using System.Linq;

namespace WiLinq.LinqProvider.Wiql
{
    public enum FieldOperationStatementType
    {
        Equals,
        Contains,
        IsGreater,
        IsGreaterOrEqual,
        IsLess,
        IsLessOrEqual,
        IsIn,
        IsInGroup,
        IsDifferent,
        IsUnder,
        IsNotUnder,
        WasEver
    };
}