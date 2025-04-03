using System.Security.Cryptography;

namespace ARI.Services;

public interface IARIService
{
	/// <exception cref="AriAlreadyExistsException"></exception>
	Task<ARIDTO> CreateAri(CreateARIDTO ari);
	/// <exception cref="AriNotExistsException"></exception>
	Task<Uri> GetUriFromAriId(string ariId);
}

public class ARIService([FromKeyedServices(ServiceKeys.BaseUriKey)] Uri baseUri) : IARIService
{
	private readonly IList<AriEntity> _cache = [];
	public Task<ARIDTO> CreateAri(CreateARIDTO ari)
	{
		int hashCode = ari.GetHashCode();
		string ariId = hashCode.ToString();
		var result = new Uri(baseUri, ariId);
		if (_cache.SingleOrDefault(x => x.AriId == ariId || x.OriginalUri == ari.UriToAri) is { } ariEntity)
			throw new AriAlreadyExistsException(ari, ariEntity);
		while (_cache.SingleOrDefault(x => x.Ari == result) is not null)
			result = new Uri(baseUri, RandomNumberGenerator.GetInt32(int.MaxValue).ToString());

		_cache.Add(new AriEntity { Ari = result, OriginalUri = ari.UriToAri, AriId = ariId });
		return Task.FromResult(new ARIDTO(result, ariId));
	}
	public Task<Uri> GetUriFromAriId(string ariId)
	{
		if (_cache.SingleOrDefault(x => x.AriId == ariId) is not { } ariEntity)
			throw new AriNotExistsException(ariId);

		return Task.FromResult(ariEntity.OriginalUri);
	}
}

public class AriNotExistsException(string notExistingAriId) : Exception
{
	public string NotExistingAriId { get; } = notExistingAriId;
}

public class AriEntity
{
	public required Uri OriginalUri { get; set; }
	public required Uri Ari { get; set; }

	public string AriId { get; set; }
}

public class AriAlreadyExistsException(CreateARIDTO createRequest, AriEntity ariEntity) : Exception
{
	public AriEntity ARIEntity { get; } = ariEntity;
	public CreateARIDTO RequestCreateAri { get; } = createRequest;
}