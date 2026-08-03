using RealEstateRegalSpace.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RealEstateRegalSpace.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Login()
        {
            Session.Abandon();
            string pass = Crypto.Decrypt("cnwtCUx2Uwywr0C4A+Ws76+Whv2zkfWgxn8mink0w1k=");
            return View();
        }

        [HttpPost]
        public ActionResult Login(Login obj)
        {
            try
            {
                obj.password = Crypto.Encrypt(obj.password);
                DataSet ds = obj.CheckLogin();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) 
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    string msg = ds.Tables[0].Columns.Contains("Msg") ? dr["Msg"].ToString() : "";
                    if (msg == "1")
                    {
                        string userType = ds.Tables[0].Columns.Contains("UserType") ? dr["UserType"].ToString() : "";
                        if (userType.Equals("Associate", StringComparison.OrdinalIgnoreCase))
                        {
                            Session["Pk_userId"] = ds.Tables[0].Columns.Contains("Pk_userId") ? dr["Pk_userId"].ToString() : "";
                            Session["FullName"] = ds.Tables[0].Columns.Contains("FullName") ? dr["FullName"].ToString() : "";
                            Session["LoginId"] = ds.Tables[0].Columns.Contains("LoginId") ? dr["LoginId"].ToString() : "";
                            Session["CommissionType"] = ds.Tables[0].Columns.Contains("CommissionType") ? dr["CommissionType"].ToString() : "";
                            Session["UserImage"] = ds.Tables[0].Columns.Contains("Profile") ? dr["Profile"].ToString() : "";
                            
                            string commType = Session["CommissionType"] != null ? Session["CommissionType"].ToString() : "";
                            if (commType.Equals("Level", StringComparison.OrdinalIgnoreCase))
                            {
                                return RedirectToAction("DashBoard", "Associate");
                            }
                            else
                            {
                                return RedirectToAction("DirectDashBoard", "Associate");
                            }
                        }
                        else if (userType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                        {
                            Session["Pk_userId"] = ds.Tables[0].Columns.Contains("Pk_userId") ? dr["Pk_userId"].ToString() : "";
                            Session["FullName"] = ds.Tables[0].Columns.Contains("FullName") ? dr["FullName"].ToString() : "";
                            Session["LoginId"] = ds.Tables[0].Columns.Contains("LoginId") ? dr["LoginId"].ToString() : "";
                            return RedirectToAction("DashBoard", "Customer");
                        }
                        else
                        {
                            Session["Pk_adminId"] = ds.Tables[0].Columns.Contains("Pk_adminId") ? dr["Pk_adminId"].ToString() : "";
                            Session["FullName"] = ds.Tables[0].Columns.Contains("Name") ? dr["Name"].ToString() : "";
                            Session["LoginId"] = ds.Tables[0].Columns.Contains("LoginId") ? dr["LoginId"].ToString() : "";
                            Session["RoleName"] = ds.Tables[0].Columns.Contains("UsertypeName") ? dr["UsertypeName"].ToString() : "";
                            return RedirectToAction("DashBoard", "Admin");
                        }
                    }
                    else
                    {
                        string errMsg = ds.Tables[0].Columns.Contains("ErrorMessage") ? dr["ErrorMessage"].ToString() : "Invalid Login ID or Password";
                        TempData["Login"] = !string.IsNullOrEmpty(errMsg) ? errMsg : "Invalid Login ID or Password";
                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    TempData["Login"] = "Invalid Login ID or Password";
                    return RedirectToAction("Login");
                }
            }
            catch(Exception ex)
            {
                TempData["Login"] = ex.Message;
                return RedirectToAction("Login");
            }
            return View();
        }
        public ActionResult Registration()
        {
            Registration obj = new Registration();
            #region ForQueryString
            if (Request.QueryString["Pid"] != null)
            {
                obj.sponserId = Request.QueryString["Pid"].ToString();
            }
            if (Request.QueryString["lg"] != null)
            {
                obj.leg = Request.QueryString["lg"].ToString();
                if (obj.leg == "Right")
                {
                    ViewBag.RightChecked = "checked";
                    ViewBag.LeftChecked = "";
                }
                else
                {
                    ViewBag.RightChecked = "";
                    ViewBag.LeftChecked = "checked";
                }
            }
            if (Request.QueryString["Pid"] != null)
            {
                Common objcomm = new Common();
                objcomm.sponserId = obj.sponserId;
                DataSet ds = objcomm.GetMemberDetails();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {

                    obj.sponsorName = ds.Tables[0].Rows[0]["FullName"].ToString();
                }
            }
            else
            {
                ViewBag.RightChecked = "";
                ViewBag.LeftChecked = "checked";
            }
            #endregion ForQueryString

            #region BindGender
            List<SelectListItem> lstgender = Common.BindGender();
            ViewBag.ddlGender = lstgender;
            #endregion BindGenders
            return View(obj);
        }
        [HttpPost]
        public ActionResult Registration(Registration obj)
        {
            #region BindGender
            List<SelectListItem> lstgender = Common.BindGender();
            ViewBag.ddlGender = lstgender;
            #endregion BindGenders
            string password = Common.GenerateRandom();
            obj.password = Crypto.Encrypt(password);
            DataSet ds = obj.SaveRegistration();
            if (ds != null)
            {
                if (ds.Tables[0].Rows[0]["Msg"].ToString() == "1")
                {
                    Session["AssociateLoginId"] = ds.Tables[0].Rows[0]["LoginId"].ToString();
                    Session["AssociateName"] = ds.Tables[0].Rows[0]["Name"].ToString();
                    Session["Password"] = Crypto.Decrypt(ds.Tables[0].Rows[0]["Password"].ToString());
                    string msg = "Dear " + Session["AssociateName"].ToString() + " Welcome to GARDENESTATE VENTURES.Your Login Id " + Session["AssociateLoginId"].ToString() + " and Password is " + Session["Password"].ToString() + ". Kindly login www.gardenestateventures.com Thankyou";
                    BLSMS.SendSMSNew(obj.mobileNo, msg);
                    return RedirectToAction("ConfirmationPage");
                }
                else
                { 
                    return View(obj);
                }
            }
            return RedirectToAction("ConfirmationPage");
        }
        public ActionResult GetSponserDetails(string ReferBy)
        {
            Common obj = new Common();
            try
            {
                obj.sponserId = ReferBy;
                DataSet ds = obj.GetMemberDetails();
                if (ds != null)
                {
                    if (ds.Tables[0].Rows[0]["Msg"].ToString() == "1")
                    {
                        obj.DisplayName = ds.Tables[0].Rows[0]["FullName"].ToString();
                        obj.Result = "Yes";
                    }
                    else
                    {
                        obj.Result = "Invalid Sponsor Id";
                    }
                }
                else
                {
                    obj.Result = "Invalid Sponsor Id";
                }
            }
            catch (Exception ex)
            {
                obj.Result = ex.Message;
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConfirmationPage()
        {
            return View();
        }
        public ActionResult DecrptPass()
        {
            string pass = Crypto.Decrypt("PRWTD63AlCCbBn9O0qghTg==");
            return View();
        }
        public ActionResult GetStateCity(string PinCode)
        {
            Common obj = new Common();
            try
            {
                obj.pinCode = PinCode;
                DataSet ds = obj.GetStateCity();
                if (ds != null)
                {
                    if (ds.Tables[0].Rows[0]["Msg"].ToString() == "1")
                    {
                        obj.State = ds.Tables[0].Rows[0]["State"].ToString();
                        obj.City = ds.Tables[0].Rows[0]["City"].ToString();
                        obj.Result = "1";
                    }
                    else
                    {
                        obj.Result = "Invalid PinCode";
                    }
                }
                else
                {
                    obj.Result = "Invalid PinCode";
                }
            }
            catch (Exception ex)
            {
                obj.Result = ex.Message;
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PlotAvailbility()
        {
            Master model = new Master();

            #region ddlSites
            int count = 0;
            List<SelectListItem> ddlSite = new List<SelectListItem>();
            DataSet ds1 = model.GetSiteList();
            if (ds1 != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in ds1.Tables[0].Rows)
                {
                    if (count == 0)
                    {
                        ddlSite.Add(new SelectListItem { Text = "Select Site", Value = "0" });
                    }
                    ddlSite.Add(new SelectListItem { Text = r["SiteName"].ToString(), Value = r["PK_SiteID"].ToString() });
                    count = count + 1;
                }
            }

            ViewBag.ddlSite = ddlSite;

            #endregion

            List<SelectListItem> ddlSector = new List<SelectListItem>();
            ddlSector.Add(new SelectListItem { Text = "Select Sector", Value = "0" });
            ViewBag.ddlSector = ddlSector;

            List<SelectListItem> ddlBlock = new List<SelectListItem>();
            ddlBlock.Add(new SelectListItem { Text = "Select Block", Value = "0" });
            ViewBag.ddlBlock = ddlBlock;

            // Fetch Plot Stats for initial load (All Sites)
            Reports report = new Reports();
            DataSet dsStats = report.GetPlotCount();
            if (dsStats != null && dsStats.Tables.Count > 0)
            {
                ViewBag.PlotStats = dsStats.Tables[0];
                ViewBag.SiteStats = dsStats.Tables[0];
            }

            // Default to Ganga Enclave (SiteID = 17) on initial GET load for ultra-fast <0.1s page load
            model.SiteID = "17";
            DataSet ds = model.GetPlotList();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                List<Master> lst = new List<Master>();
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    Master obj = new Master();
                    obj.PlotID = r["PK_PlotID"].ToString();
                    obj.SiteID = r["PK_SiteID"].ToString();
                    obj.SectorID = r["PK_SectorID"].ToString();
                    obj.BlockID = r["PK_BlockID"].ToString();
                    obj.EncryptKey = r["PK_PlotID"].ToString();
                    obj.SiteName = r["SiteName"].ToString();
                    obj.SectorName = r["SectorName"].ToString();
                    obj.BlockName = r["BlockName"].ToString();
                    obj.PlotNumber = r["PlotNumber"].ToString();
                    obj.PlotArea = r["TotalArea"].ToString();
                    obj.PlotSize = r.Table.Columns.Contains("PlotSize") ? r["PlotSize"].ToString() : "";
                    obj.PlotRate = r["PlotRate"].ToString();
                    obj.PlotAmount = r["PlotAmount"].ToString();
                    obj.PLCCharge = r["PLCCharge"].ToString();
                    obj.BookingPercent = r["BookingPercent"].ToString();
                    obj.PLCNames = r["plcnames"].ToString();
                    obj.AllottmentPercent = (r["AllottmentPercent"].ToString());
                    obj.PlotStatus = (r["PlotStatus"].ToString());
                    obj.ColorCSS = (r["StatusColor"].ToString());
                    lst.Add(obj);
                }
                model.lstPlot = lst;
            }

            return View(model);
        }
        public ActionResult GetBlockList(string SiteID, string SectorID)
        {
            List<SelectListItem> lstBlock = new List<SelectListItem>();
            Master model = new Master();
            model.SiteID = SiteID == "0" ? null : SiteID;
            model.SectorID = SectorID == "0" ? null : SectorID;
            DataSet dsblock = model.GetBlockList();

            #region ddlBlock
            if (dsblock != null && dsblock.Tables.Count > 0 && dsblock.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow dr in dsblock.Tables[0].Rows)
                {
                    lstBlock.Add(new SelectListItem { Text = dr["BlockName"].ToString(), Value = dr["PK_BlockID"].ToString() });
                }

            }

            model.lstBlock = lstBlock;
            #endregion

            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSiteDetails(string SiteID)
        {
            try
            {
                Master model = new Master();
                model.SiteID = SiteID;


                #region GetSectors
                List<SelectListItem> ddlSector = new List<SelectListItem>();
                DataSet dsSector = model.GetSectorList();

                if (dsSector != null && dsSector.Tables.Count > 0)
                {
                    foreach (DataRow r in dsSector.Tables[0].Rows)
                    {
                        ddlSector.Add(new SelectListItem { Text = r["SectorName"].ToString(), Value = r["PK_SectorID"].ToString() });

                    }
                    model.Result = "yes";
                }
                model.ddlSector = ddlSector;
                #endregion

                #region SitePLCCharge
                List<Master> lstPlcCharge = new List<Master>();
                DataSet dsPlcCharge = model.GetPLCChargeList();

                if (dsPlcCharge != null && dsPlcCharge.Tables.Count > 0)
                {
                    foreach (DataRow r in dsPlcCharge.Tables[0].Rows)
                    {
                        Master obj = new Master();

                        obj.PLCName = r["PLCName"].ToString();
                        obj.PLCCharge = "0";
                        obj.PLCID = r["PK_PLCID"].ToString();

                        lstPlcCharge.Add(obj);
                    }
                    model.lstPLC = lstPlcCharge;
                }
                #endregion
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult PlotAvailbility(Master model)
        {
            List<Master> lst = new List<Master>();
            model.SiteID = model.SiteID == "0" ? null : model.SiteID;
            model.SectorID = model.SectorID == "0" ? null : model.SectorID;
            model.BlockID = model.BlockID == "0" ? null : model.BlockID;

            DataSet ds = model.GetPlotList();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    Master obj = new Master();
                    obj.PlotID = r["PK_PlotID"].ToString();
                    obj.SiteID = r["PK_SiteID"].ToString();
                    obj.SectorID = r["PK_SectorID"].ToString();
                    obj.BlockID = r["PK_BlockID"].ToString();
                    obj.EncryptKey = r["PK_PlotID"].ToString();
                    obj.SiteName = r["SiteName"].ToString();
                    obj.SectorName = r["SectorName"].ToString();
                    obj.BlockName = r["BlockName"].ToString();
                    obj.PlotNumber = r["PlotNumber"].ToString();
                    obj.PlotArea = r["TotalArea"].ToString();
                    obj.PlotSize = r.Table.Columns.Contains("PlotSize") ? r["PlotSize"].ToString() : "";
                    obj.PlotRate = r["PlotRate"].ToString();
                    obj.PlotAmount = r["PlotAmount"].ToString();
                    obj.PLCCharge = r["PLCCharge"].ToString();
                    obj.BookingPercent = r["BookingPercent"].ToString();
                    obj.PLCNames = r["plcnames"].ToString();
                    obj.AllottmentPercent = (r["AllottmentPercent"].ToString());
                    obj.PlotStatus = (r["PlotStatus"].ToString());
                    obj.ColorCSS = (r["StatusColor"].ToString());
                    lst.Add(obj);
                }
                model.lstPlot = lst;
            }

            // Fetch Plot Stats for filters
            Reports report = new Reports();
            report.SiteID = model.SiteID;
            report.SectorID = model.SectorID;
            report.BlockID = model.BlockID;
            DataSet dsStats = report.GetPlotCount();
            if (dsStats != null && dsStats.Tables.Count > 0)
            {
                ViewBag.PlotStats = dsStats.Tables[0];
            }

            // Fetch Total Plots for the entire Site (Global Total)
            Reports reportSite = new Reports();
            reportSite.SiteID = model.SiteID;
            DataSet dsSiteStats = reportSite.GetPlotCount();
            if (dsSiteStats != null && dsSiteStats.Tables.Count > 0)
            {
                ViewBag.SiteStats = dsSiteStats.Tables[0];
            }

            #region ddlSite
            int count1 = 0;
            Master objmaster = new Master();
            List<SelectListItem> ddlSite = new List<SelectListItem>();
            DataSet dsSite = objmaster.GetSiteList();
            if (dsSite != null && dsSite.Tables.Count > 0 && dsSite.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in dsSite.Tables[0].Rows)
                {
                    if (count1 == 0)
                    {
                        ddlSite.Add(new SelectListItem { Text = "Select Site", Value = "0" });
                    }
                    ddlSite.Add(new SelectListItem { Text = r["SiteName"].ToString(), Value = r["PK_SiteID"].ToString() });
                    count1 = count1 + 1;

                }
            }
            ViewBag.ddlSite = ddlSite;
            #endregion

            #region GetSectors
            List<SelectListItem> ddlSector = new List<SelectListItem>();
            DataSet dsSector = objmaster.GetSectorList();
            int sectorcount = 0;

            if (dsSector != null && dsSector.Tables.Count > 0)
            {

                foreach (DataRow r in dsSector.Tables[0].Rows)
                {
                    if (sectorcount == 0)
                    {
                        ddlSector.Add(new SelectListItem { Text = "Select Sector", Value = "0" });
                    }
                    ddlSector.Add(new SelectListItem { Text = r["SectorName"].ToString(), Value = r["PK_SectorID"].ToString() });
                    sectorcount = 1;
                }
            }

            ViewBag.ddlSector = ddlSector;
            #endregion

            #region BlockList
            List<SelectListItem> lstBlock = new List<SelectListItem>();

            int blockcount = 0;
            objmaster.SiteID = model.SiteID == "0" ? null : model.SiteID;
            objmaster.SectorID = model.SectorID == "0" ? null : model.SectorID;
            DataSet dsblock = objmaster.GetBlockList();


            if (dsblock != null && dsblock.Tables.Count > 0 && dsblock.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow dr in dsblock.Tables[0].Rows)
                {
                    if (blockcount == 0)
                    {
                        lstBlock.Add(new SelectListItem { Text = "Select Block", Value = "0" });
                    }
                    lstBlock.Add(new SelectListItem { Text = dr["BlockName"].ToString(), Value = dr["PK_BlockID"].ToString() });
                    blockcount = 1;
                }

            }


            ViewBag.ddlBlock = lstBlock;
            #endregion
            return View(model);
        }

        public virtual PartialViewResult Menu()
        {
            Home Menu = null;
            

            if (Session["_Menu"] != null)
            {
                Menu = (Home)Session["_Menu"];
            }
            else
            {

                Menu = Home.GetMenus(Session["Pk_adminId"].ToString(), Session["RoleName"].ToString()); // pass employee id here
                Session["_Menu"] = Menu;
            }
            return PartialView("_Menu", Menu);
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AboutUs()
        {
            return View();
        }
        public ActionResult ContactUs()
        {
            return View();
        }
        public ActionResult Projects()
        {
            return View();
        }
        public ActionResult Gallery()
        {
            return View();
        }

        public ActionResult GetPassword(string LoginId)
        {
            Home obj = new Home();
            obj.LoginId = LoginId;
            DataSet ds = obj.GetPassword();
            if (ds != null)
            {
                if (ds.Tables[0].Rows[0]["Msg"].ToString() == "1")
                {
                   string msg = "Dear " + ds.Tables[0].Rows[0]["Name"].ToString() + " Your Password is " +Crypto.Decrypt(ds.Tables[0].Rows[0]["Password"].ToString()) + " Kindly login www.gardenestateventures.com Thankyou";
                    BLSMS.SendSMSNew(ds.Tables[0].Rows[0]["Mobile"].ToString(), msg);
                    obj.Result = "1";

                }
                else
                {
                    obj.Result = "0";
                }
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TermsCondition()
        {
            Home home = new Home();
            DataSet ds = home.GetTermsCondition();
            home.lstDetails = ds.Tables[0];
            return View(home);
        }

        [HttpGet]
        public JsonResult GetAssociateName(string loginId)
        {
            try
            {
                SqlParameter[] para = { new SqlParameter("@LoginId", loginId ?? "") };
                string query = "SELECT RTRIM(LTRIM(ISNULL(FirstName, '') + ' ' + ISNULL(LastName, ''))) AS Name FROM tblUserLogin WHERE LoginId = @LoginId";
                DataSet ds = Connection.ExecuteQueryText(query, para);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(ds.Tables[0].Rows[0]["Name"].ToString()))
                {
                    return Json(new { Result = "1", Name = ds.Tables[0].Rows[0]["Name"].ToString() }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Result = "0", Message = "Associate ID not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "0", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SavePlotHoldAjax(string PlotID, string SiteID, string SectorID, string BlockID, string PlotNumber, string AssociateID, string AssociateName, string CustomerName, string CustomerMobile, string Remark)
        {
            try
            {
                SqlParameter[] para = {
                    new SqlParameter("@Fk_PlotId", PlotID ?? "0"),
                    new SqlParameter("@FK_SiteID", SiteID ?? "0"),
                    new SqlParameter("@FK_SectorID", SectorID ?? "0"),
                    new SqlParameter("@FK_BlockID", BlockID ?? "0"),
                    new SqlParameter("@PlotNumber", PlotNumber ?? ""),
                    new SqlParameter("@AssociateID", AssociateID ?? ""),
                    new SqlParameter("@AssociateName", AssociateName ?? ""),
                    new SqlParameter("@CustomerName", CustomerName ?? ""),
                    new SqlParameter("@CustomerMobile", CustomerMobile ?? ""),
                    new SqlParameter("@Amount", 0),
                    new SqlParameter("@Remark", Remark ?? ""),
                    new SqlParameter("@AddedBy", Session["Pk_AdminId"] != null ? Session["Pk_AdminId"].ToString() : "1")
                };

                DataSet ds = Connection.ExecuteQuery("PlotHold", para);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["Msg"].ToString() == "1")
                    {
                        return Json(new { Result = "1", Message = "Hold Request for Plot " + PlotNumber + " submitted successfully! Sent to Admin for approval." });
                    }
                    else
                    {
                        string err = ds.Tables[0].Columns.Contains("ErrorMessage") ? ds.Tables[0].Rows[0]["ErrorMessage"].ToString() : "Could not process hold request.";
                        return Json(new { Result = "0", Message = err });
                    }
                }
                return Json(new { Result = "0", Message = "Failed to process hold request." });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "0", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SavePlotEnquiryAjax(string PlotNumber, string SiteName, string SectorName, string BlockName, string Name, string Mobile, string Message)
        {
            try
            {
                SqlParameter[] checkPara = {
                    new SqlParameter("@PlotNumber", PlotNumber ?? ""),
                    new SqlParameter("@Mobile", Mobile ?? "")
                };
                string checkQuery = "SELECT COUNT(1) FROM tblPlotEnquiry WHERE PlotNumber = @PlotNumber AND Mobile = @Mobile AND EnquiryDate > DATEADD(minute, -2, GETDATE())";
                DataSet dsCheck = Connection.ExecuteQueryText(checkQuery, checkPara);
                if (dsCheck != null && dsCheck.Tables.Count > 0 && dsCheck.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsCheck.Tables[0].Rows[0][0]) > 0)
                {
                    // Duplicate rapid click request within 2 minutes: return success response without re-inserting
                    string dupMsg = "Thank you " + Name + "! Your enquiry for Plot " + PlotNumber + " (" + SiteName + ") has been received. Our team will contact you at " + Mobile + " shortly.";
                    return Json(new { Result = "1", Message = dupMsg });
                }

                SqlParameter[] para = {
                    new SqlParameter("@PlotNumber", PlotNumber ?? ""),
                    new SqlParameter("@SiteName", SiteName ?? ""),
                    new SqlParameter("@SectorName", SectorName ?? ""),
                    new SqlParameter("@BlockName", BlockName ?? ""),
                    new SqlParameter("@Name", Name ?? ""),
                    new SqlParameter("@Mobile", Mobile ?? ""),
                    new SqlParameter("@Message", Message ?? "")
                };

                string query = "INSERT INTO tblPlotEnquiry (PlotNumber, SiteName, SectorName, BlockName, Name, Mobile, Message, EnquiryDate, Status) VALUES (@PlotNumber, @SiteName, @SectorName, @BlockName, @Name, @Mobile, @Message, GETDATE(), 'New')";
                Connection.ExecuteNonQueryText(query, para);

                string responseMsg = "Thank you " + Name + "! Your enquiry for Plot " + PlotNumber + " (" + SiteName + ") has been received. Our team will contact you at " + Mobile + " shortly.";
                return Json(new { Result = "1", Message = responseMsg });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "0", Message = ex.Message });
            }
        }

    }
}


