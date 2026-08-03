using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace RealEstateRegalSpace.Models
{
    public class SiteVisit : Common
    {
        public long PK_VisitID { get; set; }
        public string FK_AssociateID { get; set; }
        public string AssociateCode { get; set; }
        public string AssociateName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string SiteID { get; set; }
        public string SiteName { get; set; }
        public string BlockName { get; set; }
        public string PlotNumber { get; set; }
        public string VisitDate { get; set; }
        public string VehicleRequired { get; set; }
        public string PickupAddress { get; set; }
        public int NumberOfPersons { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public string AdminRemarks { get; set; }
        public string DriverDetails { get; set; }
        public string CreatedDate { get; set; }

        public static void EnsureTableExists()
        {
            try
            {
                string createSql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tblSiteVisit')
                BEGIN
                    CREATE TABLE tblSiteVisit (
                        PK_VisitID BIGINT IDENTITY(1,1) PRIMARY KEY,
                        FK_AssociateID BIGINT NULL,
                        AssociateCode NVARCHAR(50) NULL,
                        AssociateName NVARCHAR(100) NULL,
                        CustomerName NVARCHAR(100) NOT NULL,
                        CustomerMobile NVARCHAR(20) NOT NULL,
                        SiteID BIGINT NULL,
                        SiteName NVARCHAR(100) NULL,
                        BlockName NVARCHAR(100) NULL,
                        PlotNumber NVARCHAR(50) NULL,
                        VisitDate DATETIME NOT NULL,
                        VehicleRequired NVARCHAR(20) DEFAULT 'No',
                        PickupAddress NVARCHAR(255) NULL,
                        NumberOfPersons INT DEFAULT 1,
                        Remarks NVARCHAR(MAX) NULL,
                        Status NVARCHAR(50) DEFAULT 'Pending',
                        AdminRemarks NVARCHAR(MAX) NULL,
                        DriverDetails NVARCHAR(255) NULL,
                        CreatedDate DATETIME DEFAULT GETDATE()
                    );
                END";
                Connection.ExecuteNonQueryText(createSql);
            }
            catch { }
        }

        public DataSet SaveSiteVisit()
        {
            EnsureTableExists();
            string insertSql = @"
            INSERT INTO tblSiteVisit 
            (FK_AssociateID, AssociateCode, AssociateName, CustomerName, CustomerMobile, SiteID, SiteName, BlockName, PlotNumber, VisitDate, VehicleRequired, PickupAddress, NumberOfPersons, Remarks, Status, CreatedDate)
            VALUES
            (@FK_AssociateID, @AssociateCode, @AssociateName, @CustomerName, @CustomerMobile, @SiteID, @SiteName, @BlockName, @PlotNumber, @VisitDate, @VehicleRequired, @PickupAddress, @NumberOfPersons, @Remarks, 'Pending', GETDATE());
            SELECT 1 AS Result, 'Site visit request submitted successfully!' AS Message;";

            DateTime parsedVisitDate = DateTime.Now;
            DateTime.TryParse(VisitDate, out parsedVisitDate);

            SqlParameter[] para = {
                new SqlParameter("@FK_AssociateID", (object)FK_AssociateID ?? DBNull.Value),
                new SqlParameter("@AssociateCode", (object)AssociateCode ?? DBNull.Value),
                new SqlParameter("@AssociateName", (object)AssociateName ?? DBNull.Value),
                new SqlParameter("@CustomerName", (object)CustomerName ?? DBNull.Value),
                new SqlParameter("@CustomerMobile", (object)CustomerMobile ?? DBNull.Value),
                new SqlParameter("@SiteID", (object)SiteID ?? DBNull.Value),
                new SqlParameter("@SiteName", (object)SiteName ?? DBNull.Value),
                new SqlParameter("@BlockName", (object)BlockName ?? DBNull.Value),
                new SqlParameter("@PlotNumber", (object)PlotNumber ?? DBNull.Value),
                new SqlParameter("@VisitDate", parsedVisitDate),
                new SqlParameter("@VehicleRequired", (object)VehicleRequired ?? "No"),
                new SqlParameter("@PickupAddress", (object)PickupAddress ?? DBNull.Value),
                new SqlParameter("@NumberOfPersons", NumberOfPersons > 0 ? NumberOfPersons : 1),
                new SqlParameter("@Remarks", (object)Remarks ?? DBNull.Value)
            };

            return Connection.ExecuteQueryText(insertSql, para);
        }

        public static DataTable GetSiteVisits(string associateUserId = null, string status = null, string siteId = null)
        {
            EnsureTableExists();
            string sql = "SELECT PK_VisitID, FK_AssociateID, AssociateCode, AssociateName, CustomerName, CustomerMobile, SiteID, SiteName, BlockName, PlotNumber, VisitDate, VehicleRequired, PickupAddress, NumberOfPersons, Remarks, Status, AdminRemarks, DriverDetails, CreatedDate FROM tblSiteVisit WHERE 1=1";
            List<SqlParameter> paraList = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(associateUserId))
            {
                sql += " AND FK_AssociateID = @FK_AssociateID";
                paraList.Add(new SqlParameter("@FK_AssociateID", associateUserId));
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND Status = @Status";
                paraList.Add(new SqlParameter("@Status", status));
            }
            if (!string.IsNullOrEmpty(siteId))
            {
                sql += " AND SiteID = @SiteID";
                paraList.Add(new SqlParameter("@SiteID", siteId));
            }

            sql += " ORDER BY PK_VisitID DESC";

            DataSet ds = Connection.ExecuteQueryText(sql, paraList.ToArray());
            return (ds != null && ds.Tables.Count > 0) ? ds.Tables[0] : new DataTable();
        }

        public static int UpdateVisitStatus(long visitId, string status, string adminRemarks, string driverDetails)
        {
            EnsureTableExists();
            string updateSql = "UPDATE tblSiteVisit SET Status = @Status, AdminRemarks = @AdminRemarks, DriverDetails = @DriverDetails WHERE PK_VisitID = @VisitID";
            SqlParameter[] para = {
                new SqlParameter("@VisitID", visitId),
                new SqlParameter("@Status", (object)status ?? "Pending"),
                new SqlParameter("@AdminRemarks", (object)adminRemarks ?? DBNull.Value),
                new SqlParameter("@DriverDetails", (object)driverDetails ?? DBNull.Value)
            };
            return Connection.ExecuteNonQueryText(updateSql, para);
        }
    }
}
