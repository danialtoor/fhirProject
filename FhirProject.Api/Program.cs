using FhirProject.Api.Services;

const string ClientCorsPolicy = "ClientCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IFhirService, FhirService>();
builder.Services.AddSingleton<IHl7V2ParserService, Hl7V2ParserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy =>
    {
        policy.WithOrigins("https://localhost:7054", "http://localhost:5210")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(ClientCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
