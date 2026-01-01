using TTNT_DAL;
using TTNT_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;



namespace BUS_TTNT
{
    public class XuLyDoThi
    {
        // Gọi xuống kho dữ liệu (DAL)
        private GraphRepository _khoDuLieu = new GraphRepository();

        // 1. Lấy lịch sử bài toán (để hiện lên Menu chọn bài cũ)
        public List<BaiToan> LayHetBaiToan()
        {
            return _khoDuLieu.LayLichSu();
        }

        // 2. Lấy chi tiết 1 bài toán (để vẽ lại lên màn hình)
        public BaiToan LayMotBai(int id)
        {
            return _khoDuLieu.LayChiTiet(id);
        }

        // =============================================================
        // HÀM 3: LƯU DỮ LIỆU TỪ MÀN HÌNH XUỐNG DATABASE
        // =============================================================
        public void LuuDuLieu(string tenBaiToan, List<Dinh> dsDinhTuGUI, List<Canh> dsCanhTuGUI)
        {
            // Tạo bài toán mới
            var baiToanMoi = new BaiToan
            {
                TenBaiToan = tenBaiToan,
                NgayTao = DateTime.Now,
                DanhSachDinh = new List<Dinh>()
            };

            // Dictionary để nhớ: ID tạm ở giao diện (1,2,3) ứng với Đỉnh nào trong Database?
            var banDoID = new Dictionary<int, Dinh>();

            // Xử lý danh sách Đỉnh
            foreach (var d in dsDinhTuGUI)
            {
                var dinhDB = new Dinh
                {
                    Ten = d.Ten,  // Giữ lại tên hiển thị (VD: "1", "2")
                    X = d.X,
                    Y = d.Y,
                    Mau = 0       // Mặc định chưa tô màu
                };

                baiToanMoi.DanhSachDinh.Add(dinhDB);
                banDoID[d.Id] = dinhDB; // Lưu ánh xạ: ID cũ -> Object mới
            }

            // Lưu BÀI TOÁN + ĐỈNH xuống DB trước để lấy ID thật
            _khoDuLieu.LuuBaiToan(baiToanMoi);

            // Xử lý danh sách Cạnh (Dùng ID thật vừa sinh ra từ DB)
            var dsCanhMoi = new List<Canh>();
            foreach (var c in dsCanhTuGUI)
            {
                // Kiểm tra xem 2 đầu mút có tồn tại trong bản đồ không
                if (banDoID.ContainsKey(c.TuDinh) && banDoID.ContainsKey(c.DenDinh))
                {
                    var canhDB = new Canh
                    {
                        TuDinh = banDoID[c.TuDinh].Id,  // Lấy ID thật
                        DenDinh = banDoID[c.DenDinh].Id // Lấy ID thật
                    };
                    dsCanhMoi.Add(canhDB);
                }
            }

            // Lưu CẠNH vào database
            _khoDuLieu.LuuCanh(baiToanMoi.Id, dsCanhMoi);
        }

        // =============================================================
        // HÀM 4: THUẬT TOÁN THAM LAM (WELSH-POWELL) - Chạy Nhanh
        // =============================================================
        public bool ChayThamLam(int baiToanId)
        {
            var baiToan = _khoDuLieu.LayChiTiet(baiToanId);
            if (baiToan == null) return false;

            var dsDinh = baiToan.DanhSachDinh.ToList();
            var dsCanh = baiToan.DanhSachCanh.ToList();

            // Sắp xếp: Đỉnh nào có nhiều cạnh nối nhất thì tô trước
            var dsDinhSapXep = dsDinh.OrderByDescending(d =>
                dsCanh.Count(c => c.TuDinh == d.Id || c.DenDinh == d.Id)
            ).ToList();

            foreach (var dinh in dsDinhSapXep)
            {
                var mauHangXom = new List<int>();

                // Tìm xem hàng xóm đang tô màu gì
                foreach (var c in dsCanh)
                {
                    int idHangXom = -1;
                    if (c.TuDinh == dinh.Id) idHangXom = c.DenDinh;
                    else if (c.DenDinh == dinh.Id) idHangXom = c.TuDinh;

                    if (idHangXom != -1)
                    {
                        var hangXom = dsDinh.FirstOrDefault(x => x.Id == idHangXom);
                        if (hangXom != null && hangXom.Mau > 0)
                            mauHangXom.Add(hangXom.Mau);
                    }
                }

                // Chọn màu nhỏ nhất (1, 2, 3...) mà chưa bị hàng xóm dùng
                int mauChon = 1;
                while (mauHangXom.Contains(mauChon)) mauChon++;

                dinh.Mau = mauChon;
            }

            // Lưu kết quả màu xuống DB
            _khoDuLieu.CapNhatMau(baiToanId, dsDinh);
            return true;
        }

        // =============================================================
        // HÀM 5: THUẬT TOÁN TỐI ƯU (BACKTRACKING) - Chạy Chính Xác
        // =============================================================
        public bool ChayToiUu(int baiToanId)
        {
            var baiToan = _khoDuLieu.LayChiTiet(baiToanId);
            if (baiToan == null) return false;

            var dsDinh = baiToan.DanhSachDinh.OrderBy(d => d.Id).ToList();
            var dsCanh = baiToan.DanhSachCanh.ToList();

            int n = dsDinh.Count;
            int[] mangMauTam = new int[n];

            // Thử số lượng màu m từ 1 tăng dần cho đến khi tìm được
            int soMauGioiHan = 1;
            while (true)
            {
                Array.Clear(mangMauTam, 0, n); // Reset mảng màu
                if (ThuToMauDeQuy(dsDinh, dsCanh, soMauGioiHan, mangMauTam, 0))
                {
                    break; // Tìm thấy rồi!
                }
                soMauGioiHan++; // Chưa được thì tăng số màu lên thử tiếp
            }

            // Gán kết quả vào danh sách Đỉnh
            for (int i = 0; i < n; i++) dsDinh[i].Mau = mangMauTam[i];

            _khoDuLieu.CapNhatMau(baiToanId, dsDinh);
            return true;
        }

        // Hàm đệ quy hỗ trợ
        private bool ThuToMauDeQuy(List<Dinh> dsDinh, List<Canh> dsCanh, int soMauMax, int[] mangMau, int indexDinh)
        {
            if (indexDinh == dsDinh.Count) return true; // Đã tô hết

            int idDinhDangXet = dsDinh[indexDinh].Id;

            for (int c = 1; c <= soMauMax; c++)
            {
                // Kiểm tra an toàn
                bool anToan = true;
                for (int i = 0; i < dsDinh.Count; i++)
                {
                    if (mangMau[i] == c) // Nếu đỉnh kia trùng màu
                    {
                        // Kiểm tra có cạnh nối không
                        bool coCanhNoi = dsCanh.Any(edge =>
                            (edge.TuDinh == idDinhDangXet && edge.DenDinh == dsDinh[i].Id) ||
                            (edge.DenDinh == idDinhDangXet && edge.TuDinh == dsDinh[i].Id)
                        );
                        if (coCanhNoi) { anToan = false; break; }
                    }
                }

                if (anToan)
                {
                    mangMau[indexDinh] = c;
                    if (ThuToMauDeQuy(dsDinh, dsCanh, soMauMax, mangMau, indexDinh + 1)) return true;
                    mangMau[indexDinh] = 0; // Quay lui (Backtrack)
                }
            }
            return false;
        }
    }
}