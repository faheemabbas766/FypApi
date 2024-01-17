using FypApi.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BCrypt.Net;
using System.Web.Helpers;

namespace FypApi.Controllers
{
    public class AuthController : ApiController
    {
        V1Entities db = new V1Entities();

        [HttpPost]
        public HttpResponseMessage Login(String cnic, String password)
        {
            try
            {
                User user = db.Users.FirstOrDefault(e => e.cnic == cnic);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.password))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, user);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Invalid Credential");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage SignUp(User u)
        {
            try
            {
                // Hash the password before storing it
                u.password = BCrypt.Net.BCrypt.HashPassword(u.password);

                db.Users.Add(u);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, u);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage ForgetPassword(String cnic, String password)
        {
            try
            {
                var user = db.Users.FirstOrDefault(e => e.cnic == cnic);

                if (user != null)
                {
                    // Hash the new password before updating
                    user.password = BCrypt.Net.BCrypt.HashPassword(password);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Password Change Successful");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "User does not exist");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
