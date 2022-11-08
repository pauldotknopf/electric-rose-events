using System;
using Microsoft.AspNetCore.Mvc;

namespace Site.Controllers
{
	public class SiteController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}

