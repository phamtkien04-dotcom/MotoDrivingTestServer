using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using THI_HANG_A1.Models;

namespace THI_HANG_A1.Helpers
{
    public static class MotoHelper
    {
        public static Color GetMotoColor(Moto moto)
        {
            switch (moto.Mes)
            {
                case "READY": return Color.LightGreen;
                case "RUNNING": return Color.Orange;
                case "STOPPED": return Color.LightGray;
                case "OFFLINE": return Color.Red;
                default: return Color.White;
            }
        }
    }

}
