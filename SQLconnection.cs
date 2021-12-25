using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbConnectionTools
{
    public class CSQL_Connection
    {
        public CSQL_Connection()
        {

        }
        /// <summary>
        /// Test connection 
        /// </summary>
        /// <returns></returns>
        public StringBuilder TestConnection()
        {

            StringBuilder sb = new StringBuilder();
            string ConnectionString = GetConnectionString();
            // Create connection string to the db
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    // Open connection
                    conn.Open();
                    sb.Append(DateTime.Now.ToString() + ":\tConnection Opened.\n");
                    sb.Append("\t\t\tConnection properties:\n");
                    sb.Append("\t\t\t\tDatabase: " + conn.Database.ToString() + "\n");
                    sb.Append("\t\t\t\tDatasource: " + conn.DataSource.ToString() + "\n");
                    sb.Append("\t\t\t\tWorkstation ID: " + conn.WorkstationId.ToString() + "\n");
                }
                catch (SqlException ex)
                {
                    // Display error if any
                    sb.Append(":\tError: " + ex.Message.ToString() + ex.StackTrace.ToString());
                }
                finally
                {
                    // Close connection regardless
                    conn.Close();
                    sb.Append(DateTime.Now.ToString() + ":\tConnection closed successfully.\n");
                }
            }
            return sb; 
        }
        // retrieve the filed concatenation from the table Documentation.ProjectField
        public DataTable LoadDocFieldPosition(int p_ProjectID)
        {
            string ConnectionString = GetConnectionString();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Query statement from tbQueryStat
                string sql = @"select FieldName, Position FROM dbo.ProjectField WHERE ProjectID = @ProjectID";
                // create command
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Injection safeguard
                    cmd.Parameters.Add("ProjectID", SqlDbType.Int);
                    cmd.Parameters["ProjectID"].Value = p_ProjectID;
                    DataTable dt = new DataTable();
                    try
                    {
                        conn.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        // create data Table
                        dt.Columns.Add("FieldName", typeof(string));
                        dt.Columns.Add("Position", typeof(byte));
                        dt.Load(rdr);
                    }
                    catch (SqlException ex)
                    {
                        // Display error if any
                        //tb.AppendText("Error: " + ex.Message.ToString() + ex.StackTrace.ToString());
                    }
                    finally
                    {
                        conn.Close();
                    }
                    return dt;
                }
            }
        }
        /// <summary>
        /// Get the validation on whteher the table exists within the DB
        /// </summary>
        /// <returns></returns>
        public bool Sp_CheckFieldTableExists()
        {
            string ConnectionString = GetConnectionString();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Create command with the stored procedure
                using (SqlCommand cmd = new SqlCommand("dbo.CheckTableExist", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    bool ReturnValueFromProcedure = false;
                    // output
                    cmd.Parameters.Add("OutValue", SqlDbType.Int).Direction = ParameterDirection.Output;
                    try
                    {
                        // Open connection
                        conn.Open();
                        // execute stored procedure as a Execute non query method 
                        cmd.ExecuteNonQuery();
                        ReturnValueFromProcedure = Convert.ToBoolean(cmd.Parameters["OutValue"].Value);
                    }
                    catch (SqlException ex)
                    {

                    }
                    finally
                    {
                        // Close connection regardless
                        conn.Close();
                    }
                    return ReturnValueFromProcedure;
                }
            }
        }
        /// <summary>
        /// Push the log into the DB from the revit environment
        /// </summary>
        /// <param name="p_UserID"></param>
        /// <param name="p_LogType"></param>
        /// <param name="p_ProjectID"></param>
        /// <param name="p_ModelName"></param>
        /// <param name="p_ViewName"></param>
        /// <param name="p_SheetName"></param>
        /// <param name="p_ViewType"></param>
        /// <returns></returns>
        public string Sp_InserRvtLogDetail(int p_UserID, string p_LogType, int p_ProjectID, string p_ModelName, string p_ViewName,
            string p_SheetName, string p_ViewType)
        {
            string ConnectionString = GetConnectionString();
            string Result = null;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Create command with the stored procedure
                using (SqlCommand cmd = new SqlCommand("Bim.Sp_InsertLogDetail1", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Associate all parameters of the stored procedure to a Parameter
                    cmd.Parameters.Add("UserID", SqlDbType.Int);
                    cmd.Parameters.Add("ProjectID", SqlDbType.VarChar, 50);
                    cmd.Parameters.Add("ModelName", SqlDbType.VarChar, 50);
                    cmd.Parameters.Add("LogTypeValue", SqlDbType.VarChar, 30);
                    cmd.Parameters.Add("ViewName", SqlDbType.VarChar, 200);
                    cmd.Parameters.Add("SheetName", SqlDbType.VarChar, 50);
                    cmd.Parameters.Add("ViewType", SqlDbType.VarChar, 50);

                    cmd.Parameters["UserID"].Value = p_UserID;
                    cmd.Parameters["ProjectID"].Value = p_ProjectID;
                    cmd.Parameters["ModelName"].Value = p_ModelName;
                    cmd.Parameters["LogTypeValue"].Value = p_LogType;
                    cmd.Parameters["ViewName"].Value = p_ViewName;
                    cmd.Parameters["ViewName"].IsNullable = true;
                    cmd.Parameters["SheetName"].Value = p_SheetName;
                    cmd.Parameters["SheetName"].IsNullable = true;
                    cmd.Parameters["ViewType"].Value = p_ViewType;
                    cmd.Parameters["ViewType"].IsNullable = true;

                    try
                    {
                        // Open connection
                        conn.Open();
                        // execute stored procedure as a Execute non query method 
                        cmd.ExecuteNonQuery();
                        Result = "SUCCESS";
                    }
                    catch (SqlException ex)
                    {

                    }
                    finally
                    {
                        // Close connection regardless
                        conn.Close();
                    }
                    return Result;
                }
            }
        }
        /// <summary>
        /// Get all the Rvt LIVE logs. No parameters
        /// </summary>
        /// <returns></returns>
        public DataTable Sp_GetCurrentUserActivityInRevit()
        {
            DataTable ReturnTable = null;
            string ConnectionString = GetConnectionString();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Create command with the stored procedure
                using (SqlCommand cmd = new SqlCommand("Bim.sp_GetAllUserActivities", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        // Open connection
                        conn.Open();
                        using (SqlDataAdapter daMethod = new SqlDataAdapter(cmd))
                        {
                            daMethod.SelectCommand.CommandType = CommandType.StoredProcedure;
                            DataSet dsMethod = new DataSet();
                            // fill the dataset
                            daMethod.Fill(dsMethod, "DataAdapter1");
                            ReturnTable = dsMethod.Tables["DataAdapter1"];
                        }
                    }
                    catch (SqlException ex)
                    {

                    }
                    finally
                    {
                        // Close connection regardless
                        conn.Close();
                    }
                    return ReturnTable;
                }
            }
        }
        #region PULL ConnectionString
        /// <summary>
        /// Get the DB connection string
        /// </summary>
        /// <returns></returns>
        private static string GetConnectionString()
        {
            string ConnectionString = @"Server=tcp:01-testing.database.windows.net,1433;Initial Catalog=MainFrame_Develop;Persist Security Info=False;User ID=AppLogin;Password=TempAppPWD1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            return ConnectionString;
        }
        #endregion PULL ConnectionString ENDS
    }

}