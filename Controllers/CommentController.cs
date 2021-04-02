using DiscussionForum.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiscussionForum.Controllers
{
    public class CommentController : Controller
    {
        private HttpClient httpClient;

        public CommentController()
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
        
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Create(PostViewModel postViewModel)
        {
            if (Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            System.Diagnostics.Debug.WriteLine(postViewModel.postComment.Comment1);
            System.Diagnostics.Debug.WriteLine(postViewModel.post.Id);
            postViewModel.postComment.CreatedTime = DateTime.Now;
            postViewModel.postComment.CreatedBy = Session["username"].ToString();
            postViewModel.postComment.PostId = postViewModel.post.Id;
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session["token"].ToString());
                using (HttpResponseMessage response = await httpClient.PostAsJsonAsync<Comment>("api/Comments", postViewModel.postComment))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        return RedirectToAction("View", "Post", new { id = postViewModel.post.Id });
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
            return RedirectToAction("View", "Post", new { postViewModel = postViewModel });
        }
    }
}