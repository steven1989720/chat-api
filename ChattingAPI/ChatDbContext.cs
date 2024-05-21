using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Formats.Asn1;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;

namespace ChattingAPI
{
    public class ChatDbContext : IDisposable
    {
        public class Message
        {
            public int Id { get; set; }
            public DateTime Timestamp { get; set; }
            public required string Content { get; set; }
        }

        /*
         DROP TABLE IF EXISTS public.messages;
         CREATE TABLE IF NOT EXISTS public.messages
         (
             id integer NOT NULL DEFAULT nextval('message_id_seq'::regclass),
             created_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
             data character varying(128) COLLATE pg_catalog."default",
             CONSTRAINT message_pkey PRIMARY KEY (id)
         )
         */

        private string? _connectionString;

        private NpgsqlConnection? _connection = null;

        private const int MAX_LENGTH = 128;
        private bool _disposed = false;

        public ChatDbContext(IConfiguration configuration)
        {
            _connectionString = configuration?.GetConnectionString("DefaultConnection");
        }

        // Destructor (Finalizer)
        ~ChatDbContext()
        {
            Dispose(false);
        }

        protected bool Connect()
        {
            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();

                NpgsqlCommand _createCmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS messages (id SERIAL PRIMARY KEY, created_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP, data character varying(128));", _connection);
                _createCmd?.ExecuteNonQuery();

                Log.Information("Connected to {connection}", _connectionString);
            }

            return _connection != null;
        }

        public void SaveMessage(string messageText)
        {
            if (Connect())
            {
                string msg = messageText.Trim();
                if (msg.Length > MAX_LENGTH)
                {
                    msg = msg[..MAX_LENGTH];
                }
                NpgsqlCommand _insertCmd = new NpgsqlCommand("INSERT INTO messages (data) VALUES (@text)", _connection);

                _insertCmd?.Parameters.AddWithValue("@text", msg);
                _insertCmd?.ExecuteNonQuery();
            }
        }

        public IEnumerable<Message> GetHistory(int period)
        {
            var messages = new List<Message>();
            if (Connect())
            {
                NpgsqlCommand historyCmd = new NpgsqlCommand($"SELECT * FROM messages WHERE created_date >= NOW() - INTERVAL '{period} MINUTE' ORDER BY id DESC", _connection);

                using (var reader = historyCmd.ExecuteReader())
                {
                    while (reader != null && reader.Read())
                    {
                        var message = new Message
                        {
                            Id = reader.GetInt32(0),
                            Timestamp = reader.GetDateTime(1),
                            Content = reader.GetString(2)
                        };
                        messages.Add(message);
                    }
                }
            }

            return messages;
        }

        // IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources (e.g., close database connection)
                    if (_connection != null)
                    {
                        _connection.Close();
                        _connection.Dispose(); // Dispose the connection

                        _connection = null;

                        Log.Information("Disconnected from {connection}", _connectionString);
                    }
                }

                // Dispose unmanaged resources
                // (none in this example, but if you have any, clean them up here)

                _disposed = true;
            }
        }
    }
}
