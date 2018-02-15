.method assembly hidebysig static void ExceptionFilter(class [System]System.Net.WebClient webClient) cil managed
{
  .try
  {
    // string html = webClient.DownloadString("http://weblogs.asp.net/dixin");
  }
  filter
  {
    // when ((exception.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.BadRequest)
  }
  {
	// Handle exception.
  }
}
