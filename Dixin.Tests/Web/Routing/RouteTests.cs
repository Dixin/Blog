namespace Dixin.Tests.Web.Routing
{
    using System;
    using System.Web;
    using System.Web.Routing;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RouteTests
    {
        [TestMethod]
        public void UnicodeUrl()
        {
            Route route = new Route("{x}/{y}/中文", new RouteValueDictionary(new { x = "a", y = "b" }), new RouteHandler());
            VirtualPathData result = route.GetVirtualPath(new RequestContext(new EmptyHttpContext(), new RouteData(route, new RouteHandler())), new RouteValueDictionary(new { x = "a", y = "文字", bb = "中国" }));
            string path = result.VirtualPath;
            Assert.AreEqual(Uri.EscapeUriString("a/文字/中文?bb=中国"), path, true);
        }
    }

    internal class RouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext) => new HttpHandler();
    }

    internal class HttpHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
        }

        public bool IsReusable { get; }
    }

    internal class EmptyHttpContext : HttpContextBase
    {
    }
}
