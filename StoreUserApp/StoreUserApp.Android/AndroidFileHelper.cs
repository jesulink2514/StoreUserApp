using System;
using System.IO;
using StoreUserApp.Droid;
using Xamarin.Forms;
[assembly: Dependency(typeof(AndroidFileHelper))]
namespace StoreUserApp.Droid
{
    public class AndroidFileHelper : IFileHelper
    {
        public string GetFullPath(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(path, filename);
        }
    }
}