using ARI.DTOs;
using ARI.Entities;
using ARI.Helpers;

namespace ARI.Services;

public interface IAriGenerator
{
	AriEntity Generate(CreateARIDTO ari);
}
public class HashCodeAriGenerator([FromKeyedServices(ServiceKeys.BaseUriKey)] Uri baseUri) : IAriGenerator
{
	public AriEntity Generate(CreateARIDTO ari)
	{
		int hashCode = ari.GetHashCode();
		string ariId = hashCode.ToString();
		var result = new Uri(baseUri, ariId);
		
		return new AriEntity { Ari = result, OriginalUri = ari.UriToAri, AriId = ariId };
	} 
}