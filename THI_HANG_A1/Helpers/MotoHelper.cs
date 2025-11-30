using System.Drawing;
using THI_HANG_A1.Models;

namespace THI_HANG_A1.Helpers
{
    public static class MotoHelper
    {
        public static Color GetMotoColor(Moto moto)
        {
            switch (moto.Status)
            {
                case 0xC1:   // READY
                    return Color.LightGreen;

                case 0xC2:   // RUNNING
                    return Color.Orange;

                case 0xC3:   // IDLE (rảnh)
                    return Color.LightGray;
                case 0xC4:   // contest1
                    return Color.Orange;
                case 0xC5:   // contest2
                    return Color.Orange;
                case 0xC6:   // contest3
                    return Color.Orange;
                case 0xC7:   // contest4
                    return Color.Orange;
                default:
                    return Color.White;
            }
        }

    }

}
