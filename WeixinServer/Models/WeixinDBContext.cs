using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

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
        public DbSet<ImageStorage> ImageStorages { get; set; }
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

    [Table("ImageStorages")]
    public sealed class ImageStorage
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string OpenId { get; set; }
        public int CreateTime { get; set; }
        public string PicUrl { get; set; }
        //public byte[] PicContent { get; set; }
        public string ParsedUrl { get; set; }
        public byte[] ParsedContent { get; set; }
        public string ParsedDescription { get; set; }
        public string TimeLog { get; set; }
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