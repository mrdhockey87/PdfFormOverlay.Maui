namespace PdfFormOverlay.Maui
{
    // All the code in this file is included in all platforms.

    public static class AppVersion
    {
        static string framework_version = "1.0001.0001";
        static string framework_build = "1";
        public static string AppVersionNo
        {
            get
            {
                return framework_version;
            }
        }
        public static string AppBuildNo
        {
            get
            {
                return framework_build;
            }
        }
    }
}

/*
 *  version 1.0001.0001 - Initial version created buy the AI online code generator. Problem is it seemed to be missing the 
 *  FormOverlayService in this version. I used the one from the first version I had it generate. that did not have the
 *  security features. and it used json instead of a database. It uses Vitvov.Maui.PDFView - PDF viewing component,
 *  iTextSharp - PDF form field extraction and manipulation, sqlite-net-pcl - SQLite database access, SQLiteNetExtensions -
 *  Enhanced SQLite features,SQLitePCLRaw.bundle_green - SQLite native libraries, and Newtonsoft.Json - JSON serialization. mdail 9-16-25
 */