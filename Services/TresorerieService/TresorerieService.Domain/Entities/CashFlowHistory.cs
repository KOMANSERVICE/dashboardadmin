
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TresorerieService.Domain.Abstractions;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Domain.Entities;

[Table("TC00002")]
public class CashFlowHistory: Entity<Guid>
{

    [Column("fld1")]
    public string CashFlowId { get; set; }

    [Column("fld2")]
    public CashFlowAction Action { get; set; }

    [Column("fld3")]
    public string? OldStatus { get; set; }

    [Column("fld4")]
    public string? NewStatus { get; set; }

    [Column("fld25")]
    public string? Comment { get; set; }

    public virtual CashFlow CashFlow { get; set; }
}
