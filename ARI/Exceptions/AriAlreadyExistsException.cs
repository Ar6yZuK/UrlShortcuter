using ARI.DTOs;
using ARI.Entities;

namespace ARI.Exceptions;

public class AriAlreadyExistsException(CreateARIDTO createRequest, AriEntity ariEntity) : Exception
{
	public AriEntity ARIEntity { get; } = ariEntity;
	public CreateARIDTO RequestCreateAri { get; } = createRequest;
}