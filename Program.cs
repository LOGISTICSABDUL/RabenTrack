using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Collections;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Http;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Threading.Tasks;

namespace RabenTrack
{
    class Program
    {
        public static string connectionstring = "";
        public static string DownloadCompleted = "";
        public static string FTPDownload = "";
        public static string Plant_Id = "";
        public static string strHost="" ;
        public static string strFTPUserName="" ;
        
        public static string strFTPPassword ="";
        public static string strUTIFTPPath="" ;
        public static string SFTP_File_DownLoad_Location = "";
        public static SqlConnection con = null;
        public static string AfterRead_File_Save_location = "";

        public Program()
        {
             connectionstring = ConfigurationManager.ConnectionStrings["xCarrier_Connection"].ToString();
             strHost = ConfigurationManager.AppSettings["Host"];
             strFTPUserName = ConfigurationManager.AppSettings["UserName"];
             strFTPPassword = ConfigurationManager.AppSettings["password"];
             strUTIFTPPath =  ConfigurationManager.AppSettings["path"];
             SFTP_File_DownLoad_Location = ConfigurationManager.AppSettings["SFTP_File_DownLoad_Location"];
             AfterRead_File_Save_location= ConfigurationManager.AppSettings["AfterRead_File_Save_location"];
             con = new SqlConnection(connectionstring);

        }
        
