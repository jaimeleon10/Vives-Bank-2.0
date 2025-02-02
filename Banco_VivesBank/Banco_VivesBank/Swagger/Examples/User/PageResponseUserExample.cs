using Banco_VivesBank.Cliente.Dto;
using Banco_VivesBank.Cliente.Models;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.Utils.Pagination;
using Swashbuckle.AspNetCore.Filters;

namespace Banco_VivesBank.Swagger.Examples.User;

public sealed class PageResponseUserExample : IExamplesProvider<PageResponse<UserResponse>>
{
    public PageResponse<UserResponse> GetExamples()
    {
        var pageResponseUser= new PageResponse<UserResponse>();
        pageResponseUser.Content = new List<UserResponse>{
            new UserResponse
            {
                Guid = "123456at",
                Username = "JohnDoe",
                Password = "password",
                Role = "User",
                CreatedAt = "2021-10-10",
                UpdatedAt = "2021-10-10",
                IsDeleted = false
            }
        };
        pageResponseUser.TotalPages = 1;
        pageResponseUser.TotalElements = 1;
        pageResponseUser.PageSize = 1;
        pageResponseUser.PageNumber = 1;
        pageResponseUser.Empty = false;
        pageResponseUser.First = true;
        pageResponseUser.Last = true;
        pageResponseUser.SortBy = "Guid";
        pageResponseUser.Direction = "ASC";
            
        return pageResponseUser;
    }

}