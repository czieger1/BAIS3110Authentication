using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using BAIS3110Authentication.Secured;

namespace BAIS3110Authentication.Pages.Admin
{
    [Authorize(Policy = "RequireAdmin")]
    public class RegisterModel : PageModel
    {
        [BindProperty, Required]
        public string UserName { get; set; }
        [BindProperty, Required]
        public string UserEmail { get; set; }
        [BindProperty, Required]
        public string UserRole { get; set; }
        [BindProperty, DataType(DataType.Password), Required]
        public string UserPassword { get; set; }
        public string Message { get; set; }
        
        private string HashedPassword;
        private static byte[] SaltedPassword;
        private static string SaltedString;

        public void OnPost()
        {
            //secure password
            Security SecurePassword = new Security();
            SaltedPassword = SecurePassword.GenerateSalt(16);
            HashedPassword = SecurePassword.CalculateHash(UserPassword, SaltedPassword);

            //SaltedString = Convert.ToBase64String(SaltedPassword);

            //connecting to db
            string ConnectionString = Startup.ConnectionString;
            SqlConnection BAIS3150Connection = new SqlConnection(ConnectionString);
            BAIS3150Connection.Open();

            //Add Program 
            SqlCommand AddUser = new SqlCommand();
            AddUser.Connection = BAIS3150Connection;
            AddUser.CommandType = CommandType.StoredProcedure;
            AddUser.CommandText = "P_AddUser";

            //setting the parameters rewrites the variable each time 
            SqlParameter ProgramParameter;
            //add program code 
            ProgramParameter = new SqlParameter
            {
                ParameterName = "@UserName",
                SqlDbType = SqlDbType.VarChar,
                Direction = ParameterDirection.Input,
                SqlValue = UserName
            };
            AddUser.Parameters.Add(ProgramParameter);

            //add program description
            ProgramParameter = new SqlParameter
            {
                ParameterName = "@UserEmail",
                SqlDbType = SqlDbType.VarChar,
                Direction = ParameterDirection.Input,
                SqlValue = UserEmail
            };
            AddUser.Parameters.Add(ProgramParameter);
            ProgramParameter = new SqlParameter
            {
                ParameterName = "@SaltedPassword",
                SqlDbType = SqlDbType.Binary,
                Direction = ParameterDirection.Input,
                SqlValue = SaltedPassword
            };
            AddUser.Parameters.Add(ProgramParameter);
            ProgramParameter = new SqlParameter
            {
                ParameterName = "@HashedPassword",
                SqlDbType = SqlDbType.Char,
                Direction = ParameterDirection.Input,
                SqlValue = HashedPassword
            };
            AddUser.Parameters.Add(ProgramParameter);
            ProgramParameter = new SqlParameter
            {
                ParameterName = "@UserRole",
                SqlDbType = SqlDbType.VarChar,
                Direction = ParameterDirection.Input,
                SqlValue = UserRole
            };
            AddUser.Parameters.Add(ProgramParameter);
            try
            {
                AddUser.ExecuteNonQuery();
                Message = "UserAdded";
                BAIS3150Connection.Close();
            }
            catch (Exception e)
            {
                Message = e.ToString();
                BAIS3150Connection.Close();
            }

        }
    }
}

