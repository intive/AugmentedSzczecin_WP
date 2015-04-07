using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class BaseAugmentedSzczecinViewModel : Screen
    {
        private bool _isProgressRingVisible;
        public bool IsProgressRingVisible
        {
            get { return _isProgressRingVisible; }
            set
            {
                if (value != _isProgressRingVisible)
                {
                    _isProgressRingVisible = value;
                    NotifyOfPropertyChange(() => IsProgressRingVisible);
                }
            }
        }

        private bool _isProgressRingActive;
        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set
            {
                if (value != _isProgressRingActive)
                {
                    _isProgressRingActive = value;
                    NotifyOfPropertyChange(() => IsProgressRingActive);
                }
            }
        }
    }
}
