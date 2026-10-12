using AeroSync.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AeroSync.Services
{
    public class LayoverServices
    {
        public async Task<List<MatchScore>> GetLayoverMatchesAsync(PassengerContext me)
        {
            using var httpClient = new HttpClient();
            var token = await SecureStorage.GetAsync("paseto_token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            // Send your current status to the server
            var response = await httpClient.PostAsJsonAsync("https://localhost:7138/api/layover/find-matches", me);

            if (response.IsSuccessStatusCode)
            {
                // 🚀 THE FIX: Tell ApiResponse to expect a List<MatchScore> inside it!
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<MatchScore>>>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Since ApiResponse knows it's a List<MatchScore>, we can just return Data directly!
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }

            return new List<MatchScore>();
        }
        public async Task<PassengerContext> GetUserMatchData(string email)
        {
            using var httpClient = new HttpClient();
            var token = await SecureStorage.GetAsync("paseto_token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            var response = await httpClient.GetAsync($"https://localhost:7138/api/layover/get-user-match-data/{email}");
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<PassengerContext>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
               
                    return apiResponse;
                
            }
            return null;
        }
    }
}
