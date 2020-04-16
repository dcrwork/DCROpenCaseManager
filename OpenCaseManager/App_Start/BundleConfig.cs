using System.Web;
using System.Web.Optimization;

namespace OpenCaseManager
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.IgnoreList.Clear();

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/bluebird").Include(
          "~/Scripts/bluebird.min.js"));

            // syddjurs css
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/DemoStyling.css", //Change this line to change which main style should be used
                      "~/Content/bootstrap.css",
                      "~/Content/fontastic.css",
                      "~/Content/noty/noty.css",
                      "~/Content/noty/themes/mint.css",
                      "~/Content/site.css",
                      "~/Content/myChildren.css",
                      "~/Content/Child.css",
                      "~/Content/PartialViews.css",
                      "~/Content/tabs.css",
                      "~/Content/breadcrumbs.css",
                      "~/Content/journalnote.css"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                      "~/Scripts/moment.min.js",
                      "~/Scripts/moment-with-locales.min.js"));


            bundles.Add(new ScriptBundle("~/bundles/noty").Include(
                      "~/Scripts/noty.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/api").Include(
                      "~/Scripts/api.js"));

            bundles.Add(new ScriptBundle("~/bundles/translations").Include(
                      "~/Scripts/translations.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                      "~/Scripts/app.js",
                      "~/Scripts/core.js",
                      "~/Scripts/header.js"));


        }
    }
}