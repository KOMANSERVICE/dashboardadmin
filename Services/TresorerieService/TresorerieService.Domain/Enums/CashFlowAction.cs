using System;
using System.Collections.Generic;
using System.Text;

namespace TresorerieService.Domain.Enums;

public enum CashFlowAction
{    
    CREATED=1,
    UPDATED=2,
    SUBMITTED=3,
    APPROVED=4,
    REJECTED=5,
    CANCELLED=6,
    RECONCILED=7
}
