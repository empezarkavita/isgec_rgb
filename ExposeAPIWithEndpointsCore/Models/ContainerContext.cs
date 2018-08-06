using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ExposeAPIWithEndpointsCore.Models
{
    public class ContainerContext
    {
        public string ConnectionString { get; set; }

        public ContainerContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public List<Container> GetAllContainers()
        {
            List<Container> list = new List<Container>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from container_info", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Container()
                        {
                            id = Convert.ToInt32(reader["id"]),
                            sbno = reader["sbno"].ToString(),
                            containerno = reader["containerno"].ToString(),
                            shippingline = reader["shippingline"].ToString(),
                            movementdate = reader["movementdate"].ToString(),
                        });
                    }
                }
            }
            return list;
        }

        public void DeleteContainer(int id)
        {
            List<Container> list = new List<Container>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("delete from container_info where id =" + id, conn);
                cmd.ExecuteNonQuery();
            }

        }

        public void SaveContainer(Container data)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand( "insert into container_info (id,sbno,containerno,shippingline,movementdate) values (0,'"+ data.sbno + "','" + data.containerno +"','"+ data.shippingline+"','"+ data.movementdate +"');" , conn);
                cmd.ExecuteNonQuery();
            }

        }

        public void UpdateContainer(Container data)
        {
            using (MySqlConnection conn = GetConnection())
            {

                
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("update container_info set sbno='"+ data.sbno + "',containerno='"+ data.containerno + "',shippingline='"+ data.shippingline + "', movementdate='"+ data.movementdate + "' where id='"+ data.id + "';" , conn);
                cmd.ExecuteNonQuery();
            }

        }
    }
}