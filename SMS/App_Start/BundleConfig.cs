using System.Web;
using System.Web.Optimization;

namespace SMS
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            //////////////////styles/////////////////////////////


            bundles.Add(new StyleBundle("~/bundles/bootstrapStyle").Include(
               "~/plugins/bootstrap/css/bootstrap.css", new CssRewriteUrlTransformWrapper()
               ));

            bundles.Add(new StyleBundle("~/bundles/layoutStyle").Include(
                "~/plugins/dist/css/AdminLTE.css",
                "~/plugins/dist/css/skins/skin-purple-light.css"
                ));

            bundles.Add(new StyleBundle(("~/bundles/pageStyle")).Include(
               "~/plugins/select2/select2.css",//Select2 for dropdownlist
               "~/plugins/iCheck/all.css",//iCheck for checkboxes and radio inputs  
               "~/plugins/toastr/toastr.css",//toastr for popup
               "~/plugins/IsLoading/jquery.loading.css",//jquery loading page
               "~/plugins/datepicker/datepicker3.css",//bootstrap datepicker
               "~/Styles/Site.css"//validaiton and other scripts

              ));

            bundles.Add(new StyleBundle(("~/bundles/dataTableStyle")).Include(
              "~/plugins/datatables/media/css/dataTables.bootstrap.min.css"//datatable css 
             ));

            bundles.Add(new StyleBundle(("~/bundles/bootstrapSwitchStyle")).Include(
               "~/plugins/bootstrapSwitch/css/bootstrap-switch.css"//datatable css 
              ));

            bundles.Add(new StyleBundle("~/bundles/fileUploadStyle").Include(
               "~/plugins/fileUpload/css/fileinput.css"//FileUpload css
             ));

            bundles.Add(new StyleBundle(("~/bundles/jQueryStepsSytle")).Include(
              "~/plugins/jQuerySteps/css/jQuerySteps.css"//Jquery Steps css 
             ));

            bundles.Add(new StyleBundle(("~/bundles/rateyoSytle")).Include(
                "~/plugins/RateYo/jquery.rateyo.css"//RateYo css 
             ));
            /////////////////scripts//////////////////////////////////////
            bundles.Add(new ScriptBundle(("~/bundles/layoutScript")).Include(
               "~/plugins/jQuery/jquery-2.1.4.js",//jQuery 2.1.4
               "~/plugins/bootstrap/js/bootstrap.js",//Bootstrap 3.3.5
               "~/plugins/dist/js/app.js"// AdminLTE App
               ));


            bundles.Add(new ScriptBundle(("~/bundles/pageScript")).Include(
                "~/plugins/select2/select2.js",//Select2 for dropdownlist
                "~/plugins/iCheck/icheck.js",//iCheck for checkboxes and radio inputs  
                "~/plugins/toastr/toastr.js",//toastr for popup
                "~/plugins/bootbox/bootbox.js",//bootbox plugin
                "~/plugins/IsLoading/jquery.loading.js",//isLoading plugin
                "~/plugins/datepicker/bootstrap-datepicker.js"//bootstrap datepicker
                ));


            bundles.Add(new ScriptBundle(("~/bundles/inputMaskScript")).Include(
               "~/plugins/input-mask/jquery.inputmask.js",
               "~/plugins/input-mask/jquery.inputmask.extensions.js",
               "~/plugins/input-mask/jquery.inputmask.date.extensions.js"));

            //datatable js 
            bundles.Add(new ScriptBundle(("~/bundles/dataTableScript")).Include(
               "~/plugins/datatables/media/js/jquery.dataTables.js",
                "~/plugins/datatables/media/js/dataTables.bootstrap.js"));

            //input mask script
            bundles.Add(new ScriptBundle(("~/bundles/inputMaskScript")).Include(
                   "~/plugins/input-mask/jquery.inputmask.js",
                   "~/plugins/input-mask/jquery.inputmask.extensions.js",
                   "~/plugins/input-mask/jquery.inputmask.date.extensions.js",
                   "~/plugins/input-mask/jquery.inputmask.numeric.extensions.js"));

            //moment.js
            bundles.Add(new ScriptBundle(("~/bundles/moment")).Include(
            "~/plugins/moment/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrapSwitchScript").Include(
            "~/plugins/bootstrapSwitch/js/bootstrap-switch.js"));

            //FileUpload js
            bundles.Add(new ScriptBundle(("~/bundles/fileUploadScript")).Include(
               "~/plugins/fileUpload/js/fileinput.js"));

            //Jquery Steps js 
            bundles.Add(new ScriptBundle("~/bundles/jqueryStepsScript").Include(
               "~/plugins/jQuerySteps/js/jquery.steps.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/plugins/jQueryVal/jquery.unobtrusive*",
                        "~/plugins/jQueryVal/jquery.validate*"));

            //Jquery Form js 
            bundles.Add(new ScriptBundle("~/bundles/jqueryForms").Include(
                        "~/plugins/jQueryForms/jquery.form.js"));

            //CheckboxSwitch
            bundles.Add(new ScriptBundle("~/bundles/checkboxSwitch").Include(
                        "~/plugins/checkboxSwitch/js/bootstrap-checkbox.js"));

            bundles.Add(new ScriptBundle("~/bundles/rateyo").Include(
                "~/plugins/RateYo/jquery.rateyo.js"//RateYo js 

               ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/plugins/modernizr/modernizr-*"));



            BundleTable.EnableOptimizations = true;
        }
        public class CssRewriteUrlTransformWrapper : IItemTransform
        {
            public string Process(string includedVirtualPath, string input)
            {
                return new CssRewriteUrlTransform().Process("~" + VirtualPathUtility.ToAbsolute(includedVirtualPath), input);
            }
        }

    }
}