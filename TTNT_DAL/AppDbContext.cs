using System.Data.Entity;
using TTNT_DAL.Models;

namespace TTNT_DAL
{
    public class AppDbContext : DbContext
    {
        // "ChuoiKetNoi" là tên sẽ khai báo bên App.config của GUI
        public AppDbContext() : base("name=ChuoiKetNoi")
        {
            // Tự tạo DB nếu chưa có
            Database.SetInitializer(new CreateDatabaseIfNotExists<AppDbContext>());
        }

        public DbSet<BaiToan> BaiToans { get; set; }
        public DbSet<Dinh> Dinhs { get; set; }
        public DbSet<Canh> Canhs { get; set; }
    }
}