using TTNT_DAL;
using TTNT_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;



namespace TTNT_BUS
{
    public class XuLyDoThi
    {
        private GraphRepository _khoDuLieu = new GraphRepository();
        public List<BaiToan> LayHetBaiToan()
        {
            return _khoDuLieu.LayLichSu();
        }

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
        public List<BuocDuyet> TimKiemINDO(List<Dinh> dsDinh, List<Canh> dsCanh, int startId, int endId)
        {
            List<BuocDuyet> kichBan = new List<BuocDuyet>();

            // INDO: Tăng dần giới hạn độ sâu (Limit)
            int maxDepth = dsDinh.Count + 2;
            bool timThay = false;

            for (int limit = 0; limit < maxDepth; limit++)
            {
                // Bước 1: Reset (Đặc trưng của INDO)
                kichBan.Add(new BuocDuyet
                {
                    IsReset = true,
                    ThongBao = $"--- [Lặp] Giới hạn độ sâu (Limit) = {limit} ---"
                });

                // Bước 2: Gọi đệ quy DLS
                List<int> currentPath = new List<int>();
                if (DLS(dsDinh, dsCanh, startId, endId, 0, limit, currentPath, kichBan))
                {
                    timThay = true;
                    break; // Tìm thấy thì dừng ngay
                }
                else
                {
                    kichBan.Add(new BuocDuyet
                    {
                        ThongBao = $"-> Limit {limit}: Không tìm thấy. Tăng độ sâu..."
                    });
                }
            }

            if (!timThay)
            {
                kichBan.Add(new BuocDuyet { ThongBao = "❌ KẾT THÚC: Không tìm thấy đường đi." });
            }

            return kichBan;
        }

        // Hàm phụ trợ đệ quy (DLS)
        private bool DLS(List<Dinh> dsDinh, List<Canh> dsCanh, int uId, int targetId, int depth, int limit, List<int> path, List<BuocDuyet> kichBan)
        {
            var u = dsDinh.FirstOrDefault(d => d.Id == uId);
            if (u == null) return false;

            path.Add(uId);

            // Ghi nhận bước đi: Đang xét u (Màu vàng = 1)
            kichBan.Add(new BuocDuyet
            {
                DinhId = uId,
                Mau = 1,
                ThongBao = $"   - Xét {u.Ten} (Độ sâu: {depth})"
            });

            // 1. Kiểm tra Đích
            if (uId == targetId)
            {
                kichBan.Add(new BuocDuyet
                {
                    DinhId = uId,
                    Mau = 2, // Màu đỏ
                    ThongBao = $"✅ TÌM THẤY ĐÍCH {u.Ten} tại độ sâu {depth}!"
                });
                return true;
            }

            // 2. Chạm giới hạn -> Dừng
            if (depth >= limit)
            {
                path.Remove(uId);
                return false;
            }

            // 3. Duyệt láng giềng
            var neighbors = dsCanh
                .Where(c => c.TuDinh == uId)
                .OrderBy(c => c.DenDinh)
                .Select(c => c.DenDinh).ToList();

            foreach (var vId in neighbors)
            {
                if (!path.Contains(vId)) // Tránh đi lặp
                {
                    if (DLS(dsDinh, dsCanh, vId, targetId, depth + 1, limit, path, kichBan))
                    {
                        // Khi đệ quy quay về mà báo tìm thấy -> Tô màu đỏ đường về
                        kichBan.Add(new BuocDuyet { DinhId = uId, Mau = 2, ThongBao = "" });
                        return true;
                    }
                }
            }

            // Backtrack
            path.Remove(uId);
            return false;
        }
        public List<BuocDuyet> ChayDFS(List<Dinh> dsDinh, List<Canh> dsCanh, int idBatDau)
        {
            var ketQua = new List<BuocDuyet>();
            var daDuyet = new HashSet<int>();

            // Stack lưu cặp: <Đỉnh Đang Xét, Đỉnh Cha>
            // Cha = -1 nghĩa là đỉnh đầu tiên, không có cha
            var stack = new Stack<Tuple<int, int>>();
            stack.Push(new Tuple<int, int>(idBatDau, -1));

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                int u = item.Item1;      // Đỉnh con
                int parent = item.Item2; // Đỉnh cha

                if (!daDuyet.Contains(u))
                {
                    daDuyet.Add(u);

                    // Ghi lại bước đi, kèm thông tin CHA để tí nữa vẽ đường nối
                    ketQua.Add(new BuocDuyet
                    {
                        DinhId = u,
                        TuDinh = parent,   // <--- Lưu cha vào đây
                        ThongBao = $"Duyệt đỉnh {u} (đi từ {parent})"
                    });

                    // Tìm hàng xóm
                    var dsKe = new List<int>();
                    foreach (var c in dsCanh)
                    {
                        if (c.TuDinh == u) dsKe.Add(c.DenDinh);
                        if (c.DenDinh == u) dsKe.Add(c.TuDinh);
                    }

                    // Sắp xếp giảm dần và đẩy vào Stack kèm theo CHA là u
                    var neighbors = dsKe.Where(x => !daDuyet.Contains(x))
                                        .OrderByDescending(x => x).ToList();
                    foreach (var v in neighbors)
                    {
                        stack.Push(new Tuple<int, int>(v, u)); // <--- Nhớ lưu u làm cha
                    }
                }
            }
            return ketQua;
        }
    }
}