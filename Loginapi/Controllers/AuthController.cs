using Google.Apis.Auth;
using Google.Authenticator;
using Loginapi.Data;
using Loginapi.DTO;
using Loginapi.Models;
using Loginapi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.InteropServices;

namespace Loginapi.Controllers
{
    [ApiController]
    [Route("api")]

  
    public class AuthController : Controller
    {
        private IConfiguration _configuration;
        private ApplcationDbContext db;
        private readonly string appName = "LoginApi";
        public AuthController(ApplcationDbContext db , IConfiguration configuration) {
            this.db = db;
            this._configuration = configuration;
        }
        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            if(dto.Password != dto.ConfirmPassword)
            {
                return Unauthorized("Passwords do not match");
            }
            User registeredUser = db.users.Where(x => x.Email == dto.Email).FirstOrDefault();

            if (registeredUser is not null)
            {
                return Unauthorized("You Have Aleady Registered");

            }

            User user = new()
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = HashService.HashPassword(dto.Password)
            };
            db.users.Add(user);
            db.SaveChanges();
            return Ok(user);
        }
        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            User? user = db.users.Where(x => x.Email == dto.Email).FirstOrDefault();

            if(user == null )
            {
                return Unauthorized("Invalid Credintials");
            }

            if(HashService.HashPassword(dto.Password) != user.Password)
            {
                return Unauthorized("Invalid Credintials");
            }
            if(user.TfaSecret is not null)
            {
                return Ok(new
                {
                    id = user.Id
                });
            }
            Random random = new Random();
            string secret = new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSUVWXYZ234567" , 32).Select( s => s[random.Next(s.Length)]).ToArray());

            string otpAuthUrl = $"otpauth://totp/{appName}:Secret?secret={secret}&issuer={appName}".Replace(" ","%20");

            return Ok(
                new
                {
                    id = user.Id,
                    secret = secret,
                    passsql = otpAuthUrl
                });
             
            //string accessToken = TokenService.CreateAccessToken(user.Id, _configuration.GetSection("JWT:Accesskey").Value);
            //string refreshToken = TokenService.CreateRefreshToken(user.Id, _configuration.GetSection("JWT:Refreshkey").Value);

            //CookieOptions cookieoptions = new ();
            //cookieoptions.HttpOnly = true;
            //Response.Cookies.Append("refresh_token", refreshToken, cookieoptions);

            //UserToken token = new()
            //{
            //    UserId = user.Id,
            //    Token = refreshToken,
            //    ExpiredAt = DateTime.Now.AddDays(7)
            //};

            //db.UserToken.Add(token);
            //db.SaveChanges();
            //return Ok(new
            //{
            //    token = accessToken
            //});
        }
        [HttpGet("user")]

        public IActionResult User()
        {

            string authorizationHeader = Request.Headers["Authorization"];
            if(authorizationHeader == null || authorizationHeader.Length<=8)
            {
                return Unauthorized("UnAuthenticated");
            }

            string accessToken = authorizationHeader[7..];
            int id = TokenService.DecodeToken(accessToken, out bool hasTokenExpired);

            if(hasTokenExpired)
            {
                return Unauthorized("UnAuthenticated (Token Expired)");

            }

            User? user = db.users.Where(x => x.Id == id).FirstOrDefault();

            if(user is null)
            {
                return Unauthorized("UnAuthenticated");

            }
            return Ok(user);
        }
        [HttpPost("refresh")]
        public IActionResult refresh ()
        {
            if(Request.Cookies["refresh_token"] is null)
            {
                return Unauthorized("UnAuthenticated");

            }

            string? refreshtoken = Request.Cookies["refresh_token"];

            int id = TokenService.DecodeToken(refreshtoken, out bool hasTokenExpired);

            if (!db.UserToken.Where( u=> u.UserId == id && u.Token == refreshtoken && u.ExpiredAt > DateTime.Now).Any())
            {
                return Unauthorized("UnAuthenticated!");
            }

            if (hasTokenExpired)
            {
                return Unauthorized("UnAuthenticated1");

            }

            string accessToken = TokenService.CreateAccessToken(id , _configuration.GetSection("JWT:Accesskey").Value);
            return Ok( new
            {
                token = accessToken
            });
        }
        [HttpPost("logout")]
        public  IActionResult Logout()
        {
            string? refreshToken = Request.Cookies["refresh_token"];

            if(refreshToken is null)
            {
                return Ok("Already Logged out");
            }
            db.UserToken.Remove(db.UserToken.Where(u => u.Token == refreshToken).First());
            db.SaveChanges();
            Response.Cookies.Delete("refresh_token");
            return Ok();
        }

        [HttpPost("two-factor")]
        public IActionResult TwoFactor(TwoFactorDto dto)
        {
            User? user = db.users.Where( u => u.Id == dto.Id).FirstOrDefault();

            if(user is null)
            {
                return Unauthorized("Invalid Credintials");
            }

            string secret = user.TfaSecret is not null ? user.TfaSecret : dto.Secret;

            TwoFactorAuthenticator tfa = new();

            if(!tfa.ValidateTwoFactorPIN(secret, dto.Code , true))
            {
                return Unauthorized("Invalid Credintials");

            }

            if(user.TfaSecret is null)
            {
                db.users.Where(u => u.Email == user.Email).FirstOrDefault()!.TfaSecret = dto.Secret;
                db.SaveChanges();
            }

            string accessToken = TokenService.CreateAccessToken(dto.Id, _configuration.GetSection("JWT:Accesskey").Value);
            string refreshToken = TokenService.CreateRefreshToken(dto.Id, _configuration.GetSection("JWT:Refreshkey").Value);

            CookieOptions cookieoptions = new();
            cookieoptions.HttpOnly = true;
            Response.Cookies.Append("refresh_token", refreshToken, cookieoptions);

            UserToken token = new()
            {
                UserId = dto.Id,
                Token = refreshToken,
                ExpiredAt = DateTime.Now.AddDays(7)
            };

            db.UserToken.Add(token);
            db.SaveChanges();
            return Ok(new
            {
                token = accessToken
            });


        }


        [HttpPost("google-auth")]

        public async Task<IActionResult> GoogleAuth( GoogleAuthDto dto)
        {
            var googleUser = await GoogleJsonWebSignature.ValidateAsync(dto.Token);

            if( googleUser is null)
            {
                return Unauthorized(" Unauthorized! ");
            }

            User? user = db.users.Where( u => u.Email == googleUser.Email).FirstOrDefault();

            if( user is null)
            {

                user = new()
                {
                    FirstName = googleUser.GivenName,
                    LastName = googleUser.FamilyName,
                    Email = googleUser.Email,
                    Password = dto.Token
                };
                db.users.Add(user);
                db.SaveChanges();
            }
            string accessToken = TokenService.CreateAccessToken(user.Id, _configuration.GetSection("JWT:Accesskey").Value);
            string refreshToken = TokenService.CreateRefreshToken(user.Id, _configuration.GetSection("JWT:Refreshkey").Value);

            CookieOptions cookieoptions = new();
            cookieoptions.HttpOnly = true;
            Response.Cookies.Append("refresh_token", refreshToken, cookieoptions);

            UserToken token = new()
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiredAt = DateTime.Now.AddDays(7)
            };

            db.UserToken.Add(token);
            db.SaveChanges();
            return Ok(new
            {
                token = accessToken
            });



        }



        



    }
}
