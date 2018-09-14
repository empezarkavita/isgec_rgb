using System;


namespace ExposeAPIWithEndpointsCore.ISGEC
{

    public static class Common
    {

        public static string getColorCode(string movementDate, string sbNo)
        {
            string color = "";

            if (String.IsNullOrEmpty(movementDate) || movementDate == "00-00-0000")
            {
                color = Constants.RED;
            }
            else if (sbNo.Length == 0)
            {
                color = Constants.YELLOW;
            }
            else
            {
                color = Constants.GREEN;
            }


            return color;
        }



    }



}