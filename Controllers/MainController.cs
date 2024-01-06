using FypApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FypApi.Controllers
{
    public class MainController : ApiController
    {
        V1Entities db = new V1Entities();
        [HttpPost]
        public HttpResponseMessage AllPost(String uc)
        {
            try
            {
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
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Post Report Successful");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
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
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Comment Report Successful");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
