using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTNT_DAL.Models
{
    [Table("BaiToan")]
    public class BaiToan
    {
        [Key]
        public int Id { get; set; }
        public string TenBaiToan { get; set; }
        public DateTime NgayTao { get; set; }

        public virtual ICollection<Dinh> DanhSachDinh { get; set; }
        public virtual ICollection<Canh> DanhSachCanh { get; set; }
    }
}