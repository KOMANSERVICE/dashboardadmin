using System;
using System.Collections.Generic;
using System.Text;

namespace TresorerieService.Domain.Enums;

public enum CashFlowStatus
{
    DRAFT = 1,
    PENDING = 2,
    APPROVED = 3,
    REJECTED = 4,
    CANCELLED = 5
}
