using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Data
    {
        private readonly string ServerString = "Data Source=HANSONG-PC;Initial Catalog=ConversationData;Integrated Security=True";

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool InsertMessage(MessageModel message)
        {
            using (SqlConnection conn = new SqlConnection(ServerString))
            {
                try
                {
                    conn.Open();
                    string insert = string.Format("insert MessagesTable(MessageText,UserId,CreateTime,MessageFile,Guid) values('{0}','{1}','{2}','{3}','{4}')", message.MessageText, message.UserId, message.CreateTime,message.MessageFile,message.Guid);
                    SqlCommand command = new SqlCommand(insert,conn);

                    int result = command.ExecuteNonQuery();
                    return result > 0 ? true : false;
                }
                catch (SqlException e)
                {

                }
            }
            return false;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DeleteMessage(int id)
        {
            using (SqlConnection conn = new SqlConnection(ServerString))
            {
                try
                {
                    conn.Open();
                    string insert = "delete MessagesTable where id=" + id;
                    SqlCommand command = new SqlCommand(insert, conn);

                    int result = command.ExecuteNonQuery();
                    return result > 0 ? true : false;
                }
                catch (SqlException e)
                {

                }
            }
            return false;
        }

        /// <summary>
        /// 查询所有记录
        /// </summary>
        /// <returns></returns>
        public List<MessageModel> NoParameterQueryMessages()
        {
            List<MessageModel> modellist = new List<MessageModel>();
            using (SqlConnection conn=new SqlConnection(ServerString))
            {
                try
                {
                    conn.Open();
                    string select = "select * from MessagesTable";
                    SqlCommand command = new SqlCommand(select, conn);

                    SqlDataReader sqlDataReader = command.ExecuteReader();                 
                    while (sqlDataReader.Read())
                    {
                        MessageModel model = new MessageModel();

                        model.MessageText = sqlDataReader["MessageText"].ToString();
                        model.UserId = sqlDataReader["UserId"].ToString();
                        model.Id = sqlDataReader["Id"].ToString();
                        model.CreateTime = DateTime.Parse(sqlDataReader["CreateTime"].ToString());
                        model.MessageFile = sqlDataReader["MessageFile"].ToString();
                        model.Guid= sqlDataReader["Guid"].ToString();

                        modellist.Add(model);
                    }
                }
                catch (SqlException e)
                {

                }
            }
            return modellist;
        }

        /// <summary>
        /// 根据userid查询
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<MessageModel> ParameterQueryMessage(string userid)
        {
            List<MessageModel> modellist = new List<MessageModel>();
            using (SqlConnection conn = new SqlConnection(ServerString))
            {
                try
                {
                    conn.Open();
                    string select = "select * from MessagesTable where UserId="+ userid;
                    SqlCommand command = new SqlCommand(select, conn);

                    SqlDataReader sqlDataReader = command.ExecuteReader();
                    MessageModel model = new MessageModel();
                    while (sqlDataReader.Read())
                    {
                        model.MessageText = sqlDataReader["MessageText"].ToString();
                        model.UserId = sqlDataReader["UserId"].ToString();
                        model.Id = sqlDataReader["Id"].ToString();
                        model.CreateTime = DateTime.Parse(sqlDataReader["CreateTime"].ToString());
                        model.Guid = sqlDataReader["Guid"].ToString();
                        model.MessageFile = sqlDataReader["MessageFile"].ToString();

                        modellist.Add(model);
                    }
                }
                catch (SqlException e)
                {

                }
            }
            return modellist;
        }
    }
}
