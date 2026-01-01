using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTNT_DAL.Models;

namespace TTNT_DAL.Models
{
    [Table("Dinh")]
    public class Dinh
    {
        [Key]
        public int Id { get; set; }
        public string Ten { get; set; } // Tên hiển thị (1, 2...)
        public int X { get; set; }
        public int Y { get; set; }
        public int Mau { get; set; }    // 0=Trắng, 1=Đỏ...

        public int BaiToanId { get; set; }
        [ForeignKey("BaiToanId")]
        public virtual BaiToan BaiToan { get; set; }
    }
}