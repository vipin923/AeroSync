using AeroSync.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static AeroSync.Components.Pages.IntentAnalyzer;

namespace AeroSync.Services
{
    public class FeedServices
    {
        
        
       
        public async Task<Users> GetFeedUserInfo(string emailHash)
        {
            if (string.IsNullOrWhiteSpace(emailHash))
                throw new ArgumentException("Hash cannot be empty.");

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // 🚀 THE FIX: Escape the special characters (converts '/' to '%2F', etc.)
                    string safeHash = Uri.EscapeDataString(emailHash);

                    // Now inject the safe version into the path
                    var response = await httpClient.GetAsync($"https://localhost:7138/api/Feed/get-feed-user-info/{safeHash}");

                    if (response.IsSuccessStatusCode)
                    {
                        var userInfo = await response.Content.ReadFromJsonAsync<Users>();
                        return userInfo;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Error: {response.StatusCode} - {error}");
                        return new Users();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }
        public async Task<List<FeedModels>> GetPosts()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.GetAsync($"https://localhost:7138/api/Feed/get-posts");
                    if (response.IsSuccessStatusCode)
                    {
                        var posts = await response.Content.ReadFromJsonAsync<List<FeedModels>>();
                        return posts;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(error);
                        return new List<FeedModels>();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($" Error: {ex.Message}");
            }
        }
        
        public async Task<int> SavePost(FeedModels feed)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.PostAsJsonAsync($"https://localhost:7138/api/Feed/save-post", feed);
                    if (response.IsSuccessStatusCode)
                    {
                        var count = await response.Content.ReadFromJsonAsync<int>();
                        return count;
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
        public async Task<int> SaveComments(CommentModel comment)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.PostAsJsonAsync($"https://localhost:7138/api/Feed/save-comments", comment);
                    if (response.IsSuccessStatusCode)
                    {
                        var count = await response.Content.ReadFromJsonAsync<int>();
                        return count;
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
    }
}
