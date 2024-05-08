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
                (postedFile.ContentType == "image/jpeg" || postedFile.ContentType == "image/png" || postedFile.ContentType == "application/octet-stream"))
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
        public HttpResponseMessage DeletePost()
        {
            try
            {
                int pid = int.Parse(HttpContext.Current.Request.Form["pid"]);
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
                string name = HttpContext.Current.Request.Form["name"];
                string cnic = HttpContext.Current.Request.Form["cnic"];

                IQueryable<Politician> politiciansQuery = db.Politicians;

                // Perform the join with the Users table
                var query = politiciansQuery
                    .Join(db.Users,
                        politician => politician.User_cnic,
                        user => (string)user.cnic,
                        (politician, user) => new
                        {
                            Politician = politician,
                            User = user
                        });

                // Select only the Politician part from the query
                politiciansQuery = query.Select(joined => joined.Politician);


                // Filter based on the conditions
                if (!string.IsNullOrEmpty(cnic))
                {
                    // Return matching politician data based on cnic
                    politiciansQuery = politiciansQuery.Where(joined => joined.User_cnic == cnic);
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    // Return top 10 politicians with names similar to the provided name
                    politiciansQuery = politiciansQuery.Where(joined => joined.User.full_name.Contains(name)).Take(10);
                }

                // Select the desired columns from both tables
                var result = politiciansQuery
                    .Select(joined => new
                    {
                        PoliticianId = joined.id,
                        PoliticianCNIC = joined.User_cnic,
                        PoliticianPosition = joined.politicain_position,
                        PoliticianParty = joined.Party_name,
                        UserFullName = joined.User.full_name,
                        UserPassword = joined.User.password,
                        UserProvince = joined.User.user_province,
                        UserDistrict = joined.User.user_distinct,
                        UserTehsil = joined.User.user_tehsil,
                        UserUC = joined.User.user_uc,
                        UserPhone = joined.User.user_phone,
                        UserPic = joined.User.user_pic,
                        UserGender = joined.User.user_gender,
                        UserCreatedDate = joined.User.created_date,
                        UserIsDeleted = joined.User.isDeleted,
                        UserRole = joined.User.role
                    })
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
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

                String cnic = HttpContext.Current.Request.Form["cnic"];
                String uc = HttpContext.Current.Request.Form["uc"];
                if (uc != null)
                {

                    var list = db.AllPosts.Select(p => new
                    {
                        post_id = p.post_id,
                        post_date = p.post_date,
                        post_text = p.post_text,
                        post_image = p.post_image,
                        post_uc = p.post_uc,
                        user_cnic = p.user_cnic,
                        user_name = p.user_name,
                        user_picture = p.user_picture,
                        account_type = p.account_type,
                        position = p.position,
                        total_rating = p.total_rating,
                        recent_comment = p.recent_comment,
                        recent_comment_date = p.recent_comment_date,
                        status = p.status,
                        rate_score = db.Rates.FirstOrDefault(e => e.Post_id == p.post_id && e.User_cnic == cnic) != null ? db.Rates.FirstOrDefault(e => e.Post_id == p.post_id && e.User_cnic == cnic).rate_score : (int?)null,
                        followed = db.Follows.FirstOrDefault(f => f.User_cnic == cnic && f.Follower_cnic == p.user_cnic) != null ? db.Follows.FirstOrDefault(f => f.User_cnic == cnic && f.Follower_cnic == p.user_cnic).User_cnic : null,
                    }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                else
                {
                    var list = db.AllPosts.Select(p => new
                    {
                        post_id = p.post_id,
                        post_date = p.post_date,
                        post_text = p.post_text,
                        post_image = p.post_image,
                        post_uc = p.post_uc,
                        user_cnic = p.user_cnic,
                        user_name = p.user_name,
                        user_picture = p.user_picture,
                        account_type = p.account_type,
                        position = p.position,
                        total_rating = p.total_rating,
                        recent_comment = p.recent_comment,
                        recent_comment_date = p.recent_comment_date,
                        status = p.status,
                        rate_score = db.Rates.FirstOrDefault(e => e.Post_id == p.post_id && e.User_cnic == cnic) != null ? db.Rates.FirstOrDefault(e => e.Post_id == p.post_id && e.User_cnic == cnic).rate_score : (int?)null,
                        followed = db.Follows.FirstOrDefault(f => f.User_cnic == cnic && f.Follower_cnic == p.user_cnic) != null ? db.Follows.FirstOrDefault(f => f.User_cnic == cnic && f.Follower_cnic == p.user_cnic).User_cnic : null,
                    }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage AllCommentsByPostId()
        {
            try
            {
                int postId = int.Parse(HttpContext.Current.Request.Form["postId"]);
                var list = db.AllComments.Where((e) => e.post_id == postId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage UserInfoById()
        {
            try
            {
                string profileCnic = HttpContext.Current.Request.Form["profileCnic"];
                string cinc = HttpContext.Current.Request.Form["cnic"];
                var obj = db.UserInfoes.FirstOrDefault(e => e.user_cnic == profileCnic);

                if (obj != null)
                {
                    obj.is_follow = db.Follows.Any(e => e.User_cnic == profileCnic && e.Follower_cnic == cinc) ? "true" : "false";

                    // Fetch posts associated with the user
                    var userPosts = db.AllPosts.Where(p => p.politician_id == profileCnic || p.user_cnic == profileCnic).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        user_cnic = obj.user_cnic,
                        user_name = obj.user_name,
                        user_picture = obj.user_picture,
                        user_type = obj.user_type,
                        user_total_post = obj.user_total_post,
                        user_position = obj.user_position,
                        user_total_followed = obj.user_total_followed,
                        user_total_following = obj.user_total_following,
                        user_province = obj.user_province,
                        user_distinct = obj.user_distinct,
                        user_tehsil = obj.user_tehsil,
                        user_uc = obj.user_uc,
                        user_phone = obj.user_phone,
                        user_gender = obj.user_gender,
                        created_date = obj.created_date,
                        is_follow = obj.is_follow,
                        userPosts = userPosts
                    });
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
        public HttpResponseMessage RatePost()
        {
            try
            {
                int postId = int.Parse(HttpContext.Current.Request.Form["postId"]);
                int score = int.Parse(HttpContext.Current.Request.Form["score"]);
                String cnic = HttpContext.Current.Request.Form["cnic"]; 
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
        public HttpResponseMessage ReportPost()
        {
            try
            {
                String cnic = HttpContext.Current.Request.Form["cnic"];
                int postId = int.Parse(HttpContext.Current.Request.Form["postId"]);
                String reportType = HttpContext.Current.Request.Form["reportType"];
                String reportReason = HttpContext.Current.Request.Form["reportReason"];
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
        public HttpResponseMessage ReportComment()
        {
            try
            {
                String cnic = HttpContext.Current.Request.Form["cnic"];
                int commentId = int.Parse(HttpContext.Current.Request.Form["commentId"]);
                int postId = int.Parse(HttpContext.Current.Request.Form["postId"]);
                String reportType = HttpContext.Current.Request.Form["reportType"];
                String reportReason = HttpContext.Current.Request.Form["reportReason"];
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
        public HttpResponseMessage AddComment()
        {
            try
            {
                int postId = int.Parse(HttpContext.Current.Request.Form["postId"]); 
                String cnic = HttpContext.Current.Request.Form["cnic"];
                String commentText = HttpContext.Current.Request.Form["commentText"];
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
        public HttpResponseMessage FollowById()
        {
            try
            {
                string userCnic = HttpContext.Current.Request.Form["userCnic"];
                string accountCnic = HttpContext.Current.Request.Form["accountCnic"];
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
        public HttpResponseMessage LikePartyById()
        {
            try
            {
                string userCnic = HttpContext.Current.Request.Form["userCnic"];
                int partyId = int.Parse(HttpContext.Current.Request.Form["partyId"]);
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
                    if (postedFile != null && postedFile.ContentLength > 0)
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
        public HttpResponseMessage ChangeParty()
        {
            try
            {
                string userCnic = HttpContext.Current.Request.Form["userCnic"];
                string party = HttpContext.Current.Request.Form["party"];
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
