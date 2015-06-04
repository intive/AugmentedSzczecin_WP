using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
namespace AugmentedSzczecin.ViewModels
{
    public class AboutViewModel
    {
        public AboutViewModel()
        {
            SetInformationAboutVersion();
        }
        
        public string InformationAboutVersion
        {
            get;
            set;
        }

        private void SetInformationAboutVersion()
        {
            var loader = new ResourceLoader();

            PackageVersion pv = Package.Current.Id.Version;
            Version version = new Version(Package.Current.Id.Version.Major,
                Package.Current.Id.Version.Minor,
                Package.Current.Id.Version.Build,
                Package.Current.Id.Version.Revision);

            InformationAboutVersion = loader.GetString("AboutVersion") + version.ToString();
        }
    }
}
