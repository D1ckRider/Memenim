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

        public static int LocalUserID { get { return m_LocalUserId; } }

        private static readonly HttpClient client = new HttpClient();

        private static string UserToken;
        private static int m_LocalUserId;

        private static string GENERAL_API_STRING = "http://dev.apianon.ru:3000/";

        public static async Task<string> RegisterUser(string login, string pass)
        {
            AuthInfo inf = new AuthInfo();
            inf.login = login;
            inf.password = pass;

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(GENERAL_API_STRING + "users/add", data);

            string result = response.Content.ReadAsStringAsync().Result;

            AuthResponse resp = JsonConvert.DeserializeObject<AuthResponse>(result);
            if (resp.data.token != null)
            {
                m_LocalUserId = resp.data.id;
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

            var response = await client.PostAsync(GENERAL_API_STRING + "users/login2", data);

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

            var response = await client.PostAsync(GENERAL_API_STRING + "posts/commentAdd", data);

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

            var response = await client.PostAsync(GENERAL_API_STRING + "posts/get", null);

            string result = response.Content.ReadAsStringAsync().Result;
            
            PostResult resp = JsonConvert.DeserializeObject<PostResult>(result);
            return resp.data;
        }

        class CommentsResult
        {
            public bool error { get; set; }
            public List<CommentData> data { get; set; }
        }

        class PostRequestData
        {
            public int post_id { get; set; }
        }

        public static async Task<List<CommentData>> GetCommentsForPost(int postId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            PostRequestData inf = new PostRequestData();
            inf.post_id = postId;

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync(GENERAL_API_STRING + "posts/getComments", data);

            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            CommentsResult resp = JsonConvert.DeserializeObject<CommentsResult>(result);

            return resp.data;
        }

        class IdData
        {
            public int id { get; set; }
        }
        
        class GetProfileResult
        {
            public bool error { get; set; }
            public List<ProfileData> data { get; set; }
        }

        public static async Task<ProfileData> GetLocalUserInfo()
        {
            return await GetUserInfo(m_LocalUserId);
        }

        public static async Task<ProfileData> GetUserInfo(int userId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            IdData inf = new IdData();
            inf.id = userId;

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync(GENERAL_API_STRING + "users/profile/getById", data);

            string result = response.Content.ReadAsStringAsync().Result;
            GetProfileResult resp = JsonConvert.DeserializeObject<GetProfileResult>(result);

            return resp.data[0];
        }

        public static async Task<bool> EditUserInfo(ProfileData userData)
        {
            var json = JsonConvert.SerializeObject(userData);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync(GENERAL_API_STRING + "users/profile/set", data);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> AddView(int postId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            PostRequestData inf = new PostRequestData();
            inf.post_id = postId;

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync(GENERAL_API_STRING + "posts/viewAdd", data);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> AddShares(int postId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            PostRequestData inf = new PostRequestData();
            inf.post_id = postId;

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync(GENERAL_API_STRING + "posts/repost", data);
            return response.IsSuccessStatusCode;
        }

        class CommentInteractionData
        {
            public int comment_id { get; set; }
        }

        public static async Task<bool> LikeComment(int commentId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            CommentInteractionData inf = new CommentInteractionData();
            inf.comment_id = commentId;

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync(GENERAL_API_STRING + "posts/likeCommentAdd", data);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> DislikeComment(int commentId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            CommentInteractionData inf = new CommentInteractionData();
            inf.comment_id = commentId;

            var json = JsonConvert.SerializeObject(inf);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserToken);

            var response = await client.PostAsync(GENERAL_API_STRING + "posts/dislikeCommentAdd", data);
            return response.IsSuccessStatusCode;
        }

    }
}
