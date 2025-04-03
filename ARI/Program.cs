using System.Net.Mime;
using System.Text.Json.Serialization;
using ARI.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options => { options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default); });
builder.Services.AddSingleton<IARIService, ARIService>();
builder.Services.AddKeyedTransient<Uri>(ServiceKeys.BaseUriKey, (sp, key) => new Uri("http://localhost:5181/"));

var app = builder.Build();
app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            context.Response.ContentType = MediaTypeNames.Text.Plain;

            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error is AriNotExistsException)
            {
                await context.Response.WriteAsync("Ari is not exists. Create by Post to /create.");
            }
        });
    });

app.MapPost("/create", async (CreateARIDTO createAriRequest, IARIService ariService) =>
	{
		try
		{
			ARIDTO createdAri = await ariService.CreateAri(createAriRequest);
			return createdAri.Ari;
		}
		catch (AriAlreadyExistsException ex)
		{
			return ex.ARIEntity.Ari;
		}
	});

app.MapGet("/{ariId}", async ([FromRoute] string ariId, HttpContext httpContext, IARIService ariService) =>
	{
		Uri uri = await ariService.GetUriFromAriId(ariId);
		httpContext.Response.Redirect(uri.ToString());
	});

app.Run();

public record CreateARIDTO(Uri UriToAri);
public record ARIDTO(Uri Ari, string AriId);
[JsonSerializable(typeof(CreateARIDTO))]
[JsonSerializable(typeof(ARIDTO))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }

public class ServiceKeys
{
	public const string BaseUriKey = "BaseUri";
}