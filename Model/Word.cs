using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AhorcadoClient.Model
{
    public class Word
    {
        public int WordID { get; set; }
        public int CategoryID { get; set; }
        public int LanguageID { get; set; }
        public string word { get; set; }
        public string Description { get; set; }
        public int Difficult { get; set; }
    }

}
