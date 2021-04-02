using DiscussionForum.Models;
using Newtonsoft.Json.Linq;
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
    public class UsersController : Controller
    {
        private HttpClient httpClient;
        public UsersController()
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

        public ActionResult Index()
        {
            return View();
        }

        //GET: Users/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        private async Task<HttpResponseMessage> DoLogin(string Email, string Password)
        {
            var data = new List<KeyValuePair<string, string>>();
            data.Add(new KeyValuePair<string, string>("grant_type", "password"));
            data.Add(new KeyValuePair<string, string>("username", Email));
            data.Add(new KeyValuePair<string, string>("password", Password));
            var req = new HttpRequestMessage(HttpMethod.Post, "token") { Content = new FormUrlEncodedContent(data) };
            var response = await httpClient.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                JObject jObject = JObject.Parse(json);
                string token = (string)jObject["access_token"];
                string userName = (string)jObject["userName"];
                Session["token"] = token;
                Session["username"] = userName;
            }
            else
            {
                var json = response.Content.ReadAsStringAsync().Result;
                JObject jObject = JObject.Parse(json);
                string err = (string)jObject["error_description"];
                ModelState.AddModelError(string.Empty, err);
            }
            return response;
        }
        
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            try
            {
                using (HttpResponseMessage response = await DoLogin(loginViewModel.Email, loginViewModel.Password))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("index", "home");
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return View(loginViewModel);
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterBindingModel registerBindingModel)
        {
            try
            {
                using(HttpResponseMessage response = await httpClient.PostAsJsonAsync<RegisterBindingModel>("api/Account/Register", registerBindingModel))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpResponseMessage response1 = await DoLogin(registerBindingModel.Username, registerBindingModel.Password))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return RedirectToAction("index", "home");
                            }
                        }
                    }
                    else
                    {
                      
                        System.Diagnostics.Debug.WriteLine("in else");
                        var json = response.Content.ReadAsStringAsync().Result;    
                        JObject jObject = JObject.Parse(json);
                        JToken modelState = jObject["ModelState"];
                        var errArr = modelState[""].ToArray();
                        foreach(var line in errArr)
                        {
                            ModelState.AddModelError(String.Empty, line.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return View(registerBindingModel);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session["username"] = null;
            Session["token"] = null;
            return RedirectToAction("Index", "Home");
        }
    }
}