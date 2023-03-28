using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserCompleteController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly ReusableSql _resuableSql;
        public UserCompleteController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _resuableSql = new ReusableSql(config);
        }

        [HttpGet("GetUsers/{userId}/{active}")]
        public IEnumerable<UserComplete> GetUsers(int userId, bool active)
        {
            string sql = @"EXEC TutorialAppSchema.spUsers_Get";
            string stringParameters = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (userId != 0) {
                stringParameters += ", @UserId=@UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }
            if (active) {
                stringParameters += ", @Active=@ActiveParameter";
                sqlParameters.Add("@ActiveParameter", active, DbType.Boolean);
            }

            if (stringParameters.Length > 0) {
                sql += stringParameters.Substring(1);  // to remove the 1st ','
            }            

            IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);

            return users;
        }

        [HttpPut("UpsertUser")]
        public IActionResult UpsertUser(UserComplete user)
        {            
            if (_resuableSql.UpsertUser(user))
            {
                return Ok();
            }

            throw new Exception("Fail to update user");
            
        }    

        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string sql = @"EXEC TutorialAppSchema.spUser_Delete 
                @UserId = @UserIdParamter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            
            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Fail to delete user");
        }   
    }
}