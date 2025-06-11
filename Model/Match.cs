using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AhorcadoClient.Model
{
    internal class Match
    {
        public int MatchID { get; set; }

        public int Player1 { get; set; }

        public int? Player2 { get; set; }

        public int WordID { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int StatusID { get; set; }
    }
}
