namespace THI_HANG_A1.Models
{
    public class LoiViPham
    {
        public string HoTen { get; set; }
        public string SBD { get; set; }
        public string MaXeDaChon { get; set; }
        public string HangXe => "A1";
        public int DiemTru { get; set; }
        public string ThoiGianLoi { get; set; }
        public string ChiTietLoi { get; set; }
    }
}