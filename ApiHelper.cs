﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AnonymDesktopClient.DataStructs;

namespace AnonymDesktopClient
{
    static class ApiHelper
    {
        private class AuthInfo
        {
            public string login { get; set; }
            public string password { get; set; }
        }

        private class AuthResponse
        {
            public bool error { get; set; }
            public int code { get; set; }
            public UserData data { get; set; }
            public string message { get; set; }
        }

        private static readonly HttpClient client = new HttpClient();

        private static string UserToken;

        public static async Task<string> RegisterUser(string login, string pass)
        {
            AuthInfo inf = new AuthInfo();
            inf.login = login;
            inf.password = pass;

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://dev.apianon.ru:3000/users/add", data);

            string result = response.Content.ReadAsStringAsync().Result;

            AuthResponse resp = JsonConvert.DeserializeObject<AuthResponse>(result);
            if (resp.data.token != null)
            {
                UserToken = resp.data.token;
            }
            return resp.message;
        }

        public static async Task<string> Auth(string login, string pass)
        {
            AuthInfo inf = new AuthInfo();
            inf.login = login;
            inf.password = pass;

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://dev.apianon.ru:3000/users/login2", data);

            string result = response.Content.ReadAsStringAsync().Result;

            AuthResponse resp = JsonConvert.DeserializeObject<AuthResponse>(result);
            if(resp.data.token != null)
            {
                UserToken = resp.data.token;
            }
            return resp.message;
        }

        class SendCommentData
        {
            public int post_id { get; set; }
            public string text { get; set; }
            public int anonim { get; set; }
        }

        public static async Task SendComment(int postId, string content, bool? anonymous)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            SendCommentData inf = new SendCommentData();
            inf.post_id = postId;
            inf.text = content;
            inf.anonim = Convert.ToInt32(anonymous);

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync("http://dev.apianon.ru:3000/posts/commentAdd", data);

            string result = response.Content.ReadAsStringAsync().Result;
        }


        class PostResult
        {
            public bool error { get; set; }
            public List<PostData> data { get; set; }
        }

        public static async Task<List<PostData>> GetAllPosts()
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync("http://dev.apianon.ru:3000/posts/get", null);

            string result = response.Content.ReadAsStringAsync().Result;
            
            PostResult resp = JsonConvert.DeserializeObject<PostResult>(result);
            return resp.data;
        }

        class CommentsResult
        {
            public bool error { get; set; }
            public List<CommentData> data { get; set; }
        }

        class GetCommentData
        {
            public int post_id { get; set; }
        }

        public static async Task<List<CommentData>> GetCommentsForPost(int postId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            GetCommentData inf = new GetCommentData();
            inf.post_id = postId;

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync("http://dev.apianon.ru:3000/posts/getComments", data);

            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            CommentsResult resp = JsonConvert.DeserializeObject<CommentsResult>(result);

            return resp.data;
        }

        public static async Task GetUserInfo(int userId)
        {

        }
    }
}
