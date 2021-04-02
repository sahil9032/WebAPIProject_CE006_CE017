using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiscussionForum.Controllers
{
    public class HomeController : Controller
    {
        private HttpClient httpClient;

        public HomeController()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            string baseUrl = ConfigurationManager.AppSettings["baseUrl"];

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
        }
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page | Discussion Forum";
            IList<Post> postData = null;
            try
            {
                using (var response = await httpClient.GetAsync("api/Posts"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        postData = response.Content.ReadAsAsync<IList<Post>>().Result;
                    }
                    else
                    {
                        postData = new List<Post>();
                        var json = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(String.Empty, json);
                        ModelState.AddModelError(String.Empty, "Try again after some time.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError(String.Empty, "Try again after some time.");
            }
            return View(postData);
        }
    }
}
