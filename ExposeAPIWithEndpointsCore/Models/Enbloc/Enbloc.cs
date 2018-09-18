using Google.Cloud.Firestore;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    public class Enbloc
    {
        public string srl;
        public string container_no;
        public string container_type;
        public string wt;
        public string cargo;
        public string iso_code;
        public string seal_no_1;
        public string seal_no_2;
        public string seal_no_3;
        public string imdg_class;
        public string refer_temrature;
        public string oog_deatils;
        public string container_gross_details;
        public string cargo_description;
        public string bl_number;
        public string name;
        public string item_no;
        public string disposal_mode;

    }

    [FirestoreData]
    public class Yard
    {
        [FirestoreProperty]
        public string yard { get; set; }
        [FirestoreProperty]
        public string user { get; set; }
        [FirestoreProperty]
        public int total_capacity { get; set; }
        [FirestoreProperty]
        public int present_count { get; set; }
        [FirestoreProperty]
        public string present_color { get; set; }



    }


    [FirestoreData]
    public class YardCapacityDemo
    {
        [FirestoreProperty]
        public string yard { get; set; }
        [FirestoreProperty]
        public int total_capacity { get; set; }
        [FirestoreProperty]
        public int present_count { get; set; }
        [FirestoreProperty]
        public string present_color{ get; set; }

    }


}