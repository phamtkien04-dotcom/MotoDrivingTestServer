using System;
using THI_HANG_A1.Managers;

namespace THI_HANG_A1.Models
{
    public class San
    {
        public event Action OnChanged;

        public string Name { get; private set; }
        public string IP { get; private set; }
        public int PORT { get; private set; }

        public SocketHandler socketConn;

        public San(string name, string ip, int port)
        {
            Name = name;
            IP = ip;
            PORT = port;

            socketConn = new SocketHandler();
            socketConn.OnDataReceivedBytes += SocketDataHandler;  // nhận dạng Frame byte[]
        }

        // ===================  UI STATES =========================
        private void TriggerUI() => OnChanged?.Invoke();

        public bool IsConnected { get; private set; }

        public bool OngHoi { get => _ongHoi; set { _ongHoi = value; TriggerUI(); } }
        private bool _ongHoi;
        public bool Sensor1 { get => _sensor1; set { _sensor1 = value; TriggerUI(); } }
        private bool _sensor1;
        public bool Sensor2 { get => _sensor2; set { _sensor2 = value; TriggerUI(); } }
        private bool _sensor2;
        public bool Sensor3 { get => _sensor3; set { _sensor3 = value; TriggerUI(); } }
        private bool _sensor3;
        public bool Sensor4 { get => _sensor4; set { _sensor4 = value; TriggerUI(); } }
        private bool _sensor4;
        public bool Sensor5 { get => _sensor5; set { _sensor5 = value; TriggerUI(); } }
        private bool _sensor5;
        public bool Sensor6 { get => _sensor6; set { _sensor6 = value; TriggerUI(); } }
        private bool _sensor6;
        public bool Sensor7 { get => _sensor7; set { _sensor7 = value; TriggerUI(); } }
        private bool _sensor7;
        public bool Sensor8 { get => _sensor8; set { _sensor8 = value; TriggerUI(); } }
        private bool _sensor8;

        public string Mes { get => _Mes; set { _Mes = value; TriggerUI(); } }
        private string _Mes;


        // =============================================================
        // CONNECT
        // =============================================================
        public void Connect()
        {
            if (!socketConn.Connect(IP, PORT))
            {
                IsConnected = false;
                TriggerUI();
                return;
            }

            IsConnected = true;
            TriggerUI();
        }

        public void Disconnect()
        {
            socketConn.Disconnect();
            IsConnected = false;
            TriggerUI();
        }

        // =============================================================
        // PARSE ESP32 FRAME (10 BYTE)
        // =============================================================
        private void SocketDataHandler(byte[] buffer, int len)
        {
            if (len < 10) return;
            if (buffer[0] != 0x30) return;           // START_BYTE ESP32
            if (buffer[9] != 0x31) return;           // STOP_BYTE

            byte key = buffer[1];       // trạng thái / event
            byte type = buffer[2];       // GET/SET 32/33
            byte id = buffer[3];       // esp32 ID

            // GIẢI 4 BYTES DATA => 8 sensor bit
            UInt32 value = (uint)(
                (buffer[4] << 24) |
                (buffer[5] << 16) |
                (buffer[6] << 8) |
                 buffer[7]
            );

            byte crc = buffer[8];
            byte crcCalc = CRC8(buffer, 0, 8);
            Mes = BitConverter.ToString(buffer, 0, len);

            if (crc != crcCalc)
            {
                Mes += " ❌ CRC FAIL";
                return;
            }

            // =========== TÁCH 8 CẢM BIẾN TRONG VALUE =============
            Sensor1 = (value & (1 << 0)) != 0;
            Sensor2 = (value & (1 << 1)) != 0;
            Sensor3 = (value & (1 << 2)) != 0;
            Sensor4 = (value & (1 << 3)) != 0;
            Sensor5 = (value & (1 << 4)) != 0;
            Sensor6 = (value & (1 << 5)) != 0;
            Sensor7 = (value & (1 << 6)) != 0;
            Sensor8 = (value & (1 << 7)) != 0;

            OngHoi = Sensor4;   // nếu bạn muốn sensor4 chính là ống hơi  

            TriggerUI();
        }

        // =============================================================
        // CRC8 giống ESP32
        // =============================================================
        private byte CRC8(byte[] data, int start, int len)
        {
            byte crc = 0x00;
            byte poly = 0x07;

            for (int i = start; i < len; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                {
                    bool msb = (crc & 0x80) != 0;
                    crc <<= 1;
                    if (msb) crc ^= poly;
                }
            }
            return crc;
        }
    }
}
