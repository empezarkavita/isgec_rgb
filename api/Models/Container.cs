using System;

namespace ExposeAPIWithEndpointsCore.Models
{
    public class Container
    {

        public int id { get; set; }

        public string sbno { get; set; }

        public string containerno { get; set; }

        public string shippingline { get; set; }


        public string movementdate { get; set; }

        public string color { get; set; }

    }


    public class containerdetails
    {
        public string containerno;
        public string shippingline;
        public string sbno;
        public string movementdate;
        public string color;

        public string party;
    }

    public class msc
    {
         public string containerno;
         public string party;
         public string status;
         public string color;
         public string footer;

    }

}