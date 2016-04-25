﻿using System;
using System.IdentityModel.Services;
using System.Threading.Tasks;
using System.Web;
using Dragon.SecurityServer.Common.Models;

namespace Dragon.SecurityServer.AccountSTS.Client
{
    public class AccountSTSClient : IClient
    {
        private readonly string _serviceUrl;
        private readonly string _realm;
        private readonly GenericSTSClient.Client _client;

        private const string ClearCacheAction = "ClearCache";
        private const string UpdateAction = "Update";
        private const string ResetPasswordAction = "ResetPassword";

        public AccountSTSClient(string serviceUrl, string realm)
        {
            _serviceUrl = serviceUrl;
            _realm = realm;
            _client = new GenericSTSClient.Client(serviceUrl);
        }

        public void SetHmacSettings(HmacSettings settings)
        {
            _client.HmacSettings = settings;
        }

        public async Task ClearCache()
        {
            await _client.GetRequest(ClearCacheAction);
        }

        public string GetManagementUrl(string action, string replyUrl)
        {
            var rst = new SignInRequestMessage(new Uri(_serviceUrl), _realm, replyUrl);
            var encodedReplyUrl = HttpUtility.UrlEncode(replyUrl);
            rst.Context = $"rm=0&id=passive&ru={encodedReplyUrl}";
            rst.CurrentTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
            rst.Parameters.Add("action", action);
            var requestUrl = rst.RequestUrl;
            return requestUrl;
        }

        public async Task Update<T>(T model)
        {
            await _client.PostRequest(UpdateAction, model);
        }

        public async Task ResetPassword<T>(T model)
        {
            await _client.PostRequest(ResetPasswordAction, model);
        }
    }
}
