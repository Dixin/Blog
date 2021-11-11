using Microsoft.AspNetCore;

await WebHost
    .CreateDefaultBuilder(args)
    .Configure(Configure)
    .Build()
    .RunAsync();

static void Configure(WebHostBuilderContext hostContext, IApplicationBuilder app) =>
    app.Run(async httpContext =>
    {
        string requestPath = httpContext.Request.Path.Value?.TrimStart('/').TrimEnd('/') ?? string.Empty;
        string localRelativePath = requestPath.Replace('/', '\\');
        const string LocalRoot = @"D:\Files\Library";
        string localPath = Path.Combine(LocalRoot, localRelativePath);
        if (!Directory.Exists(localPath))
        {
            return;
        }

        string[] requestSegments = localRelativePath.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string parentHtml = requestSegments.Length > 0 ? $@"<li>⬆️<a href=""/{string.Join('/', requestSegments.SkipLast(1))}"">..</a></li>" : string.Empty;
        string directoriesHtml = string.Join(Environment.NewLine,
            new DirectoryInfo(localPath)
                .GetDirectories()
                .OrderBy(item => item.Name)
                .Select(directory => $@"<li>{(directory.Attributes.HasFlag(FileAttributes.Hidden) ? "🗀" : "📁")}<a href=""{(string.IsNullOrWhiteSpace(requestPath) ? "/" : "/" + requestPath + "/")}{directory.Name}"">{directory.Name}</a></li>"));
        string filesHtml = string.Join(Environment.NewLine,
            new DirectoryInfo(localPath)
                .GetFiles()
                .OrderBy(item => item.Name)
                .Select(file => $"<li>🗄{file.Name}</li>"));
        string title = requestSegments.Length > 0 ? requestSegments.Last() : "/";
        string html = @$"
<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"">
        <title>{title}</title>
    </head>
    <body>
        <h2>{localRelativePath}</h2>
        <ul>
            {parentHtml}
            {directoriesHtml}
            {filesHtml}
        </ul>
    </body>
</html>";
        await httpContext.Response.WriteAsync(html);
    });