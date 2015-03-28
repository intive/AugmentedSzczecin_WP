using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    class SignUpViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        public SignUpViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
    }
}
