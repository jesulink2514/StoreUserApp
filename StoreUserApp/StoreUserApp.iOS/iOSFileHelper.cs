using System;
using System.IO;
using StoreUserApp.iOS;
using Xamarin.Forms;

[assembly:Dependency(typeof(iOSFileHelper))]
namespace StoreUserApp.iOS
{
    public class iOSFileHelper : IFileHelper
    {
        public string GetFullPath(string filename)
        {
            var docFolder = Environment
                .GetFolderPath(Environment.SpecialFolder.Personal);
            
            var libFolder = Path.Combine(docFolder, "..", "Library");

            return Path.Combine(libFolder, filename);
        }
    }
}