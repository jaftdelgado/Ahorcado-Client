using System;

namespace AhorcadoClient.Model
{
    public class Player
    {
        public int PlayerID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime BirthDay { get; set; }

        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public byte[] ProfilePic { get; set; }

        public int TotalScore { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int? SelectedLanguageID { get; set; }
    }
}
