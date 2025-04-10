using Microsoft.EntityFrameworkCore;

namespace ARI.Entities;

[PrimaryKey(nameof(AriId))]
public class AriEntity
{
	public required Uri OriginalUri { get; set; }
	public required Uri Ari { get; set; }

	public string AriId { get; set; }
}