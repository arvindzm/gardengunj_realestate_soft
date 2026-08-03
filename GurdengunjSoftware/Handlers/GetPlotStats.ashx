<%@ WebHandler Language="C#" Class="GetPlotStats" %>

using System;
using System.Web;
using System.Data;
using System.Web.Script.Serialization;
using RealEstateRegalSpace.Models;

public class GetPlotStats : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "application/json";
        string siteId = context.Request.QueryString["siteId"];
        
        Reports report = new Reports();
        report.SiteID = string.IsNullOrEmpty(siteId) || siteId == "0" ? null : siteId;
        
        DataSet ds = report.GetPlotCount();
        
        var result = new {
            TotalPlots = 0,
            Available = 0,
            Booked = 0,
            OnHold = 0,
            Registered = 0,
            Cancelled = 0
        };

        if (ds != null && ds.Tables.Count > 0) {
            foreach (DataRow row in ds.Tables[0].Rows) {
                result = new {
                    TotalPlots = result.TotalPlots + (ds.Tables[0].Columns.Contains("TotalPlots") ? Convert.ToInt32(row["TotalPlots"]) : 0),
                    Available = result.Available + (ds.Tables[0].Columns.Contains("AvailablePlots") ? Convert.ToInt32(row["AvailablePlots"]) : 0),
                    Booked = result.Booked + (ds.Tables[0].Columns.Contains("BookedPlots") ? Convert.ToInt32(row["BookedPlots"]) : 0),
                    OnHold = result.OnHold + (ds.Tables[0].Columns.Contains("HoldPlots") ? Convert.ToInt32(row["HoldPlots"]) : 0),
                    Registered = result.Registered + (ds.Tables[0].Columns.Contains("RegisteredPlots") ? Convert.ToInt32(row["RegisteredPlots"]) : 0),
                    Cancelled = result.Cancelled + (ds.Tables[0].Columns.Contains("CancelledPlots") ? Convert.ToInt32(row["CancelledPlots"]) : 0)
                };
            }
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        context.Response.Write(serializer.Serialize(result));
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}
