using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.WebPages;
using ChatApp.Util;

namespace ChatApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DisplayModeProvider.Instance.Modes.Insert(0, new MobileDisplayMode());
            DisplayModeProvider.Instance.Modes.Insert(1, new DefaultDisplayMode("Mobile")
            {
                ContextCondition = context =>
                    context.GetOverriddenBrowser().IsMobileDevice
            }); 

        }
    }
}