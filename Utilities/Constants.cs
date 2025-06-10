using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhorcadoClient.Utilities
{
    public static class Constants
    {
        public static readonly string DEFAULT_PROFILE_PIC_PATH = "pack://application:,,,/Resources/Images/default-profile-pic.png";

        public static readonly int DEFAULT_WIDTH = 160;
        public static readonly int DEFAUL_HEIGHT = 160;
        public static readonly int DEFAULT_QUALITY = 90;
        public static readonly long MAX_IMAGE_SIZE = 200 * 1024;

        public static readonly string IMAGE_FILE_FILTER = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

        public static readonly string SAFE_PASSWORD_PATTERN = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,}$";
    }
}
