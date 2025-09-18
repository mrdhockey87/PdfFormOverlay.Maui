using System.Reflection;

namespace PdfFormOverlay.Maui
{
    // All the code in this file is included in all platforms.

    public class FrameworkVersion
    {

        // Get the assembly containing this code.
        private readonly Assembly assembly = typeof(FrameworkVersion).Assembly;

        public string GetAssemblyVersion()
        {
            // Get the AssemblyInformationalVersionAttribute, which corresponds to the <Version> in the .csproj.
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            // If the attribute is found, return its value. Otherwise, fall back to the AssemblyName.
            return versionAttribute?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "2.002.0003";
        }
    }
}

/*
 *  version 2.002.0004 - Made a number of changes and correction to the app, now it compiles without errors, there are several warnings
 *                       mainly about missing awaits which I need to fix by making the calling methods async and adding Task around thw methods steps
 *                       that are calling async methods. I also need to fix some null and non null types etc. mdail 9-18-25
 *  version 2.002.0003 - Fixed some of the erros, however the are still a tons of errors, but I'm not sure why some of them are happening. mdail 9-17-25`
 *  version 2.002.0002 - Had to change the PDF library from iTextSharp to PDFsharp because iTextSharp not supported in .NET MAUI. mdail 9-17-25
 *  version 1.001.0001 - Initial version created buy the AI online code generator. Problem is it seemed to be missing the 
 *  FormOverlayService in this version. I used the one from the first version I had it generate. that did not have the
 *  security features. and it used json instead of a database. It uses Vitvov.Maui.PDFView - PDF viewing component,
 *  iTextSharp - PDF form field extraction and manipulation, sqlite-net-pcl - SQLite database access, SQLiteNetExtensions -
 *  Enhanced SQLite features,SQLitePCLRaw.bundle_green - SQLite native libraries, and Newtonsoft.Json - JSON serialization. mdail 9-16-25
 */