using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Banco_VivesBank.Utils.Pagination;

public class PaginationLinksUtils
{
    public virtual string CreateLinkHeader<T>(PageResponse<T> page, Uri baseUri)
    {
        var linkHeader = new StringBuilder();

        if (page.PageNumber < page.TotalPages - 1)
        {
            string uri = ConstructUri(page.PageNumber + 1, page.PageSize, baseUri);
            linkHeader.Append(BuildLinkHeader(uri, "next"));
        }

        if (page.PageNumber > 0)
        {
            string uri = ConstructUri(page.PageNumber - 1, page.PageSize, baseUri);
            AppendCommaIfNecessary(linkHeader);
            linkHeader.Append(BuildLinkHeader(uri, "prev"));
        }

        if (page.PageNumber > 0)
        {
            string uri = ConstructUri(0, page.PageSize, baseUri);
            AppendCommaIfNecessary(linkHeader);
            linkHeader.Append(BuildLinkHeader(uri, "first"));
        }

        if (page.PageNumber < page.TotalPages - 1)
        {
            string uri = ConstructUri(page.TotalPages - 1, page.PageSize, baseUri);
            AppendCommaIfNecessary(linkHeader);
            linkHeader.Append(BuildLinkHeader(uri, "last"));
        }

        return linkHeader.ToString();
    }

    private string ConstructUri(int pageNumber, int pageSize, Uri baseUri)
    {
        var uriBuilder = new UriBuilder(baseUri);
        var query = QueryHelpers.ParseQuery(uriBuilder.Query);
        query["page"] = pageNumber.ToString();
        query["size"] = pageSize.ToString();
        uriBuilder.Query = QueryHelpers.AddQueryString("", query);

        return uriBuilder.ToString();
    }

    private string BuildLinkHeader(string uri, string rel)
    {
        return $"<{uri}>; rel=\"{rel}\"";
    }

    private void AppendCommaIfNecessary(StringBuilder linkHeader)
    {
        if (linkHeader.Length > 0)
        {
            linkHeader.Append(", ");
        }
    }
}
