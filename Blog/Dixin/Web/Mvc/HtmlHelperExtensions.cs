// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlHelperExtensions.cs" company="Bellevues.com">
//   Copyright (c) Bellevues.com. All rights reserved.
// </copyright>
// <summary>
//   Defines the HtmlHelperExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dixin.Web.Mvc
{
    using System.Web;
    using System.Web.Mvc;

    using Dixin.Common;

    public static class HtmlHelperExtensions
    {
        public static IHtmlString Script(this HtmlHelper htmlHelper, string path)
        {
            htmlHelper.NotNull(nameof(htmlHelper));

            TagBuilder tagBuilder = new TagBuilder("script");
            tagBuilder.MergeAttribute("type", "text/javascript");
            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            tagBuilder.MergeAttribute("src", urlHelper.Content(path));
            return htmlHelper.Raw(tagBuilder.ToString(TagRenderMode.Normal));
        }
    }
}
