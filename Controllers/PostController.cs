using DiscussionForum.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiscussionForum.Controllers
{
    public class PostController : Controller
    {
        private HttpClient httpClient;

        public PostController()
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

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            IList<Post> postData = null;
            try
            {
                using (var response = await httpClient.GetAsync("api/Posts"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        postData = response.Content.ReadAsAsync<IList<Post>>().Result;
                        postData = postData.Where(post => post.CreatedBy == Session["username"].ToString()).Select(post => post).ToList();
                    }
                    else
                    {
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

        [HttpGet]
        public async Task<ActionResult> View(int id)
        {
            Post post = new Post();
            try
            {
                using (var response = await httpClient.GetAsync("api/Posts/" + id.ToString()))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        post = response.Content.ReadAsAsync<Post>().Result;
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Post not found");
                    }
                }
/*                using (var response = await httpClient.GetAsync("api/Comments"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        comments = response.Content.ReadAsAsync<IList<Comment>>().Result;
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Post not found");
                    }
                }*/
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError(String.Empty, "Post not found");
            }
            return View(new PostViewModel { post = post, postComment = new Comment()});
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Create(Post post)
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            post.CreatedTime = DateTime.Now;
            post.CreatedBy = Session["username"].ToString();
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session["token"].ToString());
                using (HttpResponseMessage response = await httpClient.PostAsJsonAsync<Post>("api/Posts", post))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        JObject jObject = JObject.Parse(json);
                        post.Id = int.Parse((string)jObject["Id"]);
                        return RedirectToAction("View", "Post", new { id = post.Id });
                    }
                    else
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(String.Empty, json);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError(String.Empty, "Try again after some time.");
            }
            return View(post);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            Post post = null;
            try
            {
                using (var response = await httpClient.GetAsync("api/Posts/" + id.ToString()))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        post = response.Content.ReadAsAsync<Post>().Result;
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Post not found");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return View(post);
        }

        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Edit(Post post)
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session["token"].ToString());
                using (HttpResponseMessage response = await httpClient.PutAsJsonAsync<Post>("api/Posts/" + post.Id.ToString(), post))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("View", "Post", new { id = post.Id });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError(String.Empty, "Try again after some time.");
            }
            return View(post);
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            Post post = null;
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session["token"].ToString());
                using (var response = await httpClient.GetAsync("api/Posts/" + id.ToString()))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        post = response.Content.ReadAsAsync<Post>().Result;
                    }
                    else
                    {
                        post = new Post();
                        var json = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(String.Empty, json);
                        ModelState.AddModelError(String.Empty, "Post not found");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError(String.Empty, "Try again after some time.");
            }
            return View(post);
            
        }

        [HttpPost]
        public async Task<ActionResult> Delete(Post post)
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            try
            {
                using (var response = await httpClient.GetAsync("api/Posts/" + post.Id.ToString()))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Post newPost = response.Content.ReadAsAsync<Post>().Result;
                        foreach(var comment in newPost.Comments)
                        {
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session["token"].ToString());
                            var res = await httpClient.DeleteAsync("api/Comments/" + comment.Id.ToString());
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Post not found");
                    }
                }
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session["token"].ToString());
                using (HttpResponseMessage response = await httpClient.DeleteAsync("api/Posts/" + post.Id.ToString()))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Post");
                    }
                    else
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(String.Empty, json);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError(String.Empty, "Try again after some time.");
            }
            return View(post);
        }
    }
}