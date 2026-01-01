using System;
using System.Collections.Generic;
using System.Linq;
using TTNT_DAL.Models;
using Clause = System.Collections.Generic.HashSet<string>;

namespace TTNT_BUS
{
    public class GiaiThuatHopGiai
    {
        // Hàm chính: Nhận list chuỗi -> Trả về danh sách các bước chứng minh
        public (bool ThanhCong, List<BuocHopGiai> CacBuoc) ThucHienHopGiai(List<string> inputLines)
        {
            var steps = new List<BuocHopGiai>();
            var knowledgeBase = new List<Clause>();
            int stt = 1;

            // 1. Chuẩn hóa dữ liệu đầu vào
            foreach (var line in inputLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var c = ParseClause(line);
                knowledgeBase.Add(c);
                steps.Add(new BuocHopGiai
                {
                    STT = stt++,
                    HanhDong = "Giả thiết / Phủ định KL",
                    KetQua = ClauseToString(c),
                    IsRong = false
                });
            }

            // 2. Vòng lặp hợp giải
            bool foundNew = true;
            while (foundNew)
            {
                foundNew = false;
                int count = knowledgeBase.Count;

                // Duyệt tất cả các cặp mệnh đề (i, j)
                for (int i = 0; i < count; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        // Thử hợp giải 2 mệnh đề này
                        Clause ketQua = Resolve(knowledgeBase[i], knowledgeBase[j], out string trietTieu);

                        if (ketQua != null) // Nếu hợp giải được
                        {
                            // Kiểm tra xem mệnh đề mới này đã có trong KB chưa (tránh trùng lặp)
                            if (!ContainsClause(knowledgeBase, ketQua))
                            {
                                knowledgeBase.Add(ketQua);
                                foundNew = true; // Đánh dấu là có cái mới, cần chạy tiếp vòng lặp

                                bool isEmpty = ketQua.Count == 0;
                                steps.Add(new BuocHopGiai
                                {
                                    STT = stt++,
                                    HanhDong = $"Hợp giải ({i + 1}) và ({j + 1}) -> Triệt tiêu '{trietTieu.Replace("-", "")}'",
                                    KetQua = ClauseToString(ketQua),
                                    IsRong = isEmpty
                                });

                                if (isEmpty) return (true, steps); // Ra rỗng [] -> Chứng minh xong
                            }
                        }
                    }
                    if (foundNew) break; // Break để cập nhật lại count
                }
            }

            return (false, steps); // Chạy hết mà không ra [] -> Thất bại
        }

        // --- Logic cốt lõi: Hợp giải 2 tập hợp ---
        private Clause Resolve(Clause c1, Clause c2, out string literal)
        {
            literal = "";
            foreach (var lit in c1)
            {
                string doiNgau = GetDoiNgau(lit); // A -> -A
                if (c2.Contains(doiNgau))
                {
                    literal = lit;
                    // Tạo mệnh đề mới = (c1 + c2) - {lit, doiNgau}
                    Clause res = new Clause(c1);
                    res.UnionWith(c2);
                    res.Remove(lit);
                    res.Remove(doiNgau);
                    return res;
                }
            }
            return null; // Không có cặp đối ngẫu nào
        }

        // Helper: Lấy đối ngẫu (A -> -A; -A -> A)
        private string GetDoiNgau(string s) => s.StartsWith("-") ? s.Substring(1) : "-" + s;

        // Helper: Parse chuỗi nhập "A v B" thành HashSet
        private Clause ParseClause(string s)
        {
            var parts = s.Split(new char[] { ' ', 'v', 'V', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return new Clause(parts);
        }

        // Helper: In mệnh đề ra string
        private string ClauseToString(Clause c)
        {
            if (c.Count == 0) return "[] (Mệnh đề rỗng)";
            return "{ " + string.Join(" v ", c) + " }";
        }

        // Helper: Kiểm tra trùng lặp
        private bool ContainsClause(List<Clause> list, Clause target)
        {
            return list.Any(c => c.SetEquals(target));
        }
    }
}