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
    using System;
    using System.Diagnostics.Contracts;
    using System.Web;
    using System.Web.Mvc;

    public static class HtmlHelperExtensions
    {
        public static IHtmlString Script(this HtmlHelper htmlHelper, string path)
        {
            Contract.Requires<ArgumentNullException>(htmlHelper != null);

            TagBuilder tagBuilder = new TagBuilder("script");
            tagBuilder.MergeAttribute("type", "text/javascript");
            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            tagBuilder.MergeAttribute("src", urlHelper.Content(path));
            return htmlHelper.Raw(tagBuilder.ToString(TagRenderMode.Normal));
        }
    }
}
