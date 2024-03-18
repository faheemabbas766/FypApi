using FypApi.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BCrypt.Net;
using System.Web.Helpers;
using System.Web;
using System.IO;

namespace FypApi.Controllers
{
    public class AuthController : ApiController
    {
        V1Entities db = new V1Entities();

        [HttpPost]
        public HttpResponseMessage Login()
        {
            try
            {
                string cnic = HttpContext.Current.Request.Form["cnic"];
                string password = HttpContext.Current.Request.Form["password"];
                var user = db.Users.Where(e => e.cnic == cnic && e.isDeleted == 0).Select(s => new
                {
                    s.cnic,
                    s.full_name,
                    s.password,
                    s.user_province,
                    s.user_distinct,
                    s.user_tehsil,
                    s.user_uc,
                    s.user_phone,
                    s.user_pic,
                    s.user_gender,
                    s.role
                }).FirstOrDefault();


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
        public HttpResponseMessage SignUp()
        {
            try
            {
                string cnic = HttpContext.Current.Request.Form["cnic"];
                char lastDigitChar = cnic[cnic.Length - 1];
                int lastDigit = int.Parse(lastDigitChar.ToString());
                User u = db.Users.Where((e) => e.cnic == cnic).FirstOrDefault();
                if(u == null) {
                    u = new User
                    {
                        cnic = cnic,
                        full_name = HttpContext.Current.Request.Form["full_name"],
                        password = BCrypt.Net.BCrypt.HashPassword(HttpContext.Current.Request.Form["password"]),
                        user_province = HttpContext.Current.Request.Form["user_province"],
                        user_distinct = HttpContext.Current.Request.Form["user_district"],
                        user_tehsil = HttpContext.Current.Request.Form["user_tehsil"],
                        user_uc = HttpContext.Current.Request.Form["user_uc"],
                        user_phone = HttpContext.Current.Request.Form["user_phone"],
                        isDeleted = 0,
                        role = "Citizen",
                        user_gender = (lastDigit % 2 == 0) ? "female" : "male",
                        created_date = DateTime.Now,
                    };

                    var postedFile = HttpContext.Current.Request.Files["user_pic"];
                    if (postedFile != null && postedFile.ContentLength > 0 &&
                        (postedFile.ContentType == "image/jpeg" || postedFile.ContentType == "image/png"))
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName);
                        string filePath = HttpContext.Current.Server.MapPath("~/Uploads/Profile/") + fileName;
                        postedFile.SaveAs(filePath);
                        u.user_pic = fileName;
                    }
                    db.Users.Add(u);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, u);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "User Already Exist!");
                }
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
