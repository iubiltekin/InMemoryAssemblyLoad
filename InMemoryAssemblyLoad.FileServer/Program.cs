
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => { });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseFileServer();

string GetOrCreateFilePath(string fileName, string filesDirectory = "PrivateFiles")
{
    var directoryPath = Path.Combine(app.Environment.ContentRootPath, filesDirectory);

    if (!Directory.Exists(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    return Path.Combine(app.Environment.ContentRootPath, directoryPath, fileName);
}

app.MapGet("/files/{fileName}", IResult (string fileName) =>
{
    var filePath = GetOrCreateFilePath(fileName);

    if (File.Exists(filePath))
    {
        return TypedResults.PhysicalFile(filePath, fileDownloadName: $"{fileName}");
    }

    return TypedResults.NotFound("No file found with the supplied file name");
}).WithName("GetFileByName");


app.Run();
