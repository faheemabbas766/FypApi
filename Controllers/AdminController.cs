using FypApi.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls.WebParts;

namespace FypApi.Controllers
{
    public class AdminController : ApiController
    {
        V1Entities db = new V1Entities();

        [HttpPost]
        public HttpResponseMessage AllUpgradeRequests()
        {
            try
            {
                var list = db.UpgradeRequsets.Where((e) => e.request_status == "pending").Select(s => new
                {
                    s.request_type,
                    s.User_cnic,
                    s.User.full_name,
                    s.User.user_pic,
                    s.request_status,
                    s.request_document,
                    s.request_date,
                }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage AllReports()
        {
            try
            {
                var list = db.Reports
                    .Where(e => e.report_status == "pending")
                    .Select(r => new
                    {
                        ReportId = r.id,
                        ReporterCnic = r.User.cnic,
                        ReporterName = r.User.full_name,
                        ReporterPicture = r.User.user_pic,
                        ReporterGender = r.User.user_gender,
                        ReporterRole = r.User.role,
                        ReportType = r.Comment_id == null ? "Post" : "Comment",
                        ReportedItemId = r.Comment_id == null ? r.Post.id : r.Comment.id,
                        ReportedItemContent = r.Comment_id == null ? r.Post.post_text : r.Comment.comment_text,
                        ReportDate = r.report_date,
                        ReportStatus = r.report_status,
                        ReportReason = r.report_reason,
                        PostImage = r.Post.post_image,
                        PostText = r.Post.post_text,
                        PostDate = r.Post.post_date,
                    })
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        public HttpResponseMessage AllReviewPost()
        {
            try
            {
                var list = db.AllPosts.Where((e) => e.status == "Review").ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateReviewPost()
        {
            try
            {
                int pid = int.Parse(HttpContext.Current.Request.Form["pid"]);
                string status = HttpContext.Current.Request.Form["status"];

                var post = db.Posts.Where((e) => e.id == pid).FirstOrDefault();
                if (post != null)
                {
                    post.status = status;
                    return Request.CreateResponse(HttpStatusCode.OK, "Review Complete!!!");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Review Failed!!!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddAdmin()
        {
            try
            {
                string cnic = HttpContext.Current.Request.Form["cnic"];
                string type = HttpContext.Current.Request.Form["type"];

                var res = db.Admins.Where((e) => e.User_cnic == cnic).FirstOrDefault();
                if (res != null)
                {
                    res.type = type;
                    var a = db.Users.Where((i) => i.cnic == cnic).FirstOrDefault();
                    a.role = "Admin";
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Admin Updated");
                }
                else
                {
                    Admin newAdmin = new Admin();
                    newAdmin.type = type;
                    newAdmin.User_cnic =  cnic;
                    db.Admins.Add(newAdmin);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Admin Added");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddJournalist()
        {
            try
            {
                string cnic = HttpContext.Current.Request.Form["cnic"];
                string experience = HttpContext.Current.Request.Form["experience"];
                string reference = HttpContext.Current.Request.Form["reference"];

                var res = db.Journalists.Where((e) => e.User_cnic == cnic).FirstOrDefault();
                if (res != null)
                {
                    res.reference = reference;
                    res.experience = experience;
                    var a = db.Users.Where((i) => i.cnic == cnic).FirstOrDefault();
                    a.role = "Journalist";
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Journalist Updated");
                }
                else
                {
                    db.Journalists.Add(new Journalist() { User_cnic = cnic, experience = experience, reference = reference });
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Journalist Added");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddPolitician()
        {
            try
            {
                string politician_position = HttpContext.Current.Request.Form["politician_position"];
                string politician_type = HttpContext.Current.Request.Form["politician_type"];
                string cnic = HttpContext.Current.Request.Form["cnic"];
                string party = HttpContext.Current.Request.Form["party"];

                var res = db.Politicians.Where((e) => e.User_cnic == cnic).FirstOrDefault();
                if (res != null)
                {
                    res.politicain_position = politician_position;
                    res.politician_type = politician_type;
                    res.Party_name = party;
                    var a = db.Users.Where((i) => i.cnic == cnic).FirstOrDefault();
                    a.role = "Politician";
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Politician Updated");
                }
                else
                {
                    db.Politicians.Add(new Politician() { User_cnic = cnic, politicain_position = politician_position, politician_type = politician_type, Party_name = party });
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Politician Added");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddParty()
        {
            try
            {
                string name = HttpContext.Current.Request.Form["party_name"];
                string symbol = HttpContext.Current.Request.Form["party_symbol"];
                var postedFile = HttpContext.Current.Request.Files["party_flag"];

                var existingParty = db.Parties.FirstOrDefault(e => e.party_name == name);

                if (existingParty == null)
                {
                    string fileName = "abc.png"; // Default file name

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        fileName = Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName);
                        string filePath = HttpContext.Current.Server.MapPath("~/Uploads/Flags/") + fileName;
                        postedFile.SaveAs(filePath);
                    }

                    var newParty = new Party
                    {
                        party_name = name,
                        party_symbol = symbol,
                        party_flag = fileName
                    };

                    db.Parties.Add(newParty);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, "Party Added");
                }
                else
                {
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        string oldFilePath = HttpContext.Current.Server.MapPath("~/Uploads/Flags/") + existingParty.party_flag;

                        // Delete old party flag file
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName);
                        string filePath = HttpContext.Current.Server.MapPath("~/Uploads/Flags/") + fileName;
                        postedFile.SaveAs(filePath);

                        existingParty.party_symbol = symbol;
                        existingParty.party_flag = fileName;

                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Party Updated");
                    }
                    else
                    {
                        existingParty.party_symbol = symbol;
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Party details updated");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateRequest()
        {
            try
            {
                string cnic = HttpContext.Current.Request.Form["cnic"];
                string status = HttpContext.Current.Request.Form["status"];

                UpgradeRequset upgradeRequest = db.UpgradeRequsets.Where(e => e.User_cnic == cnic).OrderByDescending(e => e.request_date).FirstOrDefault();
                if (upgradeRequest != null)
                {
                    upgradeRequest.request_status = status;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Request Completed");
                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Request Failed");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
