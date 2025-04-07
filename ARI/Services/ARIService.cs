using ARI.DTOs;
using ARI.Entities;
using ARI.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ARI.Services;

public interface IARIService
{
	/// <exception cref="AriAlreadyExistsException"></exception>
	Task<ARIDTO> CreateAri(CreateARIDTO ari, CancellationToken ct = default);
	/// <exception cref="AriNotExistsException"></exception>
	Task<Uri> GetUriFromAriId(string ariId, CancellationToken ct = default);
}

public class ARIService(AppDbContext dbContext, IAriGenerator ariGenerator) : IARIService
{
	public async Task<ARIDTO> CreateAri(CreateARIDTO ari, CancellationToken ct)
	{
		if (await dbContext.Aris.SingleOrDefaultAsync(x => x.OriginalUri == ari.UriToAri, ct) is not { } ariEntity)
		{
			ariEntity = ariGenerator.Generate(ari);
			await dbContext.Aris.AddAsync(ariEntity, ct);
			await dbContext.SaveChangesAsync(ct);
		}

		return new ARIDTO(ariEntity.Ari, ariEntity.AriId);
	}
	public async Task<Uri> GetUriFromAriId(string ariId, CancellationToken ct)
	{
		if (await dbContext.Aris.SingleOrDefaultAsync(x => x.AriId == ariId, ct) is not { } ariEntity)
			throw new AriNotExistsException(ariId);

		return ariEntity.OriginalUri;
	}
}

public class ARIServiceLocalCached(IAriGenerator ariGenerator) : IARIService
{
	private readonly IList<AriEntity> _cache = [];
	public Task<ARIDTO> CreateAri(CreateARIDTO ari, CancellationToken ct = default)
	{
		AriEntity entity = ariGenerator.Generate(ari);

		_cache.ValidateExistence(ari, entity);

		_cache.Add(entity);
		return Task.FromResult(new ARIDTO(entity.Ari, entity.AriId));
	}
	public Task<Uri> GetUriFromAriId(string ariId, CancellationToken ct = default)
	{
		if (_cache.SingleOrDefault(x => x.AriId == ariId) is not { } ariEntity)
			throw new AriNotExistsException(ariId);

		return Task.FromResult(ariEntity.OriginalUri);
	}
}

file static class EntitiesExtensions
{
	/// <exception cref="AriAlreadyExistsException"></exception>
	public static void ValidateExistence(this IEnumerable<AriEntity> entities, CreateARIDTO ari, AriEntity entity)
	{
		if (entities.SingleOrDefault(x => x.AriId == entity.AriId || x.OriginalUri == ari.UriToAri) is { } ariEntity)
			throw new AriAlreadyExistsException(ari, ariEntity);
	}
}