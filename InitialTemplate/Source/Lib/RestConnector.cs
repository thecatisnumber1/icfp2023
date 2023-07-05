using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Lib
{
    public static class RestConnector
    {
        private static string? apiKey;

        public static void SetKey(string key)
        {
            apiKey = key;
        }

        public static string? GetString(string url)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            try
            {
                return client.GetStringAsync(url).Result;
            }
            catch
            {
                return null;
            }
        }

        public static T? GetJson<T>(string url)
        {
            var strung = GetString(url);
            if (strung != null)
            {
                return JsonConvert.DeserializeObject<T>(strung);
            }

            return default(T);
        }

        public static string PostFile(string url, string content)
        {
            using var client = new HttpClient();
            using var formData = new MultipartFormDataContent();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            formData.Add(new StringContent(content), "file", "file");
            var result = client.PostAsync(url, formData).Result;

            return result.Content.ReadAsStringAsync().Result;
        }
    }
}
