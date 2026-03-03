using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Domain
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public List<Invoice> Invoices { get; set; } = new();
    }
}
