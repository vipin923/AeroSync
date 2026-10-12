using AeroSync.Models;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static AeroSync.Components.Pages.IntentAnalyzer;
using static System.Net.WebRequestMethods;

namespace AeroSync.Services
{
    public class IntentServices
    {
        public async Task<string> AnalyzeIntent(string message)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var messagecontent = new { Message = message };
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    httpClient.Timeout = TimeSpan.FromMinutes(10);

                    string json = JsonSerializer.Serialize(messagecontent);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"https://localhost:7138/api/intent/intent-analyze", content);

                    if (response.IsSuccessStatusCode)
                    {
                        // 1. Read the network response as raw text
                        string rawText = await response.Content.ReadAsStringAsync();

                        // 2. Clear any active trailing LLM tokens before parsing the wrapper
                        if (rawText.Contains("<|end|>"))
                        {
                            rawText = rawText.Replace("<|end|>", "").Trim();
                        }

                        // 3. Parse the standard ApiResponse wrapper framework manually
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(rawText, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        string aiOutput = apiResponse?.Data ?? "";

                        // 4. Sanitize the inner AI payload in case it added trailing markers inside the string
                        int startIndex = aiOutput.IndexOf('{');
                        int endIndex = aiOutput.LastIndexOf('}');

                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            return aiOutput.Substring(startIndex, endIndex - startIndex + 1);
                        }

                        return aiOutput;
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
        public async Task<FlightItinerary> ExtractFlightDataFromImage(IBrowserFile file)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    httpClient.Timeout = TimeSpan.FromMinutes(10);

                    using var content = new MultipartFormDataContent();

                    // 🚀 THE CRITICAL BLAZOR STREAM FIX: 
                    // Read the browser stream completely into an isolated frontend RAM memory buffer immediately
                    using var targetMemoryStream = new MemoryStream();
                    using var browserStream = file.OpenReadStream(maxAllowedSize: 10485760); // 10MB limit

                    await browserStream.CopyToAsync(targetMemoryStream);
                    targetMemoryStream.Position = 0; // Rewind the pointer to the absolute beginning

                    // Wrap our isolated memory byte buffer for transmission instead of the live volatile browser stream
                    var fileContent = new ByteArrayContent(targetMemoryStream.ToArray());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                    content.Add(fileContent, "file", file.Name);

                    var response = await httpClient.PostAsync($"https://localhost:7138/api/intent/extract-from-image", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<FlightItinerary>>();
                        return apiResult?.Data ?? new FlightItinerary();
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[AeroSync HTTP API Error]: {error}");
                        return new FlightItinerary();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AeroSync HTTP Crash]: {ex.Message}");
                throw new Exception($"Error: {ex.Message}");
            }
        }
        public async Task<FlightItinerary?> UploadItineraryTextAsync(string pastedText)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    httpClient.Timeout = TimeSpan.FromMinutes(10); // Text is much faster than images!

                    var payload = new { Text = pastedText };

                    // Send the text payload to the new endpoint
                    var response = await httpClient.PostAsJsonAsync("https://localhost:7138/api/intent/extract-from-text", payload);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<FlightItinerary>>(new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return apiResponse?.Data;
                    }
                    else
                    {
                        string errorText = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Server Error: {errorText}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Text analysis failed: {ex.Message}");
            }
        }
        public async Task<int> SaveItinerary(FlightItinerary flightItinerary)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.PostAsJsonAsync($"https://localhost:7138/api/intent/save-itinerary", flightItinerary);

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
        public async Task<FlightItinerary> GetItinerary(string EmailHash,string Date)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var token = await SecureStorage.GetAsync("paseto_token");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.GetAsync($"https://localhost:7138/api/intent/get-itinerary/{EmailHash}/{Date}");
                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadFromJsonAsync<FlightItinerary>();
                       
                        return apiResponse;
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
    }
}
