using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ImageUpload.Data
{
    public class ImageRepository
    {
        private readonly string _connectionString;
        public ImageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Add(Image image)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Images (ImagePath, Views, Password)
                                VALUES (@path, @views, @pass)
                                SELECT SCOPE_IDENTITY()";

            cmd.Parameters.AddWithValue("@path", image.ImagePath);
            cmd.Parameters.AddWithValue("@views", 0);
            cmd.Parameters.AddWithValue("@pass", image.Password);

            connection.Open();
            return (int)(decimal)cmd.ExecuteScalar();
        }

        public Image GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Images
                                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@id", id);

            connection.Open();

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new Image
            {
                Id = id,
                ImagePath = (string)reader["ImagePath"],
                Password = (string)reader["Password"],
                Views = (int)reader["Views"]
            };
        }

        public void IncreaseView(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            int num = GetViewCount(id);

            cmd.CommandText = @"UPDATE Images SET Views = @num
                                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@num", num + 1);

            connection.Open();

            cmd.ExecuteNonQuery();
        }

        public int GetViewCount(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT Views FROM Images
                                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@id", id);

            connection.Open();

            return (int)cmd.ExecuteScalar();
        }

    }
}
