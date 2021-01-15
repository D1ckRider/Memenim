﻿using System;

namespace Memenim.Settings.Entities
{
    public class User
    {
        public string Login { get; }
        public string Token { get; }
        public int Id { get; }
        public UserStoreType StoreType { get; }

        public User(string login, string token, int id,
            UserStoreType storeType)
        {
            Login = login;
            Token = token;
            Id = id;
            StoreType = storeType;
        }
    }
}
