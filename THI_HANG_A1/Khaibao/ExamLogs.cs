// Decompiled with JetBrains decompiler
// Type: RDSL.exam.ExamLogs
// Assembly: RDSL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 067811FF-510B-4CE3-AE37-8D7738DFDDA0
// Assembly location: C:\Users\admin\Downloads\SHLXA1 - Device Control and Monitor\SHLXA1 - Device Control and Monitor\RDSL.exe

using System;
using System.Collections.Generic;

namespace THI_HANG_A1
{
    
    public class ExamLogs
    {
        public  List<ExamLogs> elist = new List<ExamLogs>();
        public int ID { get; set; }

        public int ExamID { get; set; }

        public DateTime Time { get; set; }

        public ExamLogType Type { get; set; }

        public string Comment { get; set; }

        public string Username { get; set; }

        public string StringType
        {
            get
            {
                if (this.Type == ExamLogType.Initiallize)
                    return "Khởi tạo";
                if (this.Type == ExamLogType.EditExam || this.Type == ExamLogType.EditInfo)
                    return "Sửa thông tin";
                if (this.Type == ExamLogType.Export)
                    return "Trích xuất kết quả";
                if (this.Type == ExamLogType.Import)
                    return "Nhập dữ liệu";
                if (this.Type == ExamLogType.CancelExaminee)
                    return "Hủy kết quả";
                if (this.Type == ExamLogType.PrintManual)
                    return "In biên bản";
                if (this.Type == ExamLogType.RetakeExaminee)
                    return "Cho thi lại";
                return this.Type == ExamLogType.StartWithoutSensor ? "Cảnh báo cấp xe" : "Không xác định";
            }
        }

        //public static string Display_Time => Time.ToString("dd-MM-yyyy HH:mm");

        public void  Add(ExamLogs examLogs)
        {
            elist.Add(examLogs);
        }
    }
}
