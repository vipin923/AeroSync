using AeroSync.Models;
using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AeroSync.Services
{
    public class AuthServices
    {
        public AuthServices()
        {
        }
        public async Task<int> SaveUser(Users user)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {

                    var storefile = new { UserName = user.UserName, PassengerName = user.PassengerName,Email = user.Email,  Password = user.Password };
                  
                    string json = JsonSerializer.Serialize(storefile);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"https://localhost:7138/api/Auth/register", content);
                    //  var response = await httpClient.PostAsync($"https://localhost:7003/api/Ingest/store-file", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadFromJsonAsync<int>();
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(error);
                        return 0;

                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception($" Error: {ex.Message}");

            }
        }
        public async Task<string> Login(string email, string password)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {

                    var storefile = new { Email = email, Password = password };

                    string json = JsonSerializer.Serialize(storefile);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"https://localhost:7138/api/Auth/login", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var token = await response.Content.ReadAsStringAsync();
                        await SecureStorage.SetAsync("Email", email);
                        await SecureStorage.SetAsync("paseto_token", token);

                        return token;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(error);
                        return null;

                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception($" Error: {ex.Message}");

            }
        }
        public async Task<Users> GetUserInfo(string email)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.GetAsync($"https://localhost:7138/api/Auth/get-user-info/{email}");
                    if (response.IsSuccessStatusCode)
                    {
                        var userInfo = await response.Content.ReadFromJsonAsync<Users>();
                        return userInfo;
                        // Do something with userInfo
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(error);
                        return new Users();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($" Error: {ex.Message}");
            }
        }

    }
}