        static void Main(string[] args)
        {
            new Program();

            FTPDownload = "True";
           
            List<String> files = new List<String>();

            // string BackupCompletePath = @"D:\ProcessWeaver\Raben\RabenProcessed\";
          // SFTP_File_DownLoad_Location = @"D:\RabenTrackFiles3\";
            //SFTP_File_DownLoad_Location = @"D:\Customers\Inno\Flash\TrackFiles\out\";
            //try
            //{
            //    SftpClient objSFTP = new SftpClient(strHost, strFTPUserName, strFTPPassword);
            //    objSFTP.Connect();
            //    Console.WriteLine("sftp Connected");
            //    var files22 = objSFTP.ListDirectory(strUTIFTPPath);

            //    foreach (var ff in files22)
            //    {
            //        string remotefilename = ff.Name;
            //        FileInfo fi = new FileInfo(remotefilename);

            //        if (ff.Name == "." || (ff.Name == ".."))
            //        {

            //        }
            //        else
            //        {

            //            try
            //            {

            //                using (Stream file1 = File.OpenWrite(SFTP_File_DownLoad_Location + remotefilename))
            //                {
            //                    objSFTP.DownloadFile(strUTIFTPPath + remotefilename, file1);
            //                }

            //            }
            //            catch (Exception ex) { }

            //            DownloadCompleted = "YES";
            //            Console.WriteLine("In donwload progress....");
            //        }

            //    }



            //}
            //catch (Exception ex)
            //{
            //    addloginfo("SFTP Connection Error:" + ex.Message.ToString());
            //}


            //if (DownloadCompleted.ToUpper() == "YES")
            //{
            //    Console.WriteLine("in DownloadCompleted text files");
            //    try
            //    {
            //        DeleteFile(strHost, strFTPUserName, strFTPPassword, strUTIFTPPath);

            //    }
            //    catch { }
            //}

            //outputiflepath = @"D:\RabenTrackFiles\";

            try
            {

                //System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(outputiflepath);
                //System.IO.FileInfo[] fileNames = dirInfo.GetFiles("*.pdf*");

                //foreach (System.IO.FileInfo fi in fileNames)
                //{
                //    try
                //    {
                //        string strFileName = fi.Name;
                //        string[] FileArray = strFileName.Split(new Char[] { '-', '_' });
                //        string NewFileName = FileArray[2].ToString();

                //        System.IO.File.Move(fi.FullName, LabelPath +"\\"+ NewFileName + ".pdf");

                //    }
                //    catch(Exception ex)
                //    {

                //    }
                //   // files.Add(fi.FullName.ToString());
                //}

                SFTP_File_DownLoad_Location = @"D:\Customers\Inno\Flash\TrackFiles\out\";

                //SFTP_File_DownLoad_Location = @"D:\Customers\async\";
                foreach (string f in Directory.GetFiles(SFTP_File_DownLoad_Location))
                {
                    files.Add(f);
                }

                for (int f = 0; f < files.Count; f++)
                {
                    var lines = File.ReadLines(files[f]);
                    
                    System.Threading.Thread.Sleep(100);
                    //addloginfo(files[f].ToString());
                    try
                    {
                        foreach (var Line in lines)
                        {
                            string Date = "", Time = "";
                            string PODStatus = "", CustReference = "", Delivery_num = "", StatusCode = "", TrackingNumber = ""; ;
                            string StatusCode1 = "";

                            if (Line.Contains("@@PH"))
                                continue;
                            if (Line.Contains("Q00"))
                                continue;
                            if (Line.Contains("Q30"))
                                continue;
                            if (Line.Contains("Q20"))
                                continue;
                            if (Line.Contains("Z00"))
                                continue;
                            if (Line.Contains("@@PT"))
                                continue;

                            Delivery_num = Line.Substring(3, 35).ToString().Trim();

                            StatusCode1 = Line.Substring(108, 3).ToString().Trim();
                            CustReference = Line.Substring(38, 13).ToString();

                            Date = Line.Substring(111, 8).ToString().Trim();
                            Time = Line.Substring(119, 4).ToString().Trim();

                            string DateTime = "";
                            try
                            {
                                string Day = Date.Substring(0, 2);
                                string month = Date.Substring(2, 2);
                                string Year = Date.Substring(4, 4);


                                string Hour = Time.Substring(0, 2);
                                string min = Time.Substring(2, 2);


                                DateTime = Year + "-" + month + "-" + Day + " " + Hour + ":" + min;
                            }
                            catch (Exception ex) { }
                            StatusCode = Raben_DESCRIPTION(StatusCode1);
                            string shippingnum = "";

                            // string carrier = "RABEN";
                            if (StatusCode == "DELIVERED")
                                PODStatus = "DELIVERED";
                            else
                                PODStatus = StatusCode;


                            Plant_Id = GetPlantId(Delivery_num);

                            try
                            {
                                string querycus = "SELECT Tracking_number,SHIPPING_NUM,Plant_ID FROM xcarrier_shipments WHERE DELIVERY_NUM='" + Delivery_num + "' and Plant_ID='" + Plant_Id + "' and CARRIER_DESCRIPTION='Raben' and status_code NOT IN ('CAN','OHD','OPN','OPEN','SPD','MFD','RTN') and  TRACKING_NUMBER<>'' and TRACKING_NUMBER<>'0'";
                                DataSet dscr = new DataSet();
                                SqlDataAdapter sqldcr = new SqlDataAdapter(querycus, con);
                                sqldcr.Fill(dscr);
                                try
                                {
                                    TrackingNumber = dscr.Tables[0].Rows[0]["Tracking_number"].ToString();
                                    shippingnum = dscr.Tables[0].Rows[0]["SHIPPING_NUM"].ToString();
                                   
                                }
                                catch (Exception ex)
                                {

                                }

                            }
                            catch (Exception ex)
                            {

                            }

                            if (shippingnum != "")
                            {

                                try
                                {

                                    XCARRIERUPDATE(con, DateTime, "", removespecialcharacters(PODStatus), "", TrackingNumber, shippingnum, StatusCode, "", TrackingNumber, StatusCode1, "RABEN", CustReference, files[f], Plant_Id);

                                        //Task task = ExecuteSqlTransaction(connectionstring, DateTime, "", removespecialcharacters(PODStatus), "", TrackingNumber, shippingnum, StatusCode, "", TrackingNumber, StatusCode1, "RABEN", CustReference, files[f], Plant_Id);
                                        //task.Wait();
                                }
                                catch (Exception ex) { }
                            }
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }

                    try
                    {

                        if (File.Exists(files[f]))
                        {
                            movefile_new(files[f], AfterRead_File_Save_location);
                            Console.WriteLine("file moved");

                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
                



            }
            catch (Exception ex)
            {
                addloginfo(ex.Message);
            }

        }
        public static void DeleteFile(string strHost, string strFTPUserName, string strFTPPassword, string strUTIFTPPath)
        {
            try
            {
                SftpClient objSFTP = new SftpClient(strHost, strFTPUserName, strFTPPassword);
                objSFTP.Connect();
                var files22 = objSFTP.ListDirectory(strUTIFTPPath);

                foreach (var ff in files22)
                {
                    string remotefilename = ff.Name;
                    if (ff.Name == "." || (ff.Name == ".."))
                    {
                    }
                    else
                    {
                        try
                        {
                            objSFTP.DeleteFile(strUTIFTPPath + remotefilename);
                        }
                        catch (Exception ex)
                        {
                           
                        }
                        Console.WriteLine("FTP Files are deleted");
                    }

                }

            }
            catch (Exception ex)
            {

            }
        }
        public static string GetPlantId(string PrimaryRefNumber)
        {
            string Plant_id = "";
            try
            {
                SqlConnection con = null;
                SqlCommand cmd = null;
                con = new SqlConnection(connectionstring);

                string strquery = "Select Plant_ID from xcarrier_shipments Where Delivery_Num = '" + PrimaryRefNumber + "'";
                cmd = new SqlCommand(strquery, con);
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Plant_id = dr["Plant_ID"].ToString();

                }

                dr.Close();
                con.Close();
            }
            catch (Exception er)
            {

            }
            return Plant_id;
        }

        static async Task ExecuteSqlTransaction(string connectionString, string PODDateTime, string PODSignature, string PODStatus, string PODLocation, string TrackingNo, string ShippingNo, string PODStatusCode, string plantID, string MastertrackingNo, string statuscode, string carrier, string Deliverynumber, string FileName, string Plant_Id)
        {
                                        
            string strInsertQuery = "", strUpdateQuery="";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction = null;

                // Start a local transaction.
                transaction = await Task.Run<SqlTransaction>(
                    () => connection.BeginTransaction("xCarrierTransaction")
                    );

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    string _strquery_visibility = "select POD_STATUS from XCARRIER_SHIPPING_VISIBILITY  WITH (NOLOCK) where  Tracking_num='" + TrackingNo + "' and  POD_STATUS='" + PODStatus.ToUpper() + "'";
                    string _strquery_shipments = "Select POD_STATUS,tracking_number from XCARRIER_SHIPMENTS  WITH (NOLOCK) Where  tracking_number='" + TrackingNo + "' and POD_STATUS='DELIVERED'";
                    string _strquery_Package_Master = "select PODSTATUS FROM XCarrier_Package_Master WITH (NOLOCK) where  Tracking_num='" + TrackingNo + "' and PODSTATUS='DELIVERED'";
                    SqlDataReader shpimentReader = null;
                    SqlDataReader packageReader = null;

                    using (SqlConnection awConnection = new SqlConnection(connectionString))
                    {
                        SqlCommand visibilityCmd = new SqlCommand(_strquery_visibility, awConnection);
                        SqlCommand shipmentCmd = new SqlCommand(_strquery_shipments, awConnection);
                        SqlCommand packageCmd = new SqlCommand(_strquery_shipments, awConnection);
                       
                        await awConnection.OpenAsync();
                        using (SqlDataReader visiblityReader = await visibilityCmd.ExecuteReaderAsync())
                        {
                                await visiblityReader.ReadAsync();
                             
                                if (visiblityReader.HasRows==false)
                                {
                                    strInsertQuery = "insert into [XCARRIER_SHIPPING_VISIBILITY] (shipping_num,POD_DATETIME,POD_SIGN,POD_STATUS,Tracking_num,StatusCode,Carrier,CURRENT_PLACE,NOTES) values('" + ShippingNo + "','" + PODDateTime + "','" + PODSignature + "','" + PODStatus.ToUpper() + "','" + TrackingNo + "','" + statuscode + "','" + carrier + "','" + PODLocation + "','" + FileName + "');";
                                    command.CommandText = strInsertQuery;
                                    await command.ExecuteNonQueryAsync();
                                }
                           

                            shpimentReader = await shipmentCmd.ExecuteReaderAsync();
                            using (shipmentCmd)
                            {
                                    await shpimentReader.ReadAsync();
                                
                                    if (shpimentReader.HasRows == false)
                                    {
                                        if (PODStatus.ToUpper() == "DELIVERED" && (TrackingNo != null || TrackingNo != ""))
                                        {
                                            strUpdateQuery = "update xcarrier_shipments set POD_DATETIME='" + PODDateTime + "',POD_SIGNATURE='" + PODSignature + "', POD_STATUS='" + PODStatus.ToUpper() + "',SHIPMENT_STATUS='" + PODStatus.ToUpper() + "', TRACKING_NOTE='" + PODStatus + "',STATUS_CODE='DEL' where tracking_number='" + TrackingNo + "' and Plant_Id='" + Plant_Id + "' and CARRIER_DESCRIPTION='" + carrier + "'";
                                            command.CommandText = strUpdateQuery;
                                            await command.ExecuteNonQueryAsync();
                                        }
                                        else if (PODStatus.ToUpper().Contains("CANCEL"))
                                        {
                                            strUpdateQuery = "update xcarrier_shipments set SHIPMENT_STATUS='" + PODStatus.ToUpper() + "', TRACKING_NOTE='" + PODStatus + "',STATUS_CODE='CAN' where tracking_number='" + TrackingNo + "'";
                                            command.CommandText = strUpdateQuery;
                                        }
                                        else
                                        {
                                            if (PODStatus.Contains("EXCEPTION"))
                                            {
                                                if (TrackingNo != null || TrackingNo != "")
                                                {
                                                    strUpdateQuery = "update xcarrier_shipments set SHIPMENT_STATUS='Exception',STATUS_CODE='EXC' where tracking_number='" + TrackingNo + "' and plant_id='" + Plant_Id + "' and CARRIER_DESCRIPTION='" + carrier + "'";
                                                    command.CommandText = strUpdateQuery;
                                                    await command.ExecuteNonQueryAsync();
                                                }
                                            }
                                            else
                                            {
                                                if (TrackingNo != null || TrackingNo != "")
                                                {
                                                    strUpdateQuery = "update xcarrier_shipments set SHIPMENT_STATUS='In-Transit', TRACKING_NOTE='" + PODStatus + "',STATUS_CODE='INT' where tracking_number='" + TrackingNo + "'and Plant_Id='" + Plant_Id + "' and CARRIER_DESCRIPTION='" + carrier + "'";
                                                    command.CommandText = strUpdateQuery;
                                                    await command.ExecuteNonQueryAsync();
                                                }
                                            }
                                            
                                        }
                                    }
                                
                            }
                            packageReader = await packageCmd.ExecuteReaderAsync();
                            using (packageCmd)
                            {
                                   await packageReader.ReadAsync();
                                
                                    if (packageReader.HasRows==false)
                                    {

                                    }
                                
                            }

                        }
                        //command.CommandText = ;
                    }
                    // Attempt to commit the transaction.
                    await Task.Run(() => transaction.Commit());
                    //Console.WriteLine("Both records are written to database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                    }
                }
            }
        }
        public static void XCARRIERUPDATE(SqlConnection con, string PODDateTime, string PODSignature, string PODStatus, string PODLocation, string TrackingNo, string ShippingNo, string PODStatusCode, string plantID, string MastertrackingNo, string statuscode, string carrier, string Deliverynumber,string FileName,string Plant_Id)
        {
            string strInsertQuery = "";
            string strUpdateQuery = "", strUpdateQuery1 = "";
            DataSet dsupt = new DataSet();
            try
            {
                string _strquery = "select POD_STATUS from XCARRIER_SHIPPING_VISIBILITY where Tracking_num='" + TrackingNo + "' and  POD_STATUS='" + PODStatus.ToUpper() + "' and  Carrier='" + carrier + "'";
                _strquery = _strquery + ";" + "Select POD_STATUS,tracking_number from xcarrier_shipments Where tracking_number='" + TrackingNo + "' and POD_STATUS='DELIVERED'  and  CARRIER_DESCRIPTION='" + carrier + "' and Plant_Id='" + Plant_Id + "'";

                DataSet dsquery = new DataSet();
               // addquerysql(_strquery);
                try
                {
                    SqlDataAdapter dt1 = new SqlDataAdapter(_strquery, con);
                    dt1.Fill(dsquery);
                }
                catch (Exception ex)
                { }

                try
                {
                    if (dsquery.Tables[0].Rows.Count > 0)
                    {

                    }
                    else
                    {
                        strInsertQuery = "insert into [XCARRIER_SHIPPING_VISIBILITY] (shipping_num,POD_DATETIME,POD_SIGN,POD_STATUS,Tracking_num,StatusCode,Carrier,CURRENT_PLACE,NOTES) values('" + ShippingNo + "','" + PODDateTime + "','" + PODSignature + "','" + PODStatus.ToUpper() + "','" + TrackingNo + "','" + statuscode + "','" + carrier + "','" + PODLocation + "','"+FileName+"');";
                    }


                    if (dsquery.Tables[1].Rows.Count > 0)
                    {

                    }
                    else
                    {

                        if (PODStatus.ToUpper() == "DELIVERED" && (TrackingNo != null || TrackingNo != ""))
                        {
                            strUpdateQuery = "update xcarrier_shipments set POD_DATETIME='" + PODDateTime + "',POD_SIGNATURE='" + PODSignature + "', POD_STATUS='" + PODStatus.ToUpper() + "',SHIPMENT_STATUS='" + PODStatus.ToUpper() + "', TRACKING_NOTE='" + PODStatus + "',STATUS_CODE='DEL' where tracking_number='" + TrackingNo + "' and Plant_Id='" + Plant_Id + "' and CARRIER_DESCRIPTION='"+ carrier + "'";  

                        }
                        else if (PODStatus.ToUpper().Contains("CANCEL"))
                        {
                            strUpdateQuery = "update xcarrier_shipments set SHIPMENT_STATUS='" + PODStatus.ToUpper() + "', TRACKING_NOTE='" + PODStatus + "',STATUS_CODE='CAN' where tracking_number='" + TrackingNo + "'";
                        }
                        else
                        {
                            if (PODStatus.Contains("EXCEPTION"))
                            {
                                if (TrackingNo != null || TrackingNo != "")
                                {
                                    strUpdateQuery = "update xcarrier_shipments set SHIPMENT_STATUS='Exception',STATUS_CODE='EXC' where tracking_number='" + TrackingNo + "' and plant_id='" + Plant_Id + "' and CARRIER_DESCRIPTION='" + carrier + "'";
                                }
                            }
                            else
                            {
                                if (TrackingNo != null || TrackingNo != "")
                                {
                                    strUpdateQuery = "update xcarrier_shipments set SHIPMENT_STATUS='In-Transit', TRACKING_NOTE='" + PODStatus + "',STATUS_CODE='INT' where tracking_number='" + TrackingNo + "'and Plant_Id='" + Plant_Id + "' and CARRIER_DESCRIPTION='" + carrier + "'";
                                }
                            }
                            //addquerysqlUP("shipments_UPDATE" + strUpdateQuery);
                        }

                    }


                    strInsertQuery = strInsertQuery + strUpdateQuery + strUpdateQuery1;

                    System.Threading.Thread.Sleep(100);
                    if (strInsertQuery != "")
                    {
                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        SqlCommand SqlCmd = new SqlCommand(strInsertQuery, con);
                        SqlCmd.ExecuteNonQuery();
                        if (con.State == ConnectionState.Open)
                            con.Close();
                    }
                }
                catch (Exception ex)
                {
                   // addquerysqlUP("sql error:"+ex.Message.ToString()+"tracknum:"+TrackingNo);
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                }
            }
            catch (Exception ex) { }
        }

        public static string removespecialcharacters(string strvalue)
        {
            string sshipto = "";
            try
            {

                sshipto = strvalue;
                sshipto = sshipto.Replace(" ", "");
                sshipto = sshipto.Replace("?", "");
                sshipto = sshipto.Replace("&", "");
                sshipto = sshipto.Replace("<", "");
                sshipto = sshipto.Replace(">", "");
                sshipto = sshipto.Replace("\"", "");
                sshipto = sshipto.Replace("'", "");
                sshipto = sshipto.Replace("@", "");
                sshipto = sshipto.Replace("!", "");
                sshipto = sshipto.Replace("%", "");
                sshipto = sshipto.Replace("*", "");
                sshipto = sshipto.Replace("^", "");
                sshipto = sshipto.Replace("#", "");
                sshipto = sshipto.Replace("#", "");
                sshipto = sshipto.Replace("á", "a");
                sshipto = sshipto.Replace("é", "e");
                sshipto = sshipto.Replace("ì", "i");
                sshipto = sshipto.Replace("ó", "o");
                sshipto = sshipto.Replace("ù", "u");
                sshipto = sshipto.Replace("Ä", "A");
                sshipto = sshipto.Replace("Ë", "E");

                sshipto = sshipto.Replace("Ï", "I");
                sshipto = sshipto.Replace("Ö", "O");
                sshipto = sshipto.Replace("Ü", "U");
                sshipto = sshipto.Replace("�", "");
                //return sshipto;
            }
            catch (Exception ex)
            {

            }
            return sshipto;
        }
        private static String Raben_DESCRIPTION(String StatusCode)
        {
            string Status = "";
            if (StatusCode == "000")
            {
                Status = "No difference in entrance -EXCEPTION";
                return Status;
            }
            if (StatusCode == "001")
            {
                Status = "Missing completely in the receipt (per shipment only 1x possible) -EXCEPTION";
                return Status;
            }
            if (StatusCode == "002")
            {
                Status = "Partially missing in the receipt (additional information on the number and type of packaging)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "003")
            {
                Status = "Damaged in the entrance (additional information number and type of packaging and damage text)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "004")
            {
                Status = "Packaging differences (additional information on number and packaging)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "005")
            {
                Status = "welded pallet; no guarantee for content, since it cannot be checked(only applies to non - scannable pallets)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "006")
            {
                Status = "Quantity difference bordero/unloading (recording error)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "007")
            {
                Status = "Shipment-related additional information (if not status 01 -06)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "060")
            {
                Status = "No status from EB until the time limit-EXCEPTION";
                return Status;
            }
            if (StatusCode == "050")
            {
                Status = "EB-ST No difference in input-EXCEPTION";
                return Status;
            }
            if (StatusCode == "051")
            {
                Status = "EB-ST is completely missing in the inbox (only 1x possible per shipment)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "052")
            {
                Status = "EB-ST Partially missing in the receipt (additional information on the number and type of packaging)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "053")
            {
                Status = "EB-ST damaged in the entrance (additional information number and type of packaging and damage text)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "054")
            {
                Status = "Stopped at the HUB-EXCEPTION";
                return Status;
            }
            if (StatusCode == "055")
            {
                Status = "loaded from HUB-EXCEPTION";
                return Status;
            }
            if (StatusCode == "054")
            {
                Status = "EB-ST packaging differences (additional information on the number and Packaging)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "055")
            {
                Status = "EB-ST welded pallet; no guarantee for content, because not verifiable(applies only to non - scannable pallets)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "056")
            {
                Status = "EB-ST quantity difference bordero/discharge (detection error)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "057")
            {
                Status = "EB-ST shipment-related additional information (if not states 01 - 06)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "060")
            {       
                Status = "No status from EB until the time limit-EXCEPTION";
                return Status;
            }
            if (StatusCode == "065")
            {
                Status = "Gateway shipment delayed in receipt (E2)-EXCEPTION";
                return Status;

            }
            if (StatusCode == "066")
            {
                Status = "Gateway shipment, unloading after unloading report generation-EXCEPTION";
                return Status;

            }
            if (StatusCode == "067")
            {
                Status = "stopped in the HUB PRIO2-EXCEPTION";
                return Status;
            }

            if (StatusCode == "068")
            {
                Status = "preloaded in the HUB-EXCEPTION";
                return Status;
            }
            if (StatusCode == "069")
            {
                Status = "stopped in the HUB-EXCEPTION";
                return Status;
            }
            if (StatusCode == "070")
            {
                Status = "stopped in the HUB-EXCEPTION";
                return Status;
            }
            if (StatusCode == "100")
            {
                Status = "Not in delivery - shipment is completely missing according to EB-EXCEPTION";
                return Status;
            }
            if (StatusCode == "101")
            {
                Status = "wrong shipment parameters-EXCEPTION";
                return Status;
            }
            if (StatusCode == "102")
            {
                Status = "Not out for delivery yet - goods damaged-EXCEPTION";
                return Status;
            }
            if (StatusCode == "103")
            {
                Status = "Not in delivery - shipment being researched-EXCEPTION";
                return Status;
            }
            if (StatusCode == "104")
            {
                Status = "Delivery depot transport department mistake, tail-lift required etc-EXCEPTION";
                return Status;
            }
            if (StatusCode == "105")
            {
                Status = "Not out for delivery yet - shipment's waiting for fixed del date as per instr.-EXCEPTION";
                return Status;
            }
            if (StatusCode == "106")
            {
                Status = "Not in delivery - recipient outside of production area-EXCEPTION";
                return Status;
            }
            if (StatusCode == "107")
            {
                Status = "Shipment damaged/incomplete in delivery depot (ECL)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "108")
            {
                Status = "Not in delivery - recipient is picking up himself-EXCEPTION";
                return Status;
            }
            if (StatusCode == "109")
            {
                Status = "Not out for delivery yet - regional holiday-EXCEPTION";
                return Status;
            }
            if (StatusCode == "110")
            {
                Status = "not delivered - custom clearance-EXCEPTION";
                return Status;
            }
            if (StatusCode == "111")
            {
                Status = "Not out for delivery yet -advice/notification necessary according to EDI-EXCEPTION";
                return Status;
            }
            if (StatusCode == "112")
            {
                Status = "ADV: consignee not responding the phone-EXCEPTION";
                return Status;
            }
            if (StatusCode == "113")
            {
                Status = "Not in delivery - notification for collection-EXCEPTION";
                return Status;
            }
            if (StatusCode == "114")
            {
                Status = "Lack of documents-EXCEPTION";
                return Status;
            }
            if (StatusCode == "115")
            {
                Status = "Shuttle late to the delivery depot (ECL) from transshipent depot(TCL))-EXCEPTION";
                return Status;
            }
            if (StatusCode == "116")
            {

                Status = "Consignment planned for later delivery tour-EXCEPTION";
                return Status;
            }
            if (StatusCode == "117")
            {
                Status = "Loaded on delivery trip (type LDT)";
                return Status;
            }
            if (StatusCode == "118")
            {
                Status = "Not in delivery - no acceptance of goods or agreement-EXCEPTION";
                return Status;
            }
            if (StatusCode == "119")
            {
                Status = "Not out for delivery yet - delivery has to be agreed upon with consignee-EXCEPTION";
                return Status;
            }
            if (StatusCode == "124")
            {
                Status = "Not delivered - new date set up by Consignee/Customer-EXCEPTION";
                return Status;
               
            }
            if (StatusCode == "126")
            {
                Status = "ETA - Estimated time of arrival-EXCEPTION";
                return Status;
            }
            if (StatusCode == "131")
            {
                Status = "Consignment planned for later delivery tour-EXCEPTION";
                return Status;
            }
            if (StatusCode == "150")
            {
                Status = "Shuttle late to delivery depot (ECL)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "190")
            {
                Status = "Shipment is on the way to the receiving partner-EXCEPTION";
                return Status;
            }
            if (StatusCode == "200")
            {
                Status = "Too many points of delivery-EXCEPTION";
                return Status;
            }
            if (StatusCode == "201")
            {
                Status = "Undelivered - Recipient not found. notification of delivery leave behind-EXCEPTION";
                return Status;
            }
            if (StatusCode == "202")
            {
                Status = "Not delivered - consignee refused delivery, deadline exceeded, shortage, damage-EXCEPTION";
                return Status;
            }
            if (StatusCode == "203")
            {
                Status = "Not out for delivery yet - consignment misplaced/cannot be found-EXCEPTION";
                return Status;
            }
            if (StatusCode == "300")
            {
                Status = "DELIVERED";
                return Status;
            }
            if (StatusCode == "301")
            {
                Status = "Driver has been held during delivery-EXCEPTION";
                return Status;
            }
            if (StatusCode == "302")
            {
                Status = "Not delivered - no receipt of goods or receipt of goods closed-EXCEPTION";
                return Status;
            }
            if (StatusCode == "303")
            {
                Status = "Not delivered - lift required-EXCEPTION";
                return Status;
            }
            if (StatusCode == "304")
            {
                Status = "POD-shipment delivered but uncompleted-EXCEPTION";
                return Status;
            }
            if (StatusCode == "305")
            {
                Status = "POD-damage-EXCEPTION";
                return Status;
            }
            if (StatusCode == "308")
            {
                Status = "POD - partially return-EXCEPTION";
                return Status;
            }
            if (StatusCode == "309")
            {
                Status = "AV goods damaged-EXCEPTION";
                return Status;
            }
            if (StatusCode == "310")
            {
                Status = "AV error amount-EXCEPTION";
                return Status;
            }
            if (StatusCode == "311")
            {
                Status = "Consignment refused - delivery deadline exceeded-EXCEPTION";
                return Status;
            }
            if (StatusCode == "312")
            {
                Status = "Consignment refused - delivery note missing/accompanying paperwork incomplete-EXCEPTION";
                return Status;
            }
            if (StatusCode == "313")
            {
                Status = "AV receiver pays WWNN/EUST. not-EXCEPTION";
                return Status;
            }
            if (StatusCode == "314")
            {
                Status = "AV- not ordered-EXCEPTION";
                return Status;
            }
            if (StatusCode == "315")
            {
                Status = "Completed by -EXCEPTION";
                return Status;
            }
            if (StatusCode == "316")
            {
                Status = "POD - shortages, damage-EXCEPTION";
                return Status;
            }
            if (StatusCode == "317")
            {
                Status = "Consignment refused - shortage and damage-EXCEPTION";
                return Status;
            }
            if (StatusCode == "400")
            {
                Status = "Delivery depot (ECL) warehouse mistake-EXCEPTION";
                return Status;
            }

            if (StatusCode == "401")
            {
                Status = "Force majeure / bad road condition-EXCEPTION";
                return Status;
            }
            if (StatusCode == "402")
            {
                Status = "No delivery - consignment missing as indicated in unloading report-EXCEPTION";
                return Status;
            }
            if (StatusCode == "403")
            {
                Status = "Not delivered - shipment refused, return according to disposal-EXCEPTION";
                return Status;
            }
            if (StatusCode == "404")
            {
                Status = "Not delivered - refused shipment, return available-EXCEPTION";
                return Status;
            }
            
            if (StatusCode == "406")
            {
                Status = "scaned doc.- POD archived";
                return Status;
            }
            if (StatusCode == "408")
            {
                Status = "Digital photo archived-EXCEPTION";
                return Status;
            }
            if (StatusCode == "410")
            {
                Status = "Digital photo archived Central notification recipient-EXCEPTION";
                return Status;
            }
            if (StatusCode == "411")
            {
                Status = "Delivery note archived via PDF-EXCEPTION";
                return Status;
            }
            if (StatusCode == "430")
            {
                Status = "Direct shipment, no discharge-EXCEPTION";
                return Status;
            }
            if (StatusCode == "490")
            {
                Status = "Collection order recorded-EXCEPTION";
                return Status;
            }
            if (StatusCode == "491")
            {
                Status = "Collection order received via EDI-EXCEPTION";
                return Status;
            }
            if (StatusCode == "492")
            {
                Status = "Transmission of collection order to subsystem-EXCEPTION";
                return Status;
            }
            if (StatusCode == "493")
            {
                Status = "Transfer pick-up order to carrier-EXCEPTION";
                return Status;
            }
            if (StatusCode == "494")
            {
                Status = "On the way to the sender-EXCEPTION";
                return Status;
            }
            if (StatusCode == "501")
            {
                Status = "PICK UP-collected-EXCEPTION";
                return Status;
            }
            if (StatusCode == "506")
            {
                Status = "PICK UP loaded on shuttle (trip type LLH)";
                return Status;
            }

            if (StatusCode == "520")
            {
                Status = "Unsuccessful pickup attempt - shipment not received, there company holidays-EXCEPTION";
                return Status;
            }
            if (StatusCode == "521")
            {
                Status = "Shipment didn't collect -customer responsibility -receiver not available-EXCEPTION";
                return Status;
            }
            if (StatusCode == "522")
            {
                Status = "Shipment didn't collect - Customer responsibility - wrong parameters, address-EXCEPTION";
                return Status;
            }
            if (StatusCode == "523")
            {
                Status = "Unsuccessful pickup attempt - shipment not received, there Incorrect pick-up address-EXCEPTION";
                return Status;
            }
            if (StatusCode == "524")
            {
                Status = "Unsuccessful pickup attempt - shipment not received because customer Appointment request-EXCEPTION";
                return Status;
            }
            if (StatusCode == "525")
            {
                Status = "Unsuccessful pickup attempt - shipment not received, there Original documents must be submitted-EXCEPTION";
                return Status;
            }
            if (StatusCode == "526")
            {
                Status = "Shipment didn't collect -customer responsibility -shipment not prepared correctly(not secure / missing docs / other)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "527")
            {
                Status = "Unsuccessful attempt to collect - dangerous goods papers were not hand over-EXCEPTION";
            }
            if (StatusCode == "528")
            {
                Status = "Shipment didn't collect - Customer responsibility - shipment not ready on time-EXCEPTION";
                return Status;
            }
            if (StatusCode == "529")
            {
                Status = "Unsuccessful pick-up attempt – consignment already handled by a different forwarding agent-EXCEPTION";
                return Status;
            }
            if (StatusCode == "530")
            {
                Status = "Shipment didn't collect-Raben responsibility- driver-EXCEPTION";
                return Status;
            }
            if (StatusCode == "531")
            {
                Status = "Shipment didn't collect in Collection Depot - Transport Department OCL mistake-EXCEPTION";
                return Status;
            }
            if (StatusCode == "550")
            {
                Status = "Shipment didn't collect - Raben responsibility - late arrival of pickup truck-EXCEPTION";
                return Status;
            }
            if (StatusCode == "551")
            {
                Status = "No space on shuttle from collection depot (ACL)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "552")
            {
                Status = "Collection order not carried out - free text-EXCEPTION";
                return Status;
            }
            if (StatusCode == "570")
            {
                Status = "Collection order canceled by CP-EXCEPTION";
                return Status;
            }
            if (StatusCode == "571")
            {
                Status = "New order for collection order no. free text-EXCEPTION";
                return Status;
            }
            if (StatusCode == "572")
            {
                Status = "Pickup request canceled by AP-EXCEPTION";
                return Status;
            }
            if (StatusCode == "700")
            {
                Status = "deliver the shipment again-EXCEPTION";
                return Status;
            }
            if (StatusCode == "701")
            {
                Status = "Shipment reassigned - new delivery address:-EXCEPTION";
                return Status;
            }
            if (StatusCode == "702")
            {
                Status = "Shipment back to sender-EXCEPTION";
                return Status;
            }
            if (StatusCode == "703")
            {
                Status = "Shipment back to VP-EXCEPTION";
                return Status;
            }
            if (StatusCode == "704")
            {
                Status = "Settlement - shipment/shortfall does not arrive-EXCEPTION";
                return Status;
            }
            if (StatusCode == "705")
            {
                Status = "Completion - please destroy/dispose of shipment-EXCEPTION";
                return Status;
            }
            if (StatusCode == "732")
            {
                Status = "myDelivery: Delivery date set up-EXCEPTION";
                return Status;
            }
            if (StatusCode == "733")
            {
                Status = "myDelivery Platform sent email without response";
                return Status;
            }
            if (StatusCode == "735")
            {
                Status = "Platform failed to send mail - myDelivery-EXCEPTION";
                return Status;
            }
            if (StatusCode == "736")
            {
                Status = "Email was sent to recipient - myDelivery-EXCEPTION";
                return Status;
            }
            if (StatusCode == "737")
            {
                Status = "Email address has been changed – myDelivery-EXCEPTION";
                return Status;
            }
            if (StatusCode == "738")
            {
                Status = "myDelivery: Customer requested for returning the goods-EXCEPTION";
                return Status;
            }
            if (StatusCode == "750")
            {
                Status = "Completion note - to be processed-EXCEPTION";
                return Status;
            }
            if (StatusCode == "751")
            {
                Status = "Inquiry - to be processed-EXCEPTION";
                return Status;
            }
            if (StatusCode == "752")
            {
                Status = "Completion note - not required to be processed-EXCEPTION";
                return Status;
            }
            if (StatusCode == "ME9")
            {
                Status = "Arrival by the latest time limit-EXCEPTION";
            }
            if (StatusCode == "GE1")
            {
                Status = "Arrival by the latest time limit-EXCEPTION";
                return Status;
            }
            if (StatusCode == "GE2")
            {
                Status = "Arrival after the latest time limit-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G00")
            {
                Status = "No difference in entrance-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G01")
            {
                Status = "Completely absent from the entrance-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G02")
            {
                Status = "Partially missing in the entrance-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G03")
            {
                Status = "Damaged in the entrance-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G04")
            {
                Status = "packaging differences-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G05")
            {
                Status = "welded pallet; no guarantee for content, since it cannot be checked-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G06")
            {
                Status = "Quantity difference bordero/unloading (recording error)-EXCEPTION";
                return Status;
            }
            if (StatusCode == "G07")
            {
                Status = "Shipment-related additional information-EXCEPTION";
                return Status;
            }

            return Status;
        }
        public static string movefile_new(string oldpath, string backupPath)
        {
            string newFilePath = backupPath;

            FileInfo oldFile = new FileInfo(oldpath);
            string fNameNoExt = Path.GetFileNameWithoutExtension(oldpath);
            //string ext = Path.GetFileNameWithoutExtension(oldpath);
           // string timeStamp = DateTime.Now.ToString("MMddyyyyHHmmssfff");
            string timeStamp = DateTime.Now.ToString("HHmmssfff");


            //newFilePath = newFilePath + fNameNoExt + "_" + timeStamp + oldFile.Extension;
            newFilePath = newFilePath + fNameNoExt+"__"+ timeStamp;

            File.Move(oldpath, newFilePath);

            return "";
        }
        public static void addquerysql(string query)
        {
            String strpath = "";
            strpath = "C:\\PODFiles\\raben.txt";
            string FILE_NAME = (strpath);
            System.IO.StreamWriter objWriter = new System.IO.StreamWriter(FILE_NAME, true);
            try
            {
                objWriter.WriteLine(System.Environment.NewLine + "************************************************************************" + System.Environment.NewLine + query);
                objWriter.Close();
            }
            catch (Exception ex)
            {
                objWriter.WriteLine(ex.Message + "Exception" + System.Environment.NewLine + "************************************************************************" + System.Environment.NewLine + query);
                objWriter.Close();
            }
        }
        public static void addquerysqlUP(string query)
        {
            String strpath = "";
            strpath = "C:\\PODFiles\\rabenUPDATE.txt";
            string FILE_NAME = (strpath);
            System.IO.StreamWriter objWriter = new System.IO.StreamWriter(FILE_NAME, true);
            try
            {
                objWriter.WriteLine(System.Environment.NewLine + "************************************************************************" + System.Environment.NewLine + query);
                objWriter.Close();
            }
            catch (Exception ex)
            {
                objWriter.WriteLine(ex.Message + "Exception" + System.Environment.NewLine + "************************************************************************" + System.Environment.NewLine + query);
                objWriter.Close();
            }
        }
        public static void addqueryrabendata(string query)
        {
            String strpath = "";
            strpath = "C:\\PODFiles\\rabendata.txt";
            string FILE_NAME = (strpath);
            System.IO.StreamWriter objWriter = new System.IO.StreamWriter(FILE_NAME, true);
            try
            {
                objWriter.WriteLine(System.Environment.NewLine + "************************************************************************" + System.Environment.NewLine + query);
                objWriter.Close();
            }
            catch (Exception ex)
            {
                objWriter.WriteLine(ex.Message + "Exception" + System.Environment.NewLine + "************************************************************************" + System.Environment.NewLine + query);
                objWriter.Close();
            }
        }
        public static void addloginfo(string query)
        {
            try
            {
                String strpath = "";
                strpath = System.Windows.Forms.Application.StartupPath + "\\logfile.txt";
                string FILE_NAME = (strpath);
                System.IO.StreamWriter objWriter = new System.IO.StreamWriter(FILE_NAME, true);
                objWriter.WriteLine(DateTime.Now.ToString() + "  " + query);
                objWriter.Close();
                objWriter.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
