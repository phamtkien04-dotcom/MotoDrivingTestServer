using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using THI_HANG_A1.Models;

namespace THI_HANG_A1.Managers
{
    /// <summary>
    /// CHUYÊN GIA EXCEL
    /// Chỉ làm một việc: Đọc file Excel và trả về List
    /// </summary>
    public static class ExcelLoader
    {
        public static List<ThiSinh> NapDanhSach(string filePath)
        {
            var danhSach = new List<ThiSinh>();
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Bỏ qua hàng tiêu đề

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.Cell(1).GetValue<string>())) continue;

                    var ts = new ThiSinh
                    {
                        SBD = row.Cell(1).GetValue<string>(),
                        HoTen = row.Cell(2).GetValue<string>(),
                        KetquaLT = row.Cell(3).GetValue<string>(),
                        CCCD = row.Cell(5).GetValue<string>(),
                    };
                    if (DateTime.TryParse(row.Cell(4).Value.ToString(), out DateTime ns))
                    {
                        ts.NgaySinh = ns;
                    }
                    danhSach.Add(ts);
                }
            }
            return danhSach;
        }
    }
}
