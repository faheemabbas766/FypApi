﻿using FypApi.Models;
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
                var list = db.AllReports.Where((e) => e.report_status == "pending").ToList();
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
        public HttpResponseMessage UpdateReviewPost(int pid, String status)
        {
            try
            {
                var post = db.Posts.Where((e)=> e.id == pid).FirstOrDefault();
                if(post != null)
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
        public HttpResponseMessage AddAdmin(String cnic, String type)
        {
            try
            {
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
                    db.Admins.Add(new Admin() { User_cnic = cnic, type = type });
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
        public HttpResponseMessage AddJournalist(String cnic, String experience, String reference)
        {
            try
            {
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
        public HttpResponseMessage AddPolitician(String politician_position, String politician_type, String cnic,String party=null)
        {
            try
            {
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

                    if (postedFile != null && postedFile.ContentLength > 0 &&
                        (postedFile.ContentType == "image/jpeg" || postedFile.ContentType == "image/png"))
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
                    if (postedFile != null && postedFile.ContentLength > 0 &&
                        (postedFile.ContentType == "image/jpeg" || postedFile.ContentType == "image/png"))
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
        public HttpResponseMessage UpdateRequest(String cnic, String status)
        {
            try
            {
                UpgradeRequset upgradeRequest = db.UpgradeRequsets.Where(e => e.User_cnic == cnic).OrderByDescending(e => e.request_date).FirstOrDefault();
                if (upgradeRequest != null)
                {
                    upgradeRequest.request_status = status; 
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Request Completed");
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
