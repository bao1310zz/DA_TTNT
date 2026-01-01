using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTNT_DAL.Models
{
    public class BuocDuyet
    {
        public int DinhId { get; set; }     
        public int TrangThai { get; set; }
        public string ThongBao { get; set; } 
        public int Mau { get; set; }        
        public bool IsReset { get; set; }  
        public bool IsKetLuan { get; set; }
        public string HanhDong { get; set; }
        public string MenhDeMoi { get; set; }
        public int TuDinh { get; set; }
    }
}