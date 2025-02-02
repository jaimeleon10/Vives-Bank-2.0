using Banco_VivesBank.Database.Entities;
using Banco_VivesBank.User.Controller;
using Banco_VivesBank.User.Dto;
using Banco_VivesBank.User.Exceptions;
using Banco_VivesBank.User.Models;
using Banco_VivesBank.User.Service;
using Banco_VivesBank.Utils.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.User.Controller
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<PaginationLinksUtils> _mockPaginationLinksUtils;
        private UserController _userController;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockPaginationLinksUtils = new Mock<PaginationLinksUtils>();
            _userController = new UserController(_mockUserService.Object, _mockPaginationLinksUtils.Object);
        }

         /*[Test]
         public async Task GetAll()
         {
             var expectedUsers = new List<UserResponse>
             {
                 new UserResponse { Guid = "guid1", Username = "test1" },
                 new UserResponse { Guid = "guid2", Username = "test2" }
             };

             var page = 0;
             var size = 10;
             var sortBy = "id";
             var direction = "desc";

             var pageRequest = new PageRequest
             {
                 PageNumber = page,
                 PageSize = size,
                 SortBy = sortBy,
                 Direction = direction
             };

             var pageResponse = new PageResponse<UserResponse>
             {
                 Content = expectedUsers,
                 TotalElements = expectedUsers.Count,
                 PageNumber = pageRequest.PageNumber,
                 PageSize = pageRequest.PageSize,
                 TotalPages = 1
             };

             _mockUserService.Setup(s => s.GetAllAsync(null, null, pageRequest))
                 .ReturnsAsync(pageResponse);

             var result = await _userController.Getall(null, null, page, size, sortBy, direction);

             Assert.That(result, Is.Not.Null);
             Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());

             var okResult = result.Result as OkObjectResult;
             Assert.That(okResult, Is.Null);

             var pageResponseResult = okResult.Value as PageResponse<UserResponse>;
             Assert.That(pageResponseResult, Is.Not.Null);
             Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
             Assert.That(pageResponseResult.Content, Is.EqualTo(expectedUsers));
             Assert.That(pageResponseResult.TotalElements, Is.EqualTo(expectedUsers.Count));
             Assert.That(pageResponseResult.PageNumber, Is.EqualTo(pageRequest.PageNumber));
             Assert.That(pageResponseResult.PageSize, Is.EqualTo(pageRequest.PageSize));
             Assert.That(pageResponseResult.TotalPages, Is.EqualTo(1));
         }
         */
         

        [Test]
        public async Task GetUserByGuid()
        {
            var expectedUser = new UserResponse
            {
                Guid = "guid",
                Username = "test"
            };

            _mockUserService.Setup(s => s.GetByGuidAsync("guid"))
                .ReturnsAsync(expectedUser);

            var result = await _userController.GetByGuid("guid");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var userResponse = okResult.Value as UserResponse;
            Assert.That(userResponse, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(userResponse.Guid, Is.EqualTo(expectedUser.Guid));
            Assert.That(userResponse.Username, Is.EqualTo(expectedUser.Username));
        }

        [Test]
        public async Task GetUserByUsername()
        {
            var expectedUser = new UserResponse
            {
                Guid = "guid",
                Username = "test"
            };

            _mockUserService.Setup(s => s.GetByUsernameAsync("test"))
                .ReturnsAsync(expectedUser);

            var result = await _userController.GetByUsername("test");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var userResponse = okResult.Value as UserResponse;
            Assert.That(userResponse, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(userResponse.Guid, Is.EqualTo(expectedUser.Guid));
            Assert.That(userResponse.Username, Is.EqualTo(expectedUser.Username));
        }

        [Test]
        public async Task Create()
        {
            var userRequest = new UserRequest
            {
                Username = "test",
                Password = "test"
            };

            var expectedUser = new UserResponse
            {
                Guid = "guid",
                Username = "test"
            };

            _mockUserService.Setup(s => s.CreateAsync(userRequest))
                .ReturnsAsync(expectedUser);

            var result = await _userController.Create(userRequest);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var userResponse = okResult.Value as UserResponse;
            Assert.That(userResponse, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(userResponse.Guid, Is.EqualTo(expectedUser.Guid));
            Assert.That(userResponse.Username, Is.EqualTo(expectedUser.Username));
        }


        [Test]
        public async Task Update()
        {
            var userRequest = new UserRequestUpdate
            {
                Role = Role.User.ToString(),
                IsDeleted = false

            };

            var expectedUser = new UserResponse
            {
                Guid = "guid",
                Username = "test"
            };

            _mockUserService.Setup(s => s.UpdateAsync("guid", userRequest))
                .ReturnsAsync(expectedUser);

            var result = await _userController.Update("guid", userRequest);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var userResponse = okResult.Value as UserResponse;
            Assert.That(userResponse, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(userResponse.Guid, Is.EqualTo(expectedUser.Guid));
        }


        [Test]
        public async Task Delete()
        {
            var expectedUser = new UserResponse
            {
                Guid = "guid",
                Username = "test"
            };

            _mockUserService.Setup(s => s.DeleteByGuidAsync("guid"))
                .ReturnsAsync(expectedUser);

            var result = await _userController.DeleteByGuid("guid");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var userResponse = okResult.Value as UserResponse;
            Assert.That(userResponse, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(userResponse.Guid, Is.EqualTo(expectedUser.Guid));
            Assert.That(userResponse.Username, Is.EqualTo(expectedUser.Username));
        }

    }
}