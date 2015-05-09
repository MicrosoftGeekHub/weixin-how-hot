﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeixinServer
{
    public class WeixinDBContext : DbContext
    {
        public WeixinDBContext() 
            : base("WeixinDB")
        {

        }

        public DbSet<Danmu> Danmus { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<BlackValue> BlackList { get; set; }
    }

    public sealed class Danmu
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string OpenId { get; set; }
        public string Content { get; set; }
        public int CreateTime { get; set; }
    }

    public sealed class Token
    {
        [Key]
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string ExpireDate { get; set; }
    }

    [Table("BlackList")]
    public class BlackValue
    {
        [Key]
        public int Id { get; set; }
        public string Value { get; set; }
    }
}