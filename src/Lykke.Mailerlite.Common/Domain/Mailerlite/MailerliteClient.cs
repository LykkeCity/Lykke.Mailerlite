using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Configuration;
using Lykke.Mailerlite.Common.Domain.Mailerlite.Types;
using Newtonsoft.Json;

namespace Lykke.Mailerlite.Common.Domain.Mailerlite
{   
    public class MailerliteClient : IMailerliteClient
    {
        private readonly MailerliteConfig _mailerliteConfig;
        private readonly HttpClient _httpClient;

        public MailerliteClient(
            HttpMessageHandler messageHandler,
            MailerliteConfig mailerliteConfig)
        {
            _httpClient = new HttpClient(messageHandler);
            _mailerliteConfig = mailerliteConfig;
        }

        public async Task CreateCustomerAsync(string email)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { email }),
                Encoding.UTF8,
                "application/json");
            
            content.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            var message = new HttpRequestMessage
            {
                Content = content, 
                Method = HttpMethod.Post,
                RequestUri = new Uri(_mailerliteConfig.CustomerCreateUrl)
            };

            await _httpClient.SendAsync(message);
        }

        public async Task SetCustomerKycAsync(string email, string kycState)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { fields = new { kycstatus = kycState } }),
                Encoding.UTF8,
                "application/json");
            
            content.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            var message = new HttpRequestMessage
            {
                Content = content, 
                Method = HttpMethod.Put,
                RequestUri = new Uri(string.Format(_mailerliteConfig.CustomerUpdateFieldUrl, email))
            };

            await _httpClient.SendAsync(message);
        }
        
        public async Task SetCustomerRegisteredAsync(string email, DateTime registered)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { fields = new { registered = registered.ToString("yyyy-MM-dd") } }),
                Encoding.UTF8,
                "application/json");
            
            content.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            var message = new HttpRequestMessage
            {
                Content = content, 
                Method = HttpMethod.Put,
                RequestUri = new Uri(string.Format(_mailerliteConfig.CustomerUpdateFieldUrl, email))
            };

            await _httpClient.SendAsync(message);
        }

        public async Task SetCustomerDepositedAsync(string email, bool value)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { fields = new { deposited = value ? "true" : "false" } }),
                Encoding.UTF8,
                "application/json");
            
            content.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            var message = new HttpRequestMessage
            {
                Content = content, 
                Method = HttpMethod.Put,
                RequestUri = new Uri(string.Format(_mailerliteConfig.CustomerUpdateFieldUrl, email))
            };

            await _httpClient.SendAsync(message);
        }

        public async Task SetCustomerSubmittedDocumentsAsync(string email, bool value)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { fields = new { has_ever_submitted_documents = value ? "true" : "false" } }),
                Encoding.UTF8,
                "application/json");
            
            content.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            var message = new HttpRequestMessage
            {
                Content = content, 
                Method = HttpMethod.Put,
                RequestUri = new Uri(string.Format(_mailerliteConfig.CustomerUpdateFieldUrl, email))
            };

            await _httpClient.SendAsync(message);
        }

        public async Task AddCustomerToGroupAsync(string email, string groupName)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { email, group_name = groupName }),
                Encoding.UTF8,
                "application/json");
            
            content.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            var message = new HttpRequestMessage
            {
                Content = content, 
                Method = HttpMethod.Post,
                RequestUri = new Uri(_mailerliteConfig.AddCustomerToGroupUrl)
            };

            await _httpClient.SendAsync(message);
        }

        public async Task<int?> FindGroupIdByNameAsync(string groupName)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { group_name = groupName }),
                Encoding.UTF8,
                "application/json");
            
            content.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            var message = new HttpRequestMessage
            {
                Content = content, 
                Method = HttpMethod.Post,
                RequestUri = new Uri(_mailerliteConfig.FindGroupIdByNameUrl)
            };

            var response = await _httpClient.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var items = JsonConvert.DeserializeObject<IEnumerable<Group>>(responseContent);

                return items.Select(x => x.Id).FirstOrDefault();
            }

            return null;
        }

        public async Task DeleteCustomerFromGroupAsync(string email, int groupId)
        {
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(string.Format(_mailerliteConfig.DeleteCustomerFromGroupUrl, groupId, email))
            };
            message.Headers.Add("X-MailerLite-ApiKey", _mailerliteConfig.ApiKey);
            
            await _httpClient.SendAsync(message);
        }
    }
}
