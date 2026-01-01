using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTNT_DAL.Models;

namespace TTNT_DAL.Models
{
    [Table("Canh")]
    public class Canh
    {
        [Key]
        public int Id { get; set; }
        public int TuDinh { get; set; }  // ID Đỉnh Gốc
        public int DenDinh { get; set; } // ID Đỉnh Đích
        public int TrongSo { get; set; }
        public int BaiToanId { get; set; }
        [ForeignKey("BaiToanId")]
        public virtual BaiToan BaiToan { get; set; }
    }
}