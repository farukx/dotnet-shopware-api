using System.Net.Http.Headers;
using Newtonsoft.Json;
class Program
{
    static async Task Main(string[] args)
    {
        // Shopware 6 API URL
        string baseUrl = "http://localhost/shop/public/";

        // API kullanıcı adı ve şifresi
        string clientId = "";
        string clientSecret = "";

        // OAuth 2.0 kimlik doğrulama için alınan token
        string token = await AuthenticateWithOAuthAsync(baseUrl, clientId, clientSecret);

        if (!string.IsNullOrEmpty(token))
        {
            // Ürünleri listeleyen istek
            await ListProductsAsync(baseUrl, token);
        }
        else
        {
            Console.WriteLine("OAuth kimlik doğrulama başarısız.");
        }
    }

    static async Task<string> AuthenticateWithOAuthAsync(string baseUrl, string clientId, string clientSecret)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(baseUrl);

            // OAuth 2.0 token alma endpoint'i
            string tokenEndpoint = "api/oauth/token";

            // OAuth 2.0 kimlik doğrulama isteği için gerekli verileri hazırlama
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            // OAuth 2.0 kimlik doğrulama isteği gönderme
            HttpResponseMessage response = await httpClient.PostAsync(tokenEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                // Token alındı
                var tokenResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("OAuth kimlik doğrulama başarılı.");
                return tokenResponse;
            }
            else
            {
                Console.WriteLine("OAuth kimlik doğrulama başarısız. Hata kodu: " + response.StatusCode);
                return null;
            }
        }
    }


    static async Task ListProductsAsync(string baseUrl, string token)
    {

        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(baseUrl);

            var res = JsonConvert.DeserializeObject<dynamic>(token);

            var uriBuilder = new UriBuilder("http://localhost/shop/public/api/product");
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            query["limit"] = "10"; // 10 product
            uriBuilder.Query = query.ToString();
            // İstek başlığı
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", res.access_token.ToString());

            string requestUrl = uriBuilder.ToString();

            // Ürünleri listeleyen istek gönderme
            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Ürünleri listeleyen istek başarılı
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("products:");
                Console.WriteLine(jsonResponse);
            }
            else
            {
                Console.WriteLine("Ürünleri listeleme başarısız. Hata kodu: " + response.StatusCode);
            }
        }
    }
}