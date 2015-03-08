using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Interfaces
{
	public interface IHttpRequestService
	{
		Task<string> HttpGetAsync();
	}
}
