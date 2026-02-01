namespace MediaManager.Net;

using global::Google.Apis.CustomSearchAPI.v1;
using global::Google.Apis.CustomSearchAPI.v1.Data;
using global::Google.Apis.Services;

internal static class Google
{
    internal static async Task<(string Title, string Link, string Snippet)[]> Search(string query, string apiKey, string customSearchEngineId, string applicationName)
    {
        CustomSearchAPIService service = new(new BaseClientService.Initializer
        {
            ApiKey = apiKey,
            ApplicationName = applicationName
        });

        CseResource.ListRequest listRequest = service.Cse.List();
        listRequest.Cx = customSearchEngineId;
        listRequest.Q = query;
        Search searchResult = await listRequest.ExecuteAsync();
        return searchResult.Items.Select(item => (item.Title, item.Link, item.Snippet)).ToArray();
    }
}