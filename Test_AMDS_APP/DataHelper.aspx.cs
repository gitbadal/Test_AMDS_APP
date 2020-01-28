using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Test_AMDS_APP
{
    public partial class DataHelper : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        [WebMethod]
        public static TagValueResponse[] GetPLCData(int idPLC, string FromDate, string ToDate)
        {
            TagValueResponse[] resp = null;
            try
            {
                //DateTime chrtdate_from, chrtdate_to;
                //GraphTypeDC[] objOutPutDataDCarray = null;
                //chrtdate_from = Convert.ToDateTime(FromDate);
                //chrtdate_to = Convert.ToDateTime(ToDate);
                //objOutPutDataDCarray = WcfService.objHMI2.STUB_GetAIEQuipFlag(chrtdate_from, chrtdate_to, EqpNO, Scenarios);
                //return objOutPutDataDCarray;
                string projectId = "projamdstrial";
                DateTime chrtdate_from, chrtdate_to;

                chrtdate_from = Convert.ToDateTime(FromDate);
                chrtdate_to = Convert.ToDateTime(ToDate);

                // Instantiates a client.
                using (StorageClient storageClient = StorageClient.Create())
                {
                    // The name for the new bucket.
                    string bucketName = "TestAMDSData/BFProcess_FBF_DATA_HEARTH_2020_1_28_20200128073415.csv";
                    try
                    {
                        // Creates the new bucket.
                        var bucket = storageClient.GetBucket("amds_bucket");
                        using (MemoryStream mem = new MemoryStream())
                        {
                            storageClient.DownloadObject("amds_bucket", bucketName, mem);
                            //StreamReader reader = new StreamReader(mem);
                            //string content = reader.ReadToEnd();
                            string content = Encoding.ASCII.GetString(mem.ToArray());
                            string[] sep = { "\n" };
                            List<string> lines = content.Split(sep, StringSplitOptions.RemoveEmptyEntries).ToList();
                            lines.RemoveAt(0);
                            List<TagValueResponse> dataList = new List<TagValueResponse>();
                            lines.ForEach(x => dataList.Add(new TagValueResponse()
                            {
                                READTIME = Convert.ToDateTime(x.Split(',')[0]),
                                TAGID = Convert.ToInt32(x.Split(',')[1]),
                                VALUE = Convert.ToDecimal(x.Split(',')[2])
                            }));
                            resp = dataList.Where(x => x.READTIME >= chrtdate_from && x.READTIME <= chrtdate_to && x.TAGID == idPLC).ToArray();

                        }
                        Console.WriteLine($"Bucket {bucketName} created.");
                    }
                    catch (Google.GoogleApiException e)
                    when (e.Error.Code == 409)
                    {
                        // The bucket already exists.  That's fine.
                        Console.WriteLine(e.Error.Message);
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }

    public class TagValueResponse
    {

        public DateTime READTIME { get; set; }
        public int TAGID { get; set; }
        public decimal? VALUE { get; set; }

    }
}