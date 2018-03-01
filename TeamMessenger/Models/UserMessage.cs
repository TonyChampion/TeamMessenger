using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TeamMessenger.Models
{
    public class UserMessage
    {
        public User User { get; set; }
        public string Message { get; set; }
        public DateTime DateTimeStamp { get; set; }
    }
}
