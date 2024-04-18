using FypApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ProfanityFilter;

namespace FypApi.Controllers
{
    public class MainController : ApiController
    {
        static List<string> filterWords = new List<string> {
            "badword1",
            "badword2",
            "badword3",
            "badword4",
            "badword5",
            "badword6",
            "badword7",
            "badword8",
            "badword9",
        };
        private static ProfanityFilter.ProfanityFilter profanityFilter = new ProfanityFilter.ProfanityFilter(filterWords);
        V1Entities db = new V1Entities();

        [HttpPost]
        public HttpResponseMessage AddPost()
        {
            try
            {
                var post = new Post
                {
                    post_date = DateTime.Now,
                    post_text = HttpContext.Current.Request.Form["post_text"],
                    User_cnic = HttpContext.Current.Request.Form["user_cnic"],
                    post_uc = HttpContext.Current.Request.Form["post_uc"],
                    politician_id = HttpContext.Current.Request.Form["politician_id"],
                };
                string[] words = post.post_text.Split(' ');
                bool containsProfanity = false;
                foreach (var word in words)
                {
                    if (profanityFilter.IsProfanity(word))
                    {
                        containsProfanity = true;
                        break;
                    }
                }

                if (containsProfanity)
                {
                    post.status = "Review";
                }
                else
                {
                    post.status = "Approved";
                }
                var postedFile = HttpContext.Current.Request.Files["post_image"];
                if (postedFile != null && postedFile.ContentLength > 0 &&
                (postedFile.ContentType == "image/jpeg" || postedFile.ContentType == "image/png"))
                {
                    // Generate a unique file name to avoid overwriting existing files
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName);

                    // Define the folder to save the uploaded images (adjust the path as needed)
                    string filePath = HttpContext.Current.Server.MapPath("~/Uploads/") + fileName;

                    // Save the file to the server
                    postedFile.SaveAs(filePath);
                    post.post_image = fileName;
                }
                db.Posts.Add(post);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, post.status);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage DeletePost(int pid)
        {
            try
            {
                var post = db.Posts.Find(pid);
                if (post != null)
                {
                    post.status = "Delete";
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Post Deleted!!!");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Post Not Found!!!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage AllParties()
        {
            try
            {
                var list = db.Parties.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage AllPolitician()
        {
            try
            {
                String name = HttpContext.Current.Request.Form["name"];
                String cnic = HttpContext.Current.Request.Form["cnic"];
                var list = db.Parties.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage AllPost()
        {
            try
            {

                String uc = HttpContext.Current.Request.Form["uc"];
                if (uc != null)
                {
                    var list = db.AllPosts.Where((e) => e.post_uc == uc).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    var list = db.AllPosts;
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage AllCommentsByPostId(int postId)
        {
            try
            {
                var list = db.AllComments.Where((e) => e.post_id == postId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage UserInfoById(string profileCinc, string cinc)
        {
            try
            {
                var obj = db.UserInfoes.FirstOrDefault(e => e.user_cnic == profileCinc);

                if (obj != null)
                {
                    obj.is_follow = db.Follows.Any(e => e.User_cnic == profileCinc && e.Follower_cnic == cinc) ? "true" : "false";
                    return Request.CreateResponse(HttpStatusCode.OK, obj);
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
        [HttpPost]
        public HttpResponseMessage RatePost(int postId, int score, String cnic)
        {
            try
            {
                Rate r = db.Rates.Where((i) => i.Post_id == postId && i.User_cnic == cnic).FirstOrDefault();
                if (r != null)
                {
                    r.rate_score = score;
                    r.rate_date = DateTime.Now;
                    r.Post_id = postId;
                }
                else
                {
                    r = new Rate();
                    r.rate_score = score;
                    r.rate_date = DateTime.Now;
                    r.Post_id = postId;
                    r.User_cnic = cnic;
                    db.Rates.Add(r);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Rate Successful");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage ReportPost(String cnic, int postId, String reportType, String reportReason)
        {
            try
            {
                var report = db.Reports.Where((i) => i.Post_id == postId && i.User_cnic == cnic).FirstOrDefault();
                if (report != null)
                {
                    report.report_reason = reportReason;
                    report.report_type = reportType;
                }
                else
                {
                    report = new Report();
                    report.report_date = DateTime.Now;
                    report.report_status = "pending";
                    report.report_type = reportType;
                    report.report_reason = reportReason;
                    report.Post_id = postId;
                    report.User_cnic = cnic;
                    db.Reports.Add(report);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Post Report Successful");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage ReportComment(String cnic, int commentId, int postId, String reportType, String reportReason)
        {
            try
            {
                var report = db.Reports.Where((i) => i.Comment_id == commentId && i.User_cnic == cnic).FirstOrDefault();
                if (report != null)
                {
                    report.report_reason = reportReason;
                    report.report_type = reportType;
                }
                else
                {
                    report = new Report();
                    report.report_date = DateTime.Now;
                    report.report_status = "pending";
                    report.report_type = reportType;
                    report.report_reason = reportReason;
                    report.Post_id = postId;
                    report.User_cnic = cnic;
                    report.Comment_id = commentId;
                    db.Reports.Add(report);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Comment Report Successful");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage AddComment(int postId, String cnic, String commentText)
        {
            try
            {
                db.Comments.Add(new Comment() { User_cnic = cnic, Post_id = postId, comment_date = DateTime.Now, comment_text = commentText });
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Comment Added");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage FollowById(string userCnic, string accountCnic)
        {
            try
            {
                var existingFollow = db.Follows.FirstOrDefault(f => f.User_cnic == userCnic && f.Follower_cnic == accountCnic);

                if (existingFollow == null)
                {
                    db.Follows.Add(new Follow() { User_cnic = userCnic, Follower_cnic = accountCnic });
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Followed Successful");
                }
                else
                {
                    db.Follows.Remove(existingFollow);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Unfollow Successful");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage LikePartyById(string userCnic, int partyId)
        {
            try
            {
                var existingLike = db.Likes.FirstOrDefault(f => f.User_cnic == userCnic && f.Paty_id == partyId);
                if (existingLike == null)
                {
                    db.Likes.Add(new Like() { User_cnic = userCnic, Paty_id = partyId });
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Like Successful");
                }
                else
                {
                    db.Likes.Remove(existingLike);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "UnLike Successful");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage UpgradeAccountRequest()
        {
            try
            {
                String User_cnic = HttpContext.Current.Request.Form["User_cnic"];
                var res = db.UpgradeRequsets.FirstOrDefault((e) => e.User_cnic == User_cnic && e.request_status == "Pending");
                if(res == null)
                {
                    var ur = new UpgradeRequset
                    {
                        User_cnic = User_cnic,
                        request_date = DateTime.Now,
                        request_status = "Pending",
                        request_type = HttpContext.Current.Request.Form["request_type"],
                    };
                    var postedFile = HttpContext.Current.Request.Files["request_document"];
                    if (postedFile != null && postedFile.ContentLength > 0 &&
                    (postedFile.ContentType == "image/jpeg" || postedFile.ContentType == "image/png"))
                    {
                        // Generate a unique file name to avoid overwriting existing files
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName);

                        // Define the folder to save the uploaded images (adjust the path as needed)
                        string filePath = HttpContext.Current.Server.MapPath("~/Uploads/UpgradeAccountRequest/") + fileName;

                        // Save the file to the server
                        postedFile.SaveAs(filePath);
                        ur.request_document = fileName;
                    }
                    db.UpgradeRequsets.Add(ur);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, ur);
                }else
                    return Request.CreateResponse(HttpStatusCode.OK, "Your Request is already Pending");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage ChangeParty(string userCnic, string party)
        {
            try
            {
                var politician = db.Politicians.FirstOrDefault(f => f.User_cnic == userCnic);
                if (politician != null)
                {
                    politician.Party_name = party;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Party Change Successful");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Politician not Found!!!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
