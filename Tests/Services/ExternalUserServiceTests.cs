using Core.Models;
using System.Net.Http.Json;
using System.Net;
using Infrastructure.Services;
using Moq;
using Moq.Protected;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Infrastructure.Helpers;

namespace Tests.Services
{
    public class ExternalUserServiceTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenSuccess()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(new
                    {
                        data = new User
                        {
                            Id = 1,
                            Email = "test@abc.com",
                            FirstName = "Priyank",
                            LastName = "Moraiya"
                        }
                    })
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var options = Options.Create(new ExternalApiOptions { BaseUrl = "https://reqres.in/api/" });
            var service = new ExternalUserService(httpClient, options);

            var user = await service.GetUserByIdAsync(1);

            Assert.NotNull(user);
            Assert.Equal(1, user?.Id);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers_AcrossPages()
        {
            var page1 = new APIListResponse<User>
            {
                Page = 1,
                TotalPages = 2,
                Data = new List<User>
            {
                new User { Id = 1, FirstName = "Priyank", LastName = "Moraiya", Email = "test@example.com" },
                new User { Id = 2, FirstName = "Jay", LastName = "Moraiya", Email = "test1@example.com" }
                }
            };

            var page2 = new APIListResponse<User>
            {
                Page = 2,
                TotalPages = 2,
                Data = new List<User>
            {
                new User { Id = 3, FirstName = "John", LastName = "Deo", Email = "john@example.com" }
            }
            };

            var callCount = 0;
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    callCount++;
                    if (request.RequestUri!.Query.Contains("page=1"))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = JsonContent.Create(page1)
                        };
                    }
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(page2)
                    };
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var options = Options.Create(new ExternalApiOptions { BaseUrl = "https://reqres.in/api/" });
            var service = new ExternalUserService(httpClient, options);

            var users = await service.GetAllUsersAsync();

            Assert.Equal(3, users.Count());
            Assert.Contains(users, u => u.Id == 1);
            Assert.Contains(users, u => u.Id == 3);
            Assert.Equal(2, callCount);
        }
    }
}