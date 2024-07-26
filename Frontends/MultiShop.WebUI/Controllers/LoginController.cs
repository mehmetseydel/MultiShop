﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.IdentityDtos.LoginDtos;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MultiShop.WebUI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IIdentityService _identityService;
        private readonly ILoginService _loginService;
        public LoginController(IHttpClientFactory httpClientFactory, IIdentityService identityService, ILoginService loginService)
        {
            _httpClientFactory = httpClientFactory;
            _identityService = identityService;
            this._loginService = loginService;
        }

        //[HttpGet]
        //public IActionResult SignIn()
        //{
        //    return View();
        //}

       // [HttpPost]
        public async Task<IActionResult> SignIn(SignInDto signInDto)
        {
            signInDto.Username = "mehmet03";
            signInDto.Password = "3333aA*";
            await _identityService.SignIn(signInDto);
            return RedirectToAction("Index", "Test");
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(CreateLoginDto createLoginDto)
        {
            var client = _httpClientFactory.CreateClient();            
            var content = new StringContent(JsonSerializer.Serialize(createLoginDto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5001/api/Logins", content);
            if (response.IsSuccessStatusCode)
            {
                var jsondata=await response.Content.ReadAsStringAsync();
                var tokenModel = JsonSerializer.Deserialize<JwtResponseModel>(jsondata, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                if (tokenModel!=null)
                {
                    JwtSecurityTokenHandler handler= new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(tokenModel.Token);
                    var claims=token.Claims.ToList();

                    if (tokenModel.Token != null)
                    {
                        claims.Add(new Claim("multishoptoken", tokenModel.Token));
                        var claimsIdentity=new ClaimsIdentity(claims,JwtBearerDefaults.AuthenticationScheme);
                        var autProps = new AuthenticationProperties
                        {
                            ExpiresUtc = tokenModel.ExpireDate,
                            IsPersistent = true

                        };
                        await HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), autProps);
                        var id = _loginService.GetUserId;

                        return RedirectToAction("Index", "Default");   
                    }
                }
            }
            return View();
        }
    }
}
