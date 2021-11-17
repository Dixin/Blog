namespace Examples.Web.Mvc;

using System.Web;
using System.Web.Mvc;

public static class HtmlHelperExtensions
{
    public static IHtmlString Script(this HtmlHelper htmlHelper, string path)
    {
        if (htmlHelper is null)
        {
            throw new ArgumentNullException(nameof(htmlHelper));
        }

        TagBuilder tagBuilder = new("script");
        tagBuilder.MergeAttribute("type", "text/javascript");
        UrlHelper urlHelper = new(htmlHelper.ViewContext.RequestContext);
        tagBuilder.MergeAttribute("src", urlHelper.Content(path));
        return htmlHelper.Raw(tagBuilder.ToString(TagRenderMode.Normal));
    }
}