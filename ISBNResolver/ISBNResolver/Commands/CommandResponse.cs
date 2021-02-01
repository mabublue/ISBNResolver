using System.Collections.Generic;
using System.Linq;

namespace ISBNResolver.Commands
{
    public class CommandResponse
    {
        public CommandResponse()
        {
            Errors = new List<string>();
        }

        public IEnumerable<string> Errors { get; set; }

        public bool Success => !Errors.Any();
    }
}
