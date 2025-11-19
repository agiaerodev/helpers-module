using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihelpers.Messages.Interfaces
{
    public interface IMessageProvider
    {
        public Task SendMessageAsync(string data, string topic);

    }
}
