namespace MediaManager.Net;

public record SharedMetadata(string Id, string Url, string Title, string Content, string[] Categories, string[] Tags, string[] Downloads, string[] ImdbIds);
