using Amazon.S3;
using FileUploader.Api.Hubs;
using FileUploader.Application.Interfaces;
using FileUploader.Application.Services;
using FileUploader.Infrastructure.AWS;
using FileUploader.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(
	options => options.AddDefaultPolicy(
		policy => policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowCredentials().AllowAnyHeader()
		)
	);
builder.Services.AddSignalR();
builder.Services.Configure<AwsOptions>(builder.Configuration.GetSection("Aws"));
builder.Services.AddSingleton<IS3ClientFactory, S3ClientFactory>();
builder.Services.AddScoped<IS3Uploader, S3Uploader>();
builder.Services.AddTransient<IFileUploadService, FileUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/openapi/v1.json", "v1");
});
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHub<UploadHub>("/uploadHub");

app.Run();
