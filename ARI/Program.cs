using System.Net.Mime;
using ARI;
using ARI.DTOs;
using ARI.Exceptions;
using ARI.Helpers;
using ARI.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateSlimBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// use AppDbContext.Database.EnsureCreated() on first start
builder.Services.AddDbContext<AppDbContext>(x=>x.UseNpgsql(configuration.AddUserSecrets<Program>().Build().GetConnectionString("Default")));

builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options => { options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default); });
builder.Services.AddTransient<IARIService, ARIService>();
builder.Services.AddTransient<IAriGenerator, HashCodeAriGenerator>();
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
                await context.Response.WriteAsync("Ari is not exists. Create it by Post to /create.");
            }
        });
    });

app.MapPost("/create", async (CreateARIDTO createAriRequest, IARIService ariService, CancellationToken ct) =>
	{
		try
		{
			ARIDTO createdAri = await ariService.CreateAri(createAriRequest, ct);
			return createdAri.Ari;
		}
		catch (AriAlreadyExistsException ex)
		{
			return ex.ARIEntity.Ari;
		}
	});

app.MapGet("/{ariId}", async ([FromRoute] string ariId, HttpContext httpContext, IARIService ariService, CancellationToken ct) =>
	{
		Uri uri = await ariService.GetUriFromAriId(ariId, ct);
		httpContext.Response.Redirect(uri.ToString());
	});

app.Run();