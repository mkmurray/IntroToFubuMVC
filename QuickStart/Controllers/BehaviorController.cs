using System;

namespace QuickStart.Controllers
{
    public class BehaviorController
    {
        public VariableOutputViewModel VariableOutput()
        {
            return new VariableOutputViewModel
            {
                Name = "JP",
                DateOfBirth = new DateTime(1978, 1, 1),
                Address = new Address
                {
                    Line1 = "123 Develop with Passion Lane",
                    StateCode = "BC",
                    ZipCode = "V0L 1J0"
                }
            };
        }

        public VariableOutputViewModel TransactionalVariableOutput()
        {
            return VariableOutput();
        }
    }

    public class VariableOutputViewModel
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }
        public string StateCode { get; set; }
        public string ZipCode { get; set; }
    }
}