﻿using Arthur;
using Arthur.App.Data;
using Arthur.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arthur.App;
using Arthur.App.Model;

namespace Arthur.Business
{
    public class Account
    {
        public static Result Register(string name, string password, string confirm_pwd)
        {
            name = name.Trim();
            password = password.Trim();
            confirm_pwd = confirm_pwd.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return new Result("请输入用户名！");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return new Result("请输入密码！");
            }

            if (string.IsNullOrWhiteSpace(confirm_pwd))
            {
                return new Result("再次输入密码！");
            }

            if (string.IsNullOrWhiteSpace(confirm_pwd))
            {
                return new Result("再次输入密码！");
            }

            if (password != confirm_pwd)
            {
                return new Result("两次输入密码不一致！");
            }

            return Register(name, "", password, false);
        }

        public static Result Register(string name, string number, string password, bool isEnabled)
        {

            User user = new User();
            if (Context.AccountContext.Users.Where(u => u.Name == name).Count() > 0)
            {
                return new Result("系统中已存在用户：" + name);
            }
            user.Name = name;
            user.Number = number;
            user.Password = EncryptHelper.EncodeBase64(password);
            user.IsEnabled = isEnabled;
            user.RegisterTime = DateTime.Now;
            user.RoleId = Context.AccountContext.Roles.Single(r => r.Name == "操作员").Id;
            Context.AccountContext.Users.Add(user);
            Context.AccountContext.SaveChanges();

            return Result.OK;
        }


        public static Result Login(string name, string password)
        {
            name = name.Trim();
            password = password.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return new Result("请输入用户名！");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return new Result("请输入密码！");
            }

            var user = new User();

            var entityPassword = EncryptHelper.EncodeBase64(password);
            user = Context.AccountContext.Users.FirstOrDefault(u => u.Name == name && u.Password == entityPassword) ?? new User();
            user.LastLoginTime = DateTime.Now;
            user.LoginTimes++;
            Context.AccountContext.SaveChanges();

            if (!user.IsEnabled)
            {
                return new Result(user.Name + "尚未激活或被禁用！");
            }

            Current.User = user;

            if (Current.User.Id > 0)
            {
                return Result.OK;
            }
            return new Result("用户名或密码错误！");
        }

        public static User GetUser(int id)
        {
            return Context.AccountContext.Users.FirstOrDefault(u => u.Id == id) ?? new User();
        }


        //public static bool Logout()
        //{
        //    OperationHelper.ShowTips(AppCurrent.User.Name + "成功注销");
        //    AppCurrent.User = new User(-1);
        //    return true;
        //}
    }
}
