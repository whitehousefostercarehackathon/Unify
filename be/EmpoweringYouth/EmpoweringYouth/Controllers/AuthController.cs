﻿using EmpoweringYouth.Context;
using EmpoweringYouth.Models;
using EmpoweringYouth.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EmpoweringYouth.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        [HttpPost]
        [Route("signin")]
        public IHttpActionResult SignIn([Required]LoginData loginData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();

            }

            using (var ctx = new EmpoweringYouthContext())
            {
                var passwordHash = AuthService.GeneratePasswordHash(loginData.password);
                var users = ctx.users.Where(u => u.Username == loginData.username && u.Password == passwordHash);
                User user = users.Count() > 0 ? users.First() : null;
                if (user == null)
                {
                    return BadRequest();
                }


                foreach (KeyValuePair<String, User> entry in AuthService.userCache.ToList())
                {
                    if (entry.Value.Id.Equals(user.Id))
                    {
                        AuthService.userCache.Remove(entry.Key);
                    }
                }

                var token = AuthService.GenerateToken(user);
                AuthService.userCache.Add(token, user);

                var response = new Dictionary<String, String>();
                response.Add("token", token);
                response.Add("firstname", user.Firstname);
                response.Add("lastname", user.Lastname);
                return Ok(response);
            }

        }

        [HttpPost]
        [Route("signup")]
        public IHttpActionResult SignUp(User user)
        {
            using (var ctx = new EmpoweringYouthContext())
            {
                user.Role = Role.CW;
                user.Password = AuthService.GeneratePasswordHash(user.Password);
                ctx.users.Add(user);
                ctx.SaveChanges();
                return Ok(user);
            }
        }

        [HttpGet]
        [Route("session")]
        public IHttpActionResult Session()
        {
            IEnumerable<string> requestHeaders;
            var authHeader = string.Empty;

            if (Request.Headers.TryGetValues("Authorization", out requestHeaders))
            {
                authHeader = requestHeaders.FirstOrDefault();
                User user = AuthService.GetFromCache(authHeader);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            return NotFound();
        }


    }
}



