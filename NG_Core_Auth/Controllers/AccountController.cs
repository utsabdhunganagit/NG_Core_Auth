using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NG_Core_Auth.Helpers;
using NG_Core_Auth.Models;

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppSettings _appSettings; 

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _signInManager = signInManager; 
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel formdata)
        {
            //will hold all the error related to registration
            List<string> errorList = new List<string>();
            var user = new IdentityUser
            {
                Email = formdata.Email,
                UserName = formdata.UserName,
                SecurityStamp = Guid.NewGuid().ToString()                              //itLooksForAnyKindOfChangesThatWillAppearIfUsersPasswordIsChanged or etc
            };
            var result = await _userManager.CreateAsync(user, formdata.Password);
            if(result.Succeeded)                                                       //i.e we are able to create User
            {
                await _userManager.AddToRoleAsync(user, "Customer");                   //defaultMaChaiRegisterGardaCustomer Banako
                                                                                       //sending Conformtion Email 
                return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Registration Sucessfull" });
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                    errorList.Add(error.Description);
                }
            }
            return BadRequest(new JsonResult(errorList));

            
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel formdata) 
        {
            //get user from database
            var user =  await _userManager.FindByNameAsync(formdata.UserName);
            var roles =  await _userManager.GetRolesAsync(user);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));
            double tokenExpireTime = Convert.ToDouble(_appSettings.ExpireTime);

            if(user!=null && await _userManager.CheckPasswordAsync(user,formdata.Password))
            {// if true we would need to generate token and send it back to client 
             // to generate token we would need value from appsetting.json file so we can add to token description
             //Conformation Email

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, formdata.UserName),         //it holds user identity
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // is a unique identifer for json token
                        new Claim(ClaimTypes.NameIdentifier, user.Id),                     // for user id   
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString()),
                    }),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Site,
                    Audience = _appSettings.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpireTime)
                }; 
                // generate token here
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(new { token = tokenHandler.WriteToken(token),expiration = token.ValidTo,username= user.UserName,userrole=roles.FirstOrDefault()});
            }
            //return Error
            ModelState.AddModelError("", "UserName or password is not found");
            return Unauthorized(new { LoginError = "Please check login credentials- Ivalid username/password was Entered" }); 


        }
    }
}


