using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TTNT_DAL.Models;

namespace TTNT_DAL
{
    public class GraphRepository
    {
        private AppDbContext _db = new AppDbContext();

        // Lấy lịch sử (Giữ nguyên của bạn)
        public List<BaiToan> LayLichSu() => _db.BaiToans.OrderByDescending(p => p.NgayTao).ToList();

        // --- HÀM QUAN TRỌNG: ĐỌC FILE ---
        public (List<Dinh>, List<Canh>) DocDoThi(string filePath)
        {
            var listDinh = new List<Dinh>();
            var listCanh = new List<Canh>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length == 0) return (listDinh, listCanh);

                // Dòng 1: Số đỉnh
                int soDinh = int.Parse(lines[0].Trim());

                // 1. TẠO ĐỈNH (Xếp thành vòng tròn cho đẹp)
                int tamX = 200, tamY = 200, banKinh = 100;

                for (int i = 0; i < soDinh; i++)
                {
                    double angle = 2 * Math.PI * i / soDinh;
                    listDinh.Add(new Dinh
                    {
                        Id = i,
                        // Đặt tên là "1", "2"... thay vì "0", "1" để khớp với hình của bạn
                        Ten = (i + 1).ToString(),
                        X = (int)(tamX + banKinh * Math.Cos(angle)),
                        Y = (int)(tamY + banKinh * Math.Sin(angle)),
                        Mau = 0,
                        h = 0
                    });
                }

                // 2. TẠO CẠNH (Đọc ma trận kề)
                for (int i = 0; i < soDinh; i++)
                {
                    string[] values = lines[i + 1].Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 0; j < soDinh; j++)
                    {
                        if (j < values.Length)
                        {
                            int w = int.Parse(values[j]);

                            // Nếu trọng số > 0 nghĩa là có đường đi
                            if (w > 0)
                            {
                                Canh c = new Canh();
                                c.TuDinh = listDinh[i].Id;
                                c.DenDinh = listDinh[j].Id;
                                c.TrongSo = w;
                                // -------------------------------

                                listCanh.Add(c);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi đọc file: " + ex.Message);
            }

            return (listDinh, listCanh);
        }

        // --- CÁC HÀM DB (GIỮ NGUYÊN ĐỂ KHÔNG BÁO LỖI) ---
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