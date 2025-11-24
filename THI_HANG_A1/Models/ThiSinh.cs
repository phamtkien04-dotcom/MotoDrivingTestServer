using System;

namespace THI_HANG_A1.Models
{
    public class ThiSinh
    {
        public string SBD { get; set; }
        public string HoTen { get; set; }
        public string KetquaLT { get; set; }
        public DateTime NgaySinh { get; set; }
        public string CCCD { get; set; }
        public string HangXe => "A1";
        public string MaXeDaChon { get; set; }
        public int DiemTongHop { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public int DiemThi { get; set; }
        public string ThoiGianThi { get; set; }
        public string KetQua { get; set; }
        public int LoiChamVach { get; set; }
        public int LoiChetMay { get; set; }
        public int LoiKhongXiNhan { get; set; }
        public int LoiNgaDo { get; set; }
        public int LoiChaySaiHinh { get; set; }
        public int LoiQuaTocDo { get; set; }
    }
}
