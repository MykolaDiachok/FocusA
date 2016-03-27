using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using NLog;
using NLog.Config;
using System.Data.Linq;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for WebService
/// </summary>
[WebService(Namespace = "http://focus-a.space/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService
{
    private Logger logger = LogManager.GetCurrentClassLogger();

    public WebService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld()
    {
        return "Hello World";
    }

    [WebMethod]
    public string getComInit(string fpnumber)
    {
        using (DataClassesDataContext _focusA = new DataClassesDataContext())
        {
            Table<tbl_ComInit> tablePayment = _focusA.GetTable<tbl_ComInit>();
            var comInit = (from list in tablePayment
                           where list.Init == true
                           && list.FPNumber == int.Parse(fpnumber)
                           select list);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(comInit);
        }
    }

}
