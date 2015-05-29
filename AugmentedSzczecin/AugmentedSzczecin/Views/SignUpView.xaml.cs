using Windows.UI;
using Windows.UI.ViewManagement;
namespace AugmentedSzczecin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignUpView
    {
        public SignUpView()
        {
            InitializeComponent();
            StatusBar.GetForCurrentView().ForegroundColor = Color.FromArgb(255, 52, 143, 217);
        }

    }
}
