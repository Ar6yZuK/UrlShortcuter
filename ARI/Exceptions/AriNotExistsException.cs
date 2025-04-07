namespace ARI.Exceptions;

public class AriNotExistsException(string notExistingAriId) : Exception
{
	public string NotExistingAriId { get; } = notExistingAriId;
}