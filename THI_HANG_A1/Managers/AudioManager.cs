using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using THI_HANG_A1.Models;

namespace THI_HANG_A1.Managers
{
    /// <summary>
    /// CHUYÊN GIA ÂM THANH
    /// Chỉ làm một việc: Phát âm thanh
    /// </summary>
    public class AudioManager
    {
        private const string SOUND_PATH = @"D:\THISATHACH\Dich";

        /// <summary>
        /// Sự kiện này được dùng để gửi log về Form1
        /// </summary>
        public event Action<string> OnLogMessage;

        /// <summary>
        /// Phát âm thanh (Bất đồng bộ - không chờ)
        /// </summary>
        public void PhatAmThanh(ThiSinh ts, string tenSuKien)
        {
            if (ts != null && !string.IsNullOrEmpty(ts.MaXeDaChon))
            {
                // Chạy trên luồng nền để không làm đơ UI
                Task.Run(() => PhatAmThanhSync(ts, tenSuKien));
            }
        }

        /// <summary>
        /// Phát âm thanh (Đồng bộ - CÓ CHỜ) và trả về Task
        /// </summary>
        public Task PhatAmThanhSyncTask(ThiSinh ts, string tenSuKien)
        {
            if (ts == null || string.IsNullOrEmpty(ts.MaXeDaChon))
            {
                return Task.CompletedTask;
            }
            return Task.Run(() => PhatAmThanhSync(ts, tenSuKien));
        }

        /// <summary>
        /// Lõi phát âm thanh, luôn chạy đồng bộ (PlaySync)
        /// </summary>
        private void PhatAmThanhSync(ThiSinh ts, string tenSuKien)
        {
            if (ts == null || string.IsNullOrEmpty(ts.MaXeDaChon)) return;

            string fileName = $"Xe{ts.MaXeDaChon}_{tenSuKien}.wav";
            string fullPath = Path.Combine(SOUND_PATH, fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    using (SoundPlayer soundPlayer = new SoundPlayer(fullPath))
                    {
                        soundPlayer.PlaySync();
                    }
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke($"LỖI PHÁT ÂM THANH {fileName}: {ex.Message}");
                }
            }
            else
            {
                OnLogMessage?.Invoke($"LỖI ÂM THANH: Không tìm thấy file {fileName}");
            }
        }
    }
}
