using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Helpers;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class ExternalUserService : IUserService
    {
        private readonly HttpClient httpClient;
        public ExternalUserService(HttpClient httpClient, IOptions<ExternalApiOptions> options)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                var users  = new List<User>();
                int pageNumber = 1;
                while(true)
                {
                    var response = await httpClient.GetAsync($"users?page={pageNumber}");
                    if (!response.IsSuccessStatusCode) break;

                    var usersData = await response.Content.ReadFromJsonAsync<APIListResponse<User>>();
                    if (usersData?.Data == null || !usersData.Data.Any()) break;

                    users.AddRange(usersData.Data);

                    if (pageNumber >= usersData.TotalPages) break;
                    pageNumber++;
                }

                return users;
               
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get users", ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                var response = await httpClient.GetAsync($"users/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<APIResponse<User>>();
                    return user?.Data;
                }
                return null;
            }
            catch(Exception ex)
            {
                throw new ApplicationException("Failed to get user by Id", ex);
            }
        }
    }
}
