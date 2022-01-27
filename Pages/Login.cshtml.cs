using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using BAIS3110Authentication.Secured;

namespace BAIS3110Authentication.Pages
{
    public class LoginModel : PageModel
    {      
        public string Message { get; set; }

        [BindProperty, Required]
        public string UserName { get; set; }
        [BindProperty, Required]
        public string UserEmail { get; set; }
        [BindProperty, Required]
        public string UserRole { get; set; }
        [BindProperty, DataType(DataType.Password), Required]
        public string UserPassword { get; set; }

        private string HashedPassword;



        public async Task<IActionResult> OnPost()
        {
            //connecting to db
            string ConnectionString = Startup.ConnectionString;
            SqlConnection BAIS3150Connection = new SqlConnection(ConnectionString);
            BAIS3150Connection.Open();

            //get user
            SqlCommand VerifyUser = new SqlCommand
            {
                Connection = BAIS3150Connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "P_LoginUser"
            };
            SqlParameter ProgramParameter;
            ProgramParameter = new SqlParameter
            {
                ParameterName = "@UserEmail",
                SqlDbType = SqlDbType.VarChar,
                Direction = ParameterDirection.Input,
                SqlValue = UserEmail
            };
            VerifyUser.Parameters.Add(ProgramParameter);
            try
            {
                SqlDataReader ProgramDataReader = VerifyUser.ExecuteReader();

                if (ProgramDataReader.HasRows)
                {
                    while (ProgramDataReader.Read())
                    {
                        UserName = ProgramDataReader["UserName"].ToString();
                        UserEmail = ProgramDataReader["UserEmail"].ToString();
                        UserRole = ProgramDataReader["UserRole"].ToString();
                        HashedPassword = ProgramDataReader["HashedPassword"].ToString();
                    }
                }
            }
            catch
            {
                BAIS3150Connection.Close();
                Message = "Failed try again";
            }

            //checking if correct password
            Security PasswordChecker = new Security();
            bool correctPass = PasswordChecker.CheckMatch(HashedPassword, UserPassword);
            if (correctPass is true)
            {
                var claims = new List<Claim>
                        {
                        new Claim(ClaimTypes.Email, UserEmail),
                        new Claim(ClaimTypes.Name, UserName)
                        };
                var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, UserRole));
                AuthenticationProperties authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = <bool>,
                    // Refreshing the authentication session should be allowed.
                    //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    // The time at which the authentication ticket expires. A
                    // value set here overrides the ExpireTimeSpan option of
                    // CookieAuthenticationOptions set with AddCookie.
                    IsPersistent = true,
                    // Whether the authentication session is persisted across
                    // multiple requests. When used with cookies, controls
                    // whether the cookie's lifetime is absolute (matching the
                    // lifetime of the authentication ticket) or session-based.
                    //IssuedUtc = <DateTimeOffset>,
                    // The time at which the authentication ticket was issued.
                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http
                    // redirect response value.
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
               
                return RedirectToPage("/Admin/Index");
            }
            Message = "Invalid attempt";
            return Page();


            //    //Hardcoded User Credentials
            //    string Email = "czieger@nait.ca";
            //    string Name = "Christina";
            //    string Password = "123";
            //    string Role = "Admin";
            //    if (Email == UserEmail)
            //    {
            //        if (Password == UserPassword)
            //        {
            //            var claims = new List<Claim>
            //                {
            //                new Claim(ClaimTypes.Email, Email),
            //                new Claim(ClaimTypes.Name, Name)
            //                };
            //            var claimsIdentity = new ClaimsIdentity(claims,
            //            CookieAuthenticationDefaults.AuthenticationScheme);
            //            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, Role));
            //            AuthenticationProperties authProperties = new AuthenticationProperties
            //            {
            //                //AllowRefresh = < bool >,
            //                //Refreshing the authentication session should be allowed.
            //                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            //                //The time at which the authentication ticket expires.A
            //                // value set here overrides the ExpireTimeSpan option of
            //                // CookieAuthenticationOptions set with AddCookie.
            //                IsPersistent = true,
            //                //Whether the authentication session is persisted across
            //                // multiple requests.When used with cookies,
            //                //controls
            //                // whether the cookie's lifetime is absolute (matching the
            //                // lifetime of the authentication ticket) or session-based.
            //                //IssuedUtc = < DateTimeOffset >,
            //                // The time at which the authentication ticket was issued.
            //                //RedirectUri = < string >
            //                // The full path or absolute URI to be used as an http
            //                // redirect response value.
            //            };
            //        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            //        return RedirectToPage("/Admin/Index");
            //    }
            //}
            //Message = "Invalid attempt";
            //    return Page();
        }
    }
}
