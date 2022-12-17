using Loginapi.Data;
using Loginapi.DTO;
using Loginapi.Models;
using Loginapi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loginapi.Controllers
{
    [ApiController]
    [Route("api")]

  
    public class ForgotController : Controller
    {
        private ApplcationDbContext db;
        

        public ForgotController(ApplcationDbContext db ) {
            this.db = db;
        }
        [HttpPost("forgot")]
        public IActionResult Forgot(ForgotDto dto)
        {
            ResetToken resetToken = new()
            {
                Email = dto.Email,
                Token = Guid.NewGuid().ToString()
            };

            db.ResetToken.Add(resetToken);
            db.SaveChanges();

            MailService.SendPasswordResetMailAsync(resetToken);

            
            return Ok(new
            {
                message = "Reset Link Emailed"
            });
        }

        [HttpPost("reset")]
        public IActionResult Reset(ResetDto resetDto)
        {
            if (resetDto.Password != resetDto.PasswordConfirm)
            {
                return Unauthorized("Passwords do not match");
            }
            ResetToken? resetToken = db.ResetToken.Where(x => x.Token == resetDto.Token).FirstOrDefault();
            if(resetToken is null)
            {
                return BadRequest("Invalid");
            }
            User? user = db.users.Where(u => u.Email == resetToken.Email).FirstOrDefault();
            if (user is null)
            {
                return BadRequest("User Not Found");
            }

            db.users.Where( u => u.Email == user.Email).FirstOrDefault().Password = HashService.HashPassword(resetDto.Password);
            db.SaveChanges();
            return Ok(new
            {
                message = "Password Changed Successfully!"
            });
        }


    }
}
