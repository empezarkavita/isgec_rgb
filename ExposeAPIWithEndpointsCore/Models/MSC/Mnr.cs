using System;

namespace ExposeAPIWithEndpointsCore.MSC
{
    public class MnrDBRequest
    {
        public string containerno;
        public string party;
        public string status;
        public string color;
    }

    public class MnrDBResponse
    {
        public string containerno;
        public string party;
        public string status;
        public string color;
        public string footer;
    }

    public class EMnr
    {
        public string containerno;
        public string party;
        public string status;
        public string color;
        public string footer;
    }

}