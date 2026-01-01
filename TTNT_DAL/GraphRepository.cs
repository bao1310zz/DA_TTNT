using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TTNT_DAL.Models;

namespace TTNT_DAL
{
    public class GraphRepository
    {
        private AppDbContext _db = new AppDbContext();

        public List<BaiToan> LayLichSu() => _db.BaiToans.OrderByDescending(p => p.NgayTao).ToList();

        public BaiToan LayChiTiet(int id)
        {
            return _db.BaiToans
                      .Include(p => p.DanhSachDinh)
                      .Include(p => p.DanhSachCanh)
                      .FirstOrDefault(p => p.Id == id);
        }

        public void LuuBaiToan(BaiToan bt)
        {
            _db.BaiToans.Add(bt);
            _db.SaveChanges();
        }

        public void LuuCanh(int baiToanId, List<Canh> dsCanh)
        {
            var bt = _db.BaiToans.Find(baiToanId);
            if (bt != null)
            {
                if (bt.DanhSachCanh == null) bt.DanhSachCanh = new List<Canh>();
                foreach (var c in dsCanh) bt.DanhSachCanh.Add(c);
                _db.SaveChanges();
            }
        }

        public void CapNhatMau(int baiToanId, List<Dinh> dsDinh)
        {
            foreach (var d in dsDinh)
            {
                var dinhDB = _db.Dinhs.Find(d.Id);
                if (dinhDB != null) dinhDB.Mau = d.Mau;
            }
            _db.SaveChanges();
        }
    }
}