using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms; // Cần để dùng Application.OpenForms

namespace THI_HANG_A1.Managers
{
    
    public class SerialManager
    {
        private SerialPort _espPort;
        private List<byte> _receiveBuffer = new List<byte>();

        public event Action<byte[]> OnFrameReceived;
        public event Action<string> OnLogMessage;

        public bool IsOpen => _espPort != null && _espPort.IsOpen;

        public void KetNoiSerial(string portName, int baudRate)
        {
            if (IsOpen)
            {
                OnLogMessage?.Invoke("Cổng Serial đang mở, không kết nối lại.");
                return;
            }
            try
            {
                _espPort = new SerialPort(portName, baudRate);
                _espPort.DataReceived += new SerialDataReceivedEventHandler(EspPort_DataReceived);
                _espPort.Open();
                OnLogMessage?.Invoke($"Đã kết nối COM: {portName} thành công.");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"LỖI KẾT NỐI SERIAL: Không mở được {portName}. Chi tiết: {ex.Message}");
            }
        }

        public void NgatKetNoi()
        {
            if (IsOpen)
            {
                _espPort.Close();
                _espPort.Dispose();
                OnLogMessage?.Invoke("Đã ngắt kết nối Serial.");
            }
        }

        /// <summary>
        /// Gửi khung truyền (ĐÃ VIẾT LẠI để hỗ trợ payload)
        /// </summary>
        public void GuiKhungTruyen(byte key, byte set_get, byte maXe, byte[] payload)
        {
            if (!IsOpen)
            {
                OnLogMessage?.Invoke("LỖI GỬI: Cổng Serial chưa mở.");
                return;
            }

            // Lấy độ dài payload
            int payloadLength = (payload == null) ? 0 : payload.Length;

            // Tổng kích thước khung = 9 byte Header + n byte Payload + 1 byte Stop
            int totalFrameSize = SerialConstants.HEADER_LENGTH + payloadLength + 1;
            byte[] frame = new byte[totalFrameSize];

            // 1. Tạo Header (9 byte)
            frame[0] = SerialConstants.BYTE_START;
            frame[1] = key;
            frame[2] = set_get;
            frame[3] = maXe;

            // 2. Chuyển độ dài (int) thành 4 byte (Big-Endian) theo yêu cầu
            byte[] lenBytes = BitConverter.GetBytes(payloadLength);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes); // Đảo lại để thành Big-Endian (data3...data0)

            frame[4] = lenBytes[0]; // data3
            frame[5] = lenBytes[1]; // data2
            frame[6] = lenBytes[2]; // data1
            frame[7] = lenBytes[3]; // data0

            // 3. Tính CRC cho Header (byte 1 đến 7)
            frame[8] = TinhCRC(frame); // CRC

            // 4. Copy Payload (nếu có)
            if (payloadLength > 0)
            {
                payload.CopyTo(frame, SerialConstants.HEADER_LENGTH); // Copy payload vào sau header
            }

            // 5. Thêm byte STOP
            frame[totalFrameSize - 1] = SerialConstants.BYTE_STOP;

