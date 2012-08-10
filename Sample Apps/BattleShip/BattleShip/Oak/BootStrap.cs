using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Oak
{
    public static class BootStrap
    {
        public static void Init()
        {
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(object), new ParamsModelBinder()));

            if(System.Diagnostics.Process.GetCurrentProcess().ProcessName == "iisexpress")
            {
                Massive.DynamicRepository.WriteDevLog = true;
            }
        }
    }
}
