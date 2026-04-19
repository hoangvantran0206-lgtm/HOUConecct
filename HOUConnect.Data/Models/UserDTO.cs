using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOUConnect.Data.Models
{
    public class UserDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
