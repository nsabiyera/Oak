using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Oak
{
    public class DynamicJsonResult : JsonResult
    {
        public DynamicJsonResult()
            : this(null)
        {

        }
        
        public DynamicJsonResult(object data)
        {
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            Data = data;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (DynamicToJson.CanConvertObject(Data))
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.Output.Write(DynamicToJson.Convert(Data));
                context.HttpContext.Response.Output.Flush();
            }
            else base.ExecuteResult(context);
        }
    }
}