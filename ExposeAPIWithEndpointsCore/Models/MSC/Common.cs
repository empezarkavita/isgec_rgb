using System;


namespace ExposeAPIWithEndpointsCore.MSC
{

    public static class Common
    {

        public static string getColorCode(string status)
        {
            string color = "";

            switch (status)
            {
                case Constants.Estimation:
                    color = Constants.RED;
                    break;
                case Constants.UnderRepair:
                    color = Constants.YELLOW;
                    break;
                default:
                    color = Constants.GREEN;
                    break;
            }
            return color;
        }



    }



}