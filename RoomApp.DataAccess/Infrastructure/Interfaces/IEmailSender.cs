using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoomApp.DataAccess.Infrastructure.Interfaces
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message);


    }
}
