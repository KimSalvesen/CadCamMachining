﻿using CadCamMachining.Client.Services.Contracts;
using CadCamMachining.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CadCamMachining.Client.States
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private UserInfo _userInfoCache;
        private readonly IAuthorizeApi _authorizeApi;

        public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi)
        {
            this._authorizeApi = authorizeApi;
        }

        public async Task Login(LoginParameters loginParameters)
        {
            await _authorizeApi.Login(loginParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Register(RegisterParameters registerParameters)
        {
            await _authorizeApi.Register(registerParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await _authorizeApi.Logout();
            _userInfoCache = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private async Task<UserInfo> GetUserInfo()
        {
            if (_userInfoCache != null && _userInfoCache.IsAuthenticated) return _userInfoCache;
            _userInfoCache = await _authorizeApi.GetUserInfo();
            return _userInfoCache;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await GetUserInfo();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, userInfo.UserName) }.Concat(userInfo.ExposedClaims.Select(c => new Claim(c.Key, c.Value)));
                    identity = new ClaimsIdentity(claims, "Server authentication");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex.ToString());
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
