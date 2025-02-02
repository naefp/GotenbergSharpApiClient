﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// Represents a Gotenberg Api conversion request for HTML or Markdown to pdf
    /// </summary>
    /// <remarks>
    ///     For Markdown conversions your Content.Body must contain HTML that references one or more markdown files
    ///     using the Go template function 'toHTML' within the body element. Chrome uses the function to convert the contents of a given markdown file to HTML.
    ///     See example here: https://gotenberg.dev/docs/modules/chromium#markdown
    /// </remarks>
    public sealed class HtmlRequest : ChromeRequest
    {
        public override string ApiPath
            => this.ContainsMarkdown
                ? Constants.Gotenberg.Chromium.ApiPaths.ConvertMarkdown
                : Constants.Gotenberg.Chromium.ApiPaths.ConvertHtml;

        [PublicAPI]
        public HtmlRequest() : this(false) {}

        [PublicAPI]
        public HtmlRequest(bool containsMarkdown) =>
            this.ContainsMarkdown = containsMarkdown;

        [PublicAPI]
        public bool ContainsMarkdown { get; internal set; }

        [PublicAPI]
        public FullDocument Content { get; set; }

        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (Content?.Body == null) throw new InvalidOperationException("You need to Add at least a body");

            return base.ToHttpContent()
                .Concat(Content.IfNullEmptyContent())
                .Concat(Assets.IfNullEmptyContent());
        }
    }
}