            // 6. Gửi
            try
            {
                _espPort.Write(frame, 0, frame.Length);
                OnLogMessage?.Invoke($"SENT: {BitConverter.ToString(frame).Replace("-", " ")}");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"LỖI GỬI KHUNG: {ex.Message}");
            }
        }


        private void EspPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!IsOpen) return;
            try
            {
                int bytesToRead = _espPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                _espPort.Read(buffer, 0, bytesToRead);
                _receiveBuffer.AddRange(buffer);

                // Phải Invoke về luồng UI chính để ExamManager xử lý
                var form1 = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                form1?.Invoke((MethodInvoker)delegate
                {
                    XuLyBufferNhan();
                });
            }
            catch (Exception ex)
            {
                // Bắt lỗi trên luồng Serial và log ra
                var form1 = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                form1?.Invoke((MethodInvoker)delegate
                {
                    OnLogMessage?.Invoke($"LỖI KHI ĐỌC SERIAL: {ex.Message}");
                });
            }
        }

        /// <summary>
        /// Xử lý buffer nhận (ĐÃ VIẾT LẠI HOÀN TOÀN)
        /// </summary>
        private void XuLyBufferNhan()
        {
            while (true)
            {
                // 1. Tìm byte START
                int startIndex = _receiveBuffer.FindIndex(b => b == SerialConstants.BYTE_START);

                if (startIndex == -1)
                {
                    _receiveBuffer.Clear();
                    return; // Hết việc
                }

                if (startIndex > 0)
                {
                    _receiveBuffer.RemoveRange(0, startIndex); // Xóa rác
                }

                // 2. Kiểm tra đủ Header (9 byte)
                if (_receiveBuffer.Count < SerialConstants.HEADER_LENGTH)
                {
                    return; // Chờ thêm
                }

                // 3. Đọc Header
                byte[] header = _receiveBuffer.Take(SerialConstants.HEADER_LENGTH).ToArray();

                // 4. Giải mã độ dài payload (từ byte 4-7)
                byte[] lenBytes = new byte[4] { header[4], header[5], header[6], header[7] };
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lenBytes); // Chuyển về Little-Endian để C# đọc

                int payloadLength = BitConverter.ToInt32(lenBytes, 0);

                // 5. Tính tổng kích thước khung
                int totalFrameSize = SerialConstants.HEADER_LENGTH + payloadLength + 1; // 9 + n + 1

                // 6. Kiểm tra đã nhận đủ toàn bộ khung chưa
                if (_receiveBuffer.Count < totalFrameSize)
                {
                    return; // Chưa đủ, chờ thêm
                }

                // 7. ĐÃ CÓ ĐỦ! Lấy toàn bộ khung
                byte[] fullFrame = _receiveBuffer.Take(totalFrameSize).ToArray();
                _receiveBuffer.RemoveRange(0, totalFrameSize); // Xóa khỏi buffer

                // 8. Xác thực (Validate) khung
                byte expectedCrc = TinhCRC(fullFrame); // Tính CRC trên header (byte 1-7)
                byte actualCrc = fullFrame[8];
                byte stopByte = fullFrame[totalFrameSize - 1];

                if (actualCrc != expectedCrc)
                {
                    OnLogMessage?.Invoke($"LỖI NHẬN: Khung lỗi CRC. Expected {expectedCrc:X2}, Actual {actualCrc:X2}. Frame: {BitConverter.ToString(fullFrame).Replace("-", " ")}");
                    continue; // Bỏ qua, xử lý phần còn lại của buffer
                }

                if (stopByte != SerialConstants.BYTE_STOP)
                {
                    OnLogMessage?.Invoke($"LỖI NHẬN: Khung lỗi STOP. Frame: {BitConverter.ToString(fullFrame).Replace("-", " ")}");
                    continue; // Bỏ qua
                }

                // 10. KHUNG HỢP LỆ! Kích hoạt sự kiện
                OnFrameReceived?.Invoke(fullFrame);
            }
        }

        /// <summary>
        /// Tính CRC cho 7 byte (từ Key [1] đến data0 [7])
        /// </summary>
        private byte TinhCRC(byte[] frameHeader)
        {
            byte crc = 0;
            for (int i = 1; i <= 7; i++)
            {
                crc += frameHeader[i];
            }
            return crc;
        }
    }

    /// <summary>
    /// Tách các hằng số Serial ra một nơi riêng (ĐÃ CẬP NHẬT)
    /// </summary>
    public static class SerialConstants
    {
        public const int HEADER_LENGTH = 9; // 9 byte Header (từ START đến CRC)
        public const byte BYTE_START = 0x30;
        public const byte BYTE_STOP = 0x31;
        public const byte BYTE_SET = 0x32;
        public const byte BYTE_GET = 0x33;

        // ... (Tất cả các hằng số KEY, CMD, và LỖI khác giữ nguyên) ...
        public const byte KEY_LENH_DIEU_KHIEN = 0xA0;
        public const byte CMD_BAT_DAU_THI = 0xA1;
        public const byte CMD_KET_THUC_THI = 0xA2;
        public const byte CMD_CHUAN_BI_THI = 0xA3;

        public const byte KEY_TRANG_THAI_DIEU_KHIEN = 0xC0;
        public const byte KEY_LOI_PHAN_HOI = 0xE0;

        public const byte LOI_DE_VACH_XUAT_PHAT = 0xE1;
        public const byte LOI_DE_VACH_CHUONG_NGAI = 0xE2;
        public const byte LOI_CHAM_CHAN = 0xE3;
        public const byte LOI_QUA_THOI_GIAN = 0xE4;
        public const byte LOI_DI_SAI_DUONG = 0xE5;
        public const byte LOI_DO_XE = 0xE6;
        public const byte LOI_DI_RA_NGOAI = 0xE7;
        public const byte LOI_TAT_MAY = 0xE8;
        public const byte LOI_KHONG_VI_DOI_MUI = 0xE9;
        public const byte LOI_XI_NHAN = 0xEA;
    }
}

