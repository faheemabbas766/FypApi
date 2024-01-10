using FypApi.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
                User obj = db.Users.Where((e) => e.cnic == cnic && e.password == password).FirstOrDefault();
                if (obj != null)
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                else
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Invalid Credential");
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
                var u = db.Users.Where((e) => e.cnic == cnic).FirstOrDefault();
                if(u != null)
                {
                    u.password = password;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Password Change Successful");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "user does not exist");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
