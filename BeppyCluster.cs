using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BepInEx.Configuration;

namespace BeppyServer {
    // For every server that wants to be a part of a cluster of servers
    // we need to allow them to write either a file containing the cluster information
    // or specify runtime arguments

    // In doing so, we want to specify the following:
    //  Server Name
    //  Database Type
    //  Database IP
    //  Database Username
    //  Database Password

    public class BeppyCluster {
        private readonly string connectionString;

        private readonly SqlConnection dbConnection;
        private readonly ConfigEntry<string> dbIp;
        private readonly ConfigEntry<string> dbName;
        private readonly ConfigEntry<string> dbPassword;
        private readonly ConfigEntry<string> dbPort;
        private readonly ConfigEntry<string> dbUsername;
        private readonly ConfigEntry<bool> handlePermissions;

        private readonly ConfigEntry<string> serverName;

        public BeppyCluster(ConfigFile Config) {
            serverName = Config.Bind("Cluster", "LocalServerName", "7 Days to Die Server");
            dbName = Config.Bind("Cluster", "DatabaseName", "7D2D");
            dbIp = Config.Bind("Cluster", "DatabaseIP", "127.0.0.1");
            dbPort = Config.Bind("Cluster", "DatabasePort", "1433");
            dbUsername = Config.Bind("Cluster", "DatabaseUsername", "admin");
            dbPassword = Config.Bind("Cluster", "DatabasePassword", "Adm1n1strator");
            handlePermissions = Config.Bind("Cluster", "UseClusterPermissions", false);

            IsHandlingPermissions = handlePermissions.Value;

            connectionString
                = $"Server={dbIp.Value},{dbPort.Value};Database={dbName.Value};User ID={dbUsername.Value};Password={dbPassword.Value};";
            dbConnection = new SqlConnection(connectionString);
        }

        public bool IsHandlingPermissions { get; set; }

        public bool VerifyDatabase() {
            dbConnection.Open();
            if (dbConnection.State == ConnectionState.Broken || dbConnection.State == ConnectionState.Closed)
                throw new Exception("Could not connect to database.");

            SqlCommand command = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.TABLES", dbConnection);
            SqlDataReader reader = command.ExecuteReader();
            if (!reader.HasRows) {
                dbConnection.Close();
                return false;
            }

            List<string> tableNames = new List<string>();
            tableNames.Add(reader.GetString(0));
            if (!reader.Read()) {
                dbConnection.Close();
                return false;
            }

            tableNames.Add(reader.GetString(0));
            if (!reader.Read()) {
                dbConnection.Close();
                return false;
            }

            tableNames.Add(reader.GetString(0));

            // If we don't have all 3, report it.
            if (!(tableNames.ContainsCaseInsensitive("groups")
                  && tableNames.ContainsCaseInsensitive("users")
                  && tableNames.ContainsCaseInsensitive("cluster")))
                return false;

            dbConnection.Close();
            return true;
        }

        public void SetupDatabase() {
            SqlCommand command = new SqlCommand(@"
DROP TABLE Groups;
DROP TABLE Users;
DROP TABLE Cluster;

CREATE TABLE Groups (
	Name VARCHAR(20) NOT NULL UNIQUE,
	Permissions VARCHAR(255),

	PRIMARY KEY (Name)
);

CREATE TABLE Users (
	SteamID VARCHAR(20) NOT NULL UNIQUE,
	Name VARCHAR(20) NOT NULL UNIQUE,
	GroupName VARCHAR(20) NOT NULL,
	AdditionalPermissions VARCHAR(255),
	Banned BIT NOT NULL DEFAULT (0),
	LastPlayed DateTime NOT NULL DEFAULT GETDATE(),

	PRIMARY KEY (SteamID),
	FOREIGN KEY (GroupName) REFERENCES Groups(Name)
);

CREATE TABLE Cluster (
	Name VARCHAR(20) NOT NULL UNIQUE,
	IP VARCHAR(20) NOT NULL,
	Port VARCHAR(5),
	Votes int DEFAULT(0)

	PRIMARY KEY (Name)
);

INSERT INTO Groups (Name, Permissions)
VALUES ('Admin', 'all');

INSERT INTO Groups (Name, Permissions)
VALUES ('User', 'cmdHelp');

INSERT INTO Users (SteamID, Name, GroupName)
VALUES (0, 'Admin', 'Admin');
", dbConnection);

            command.CommandText
                += $"INSERT INTO Cluster(Name, IP, Port) VALUES({serverName.Value}, '0.0.0.0', '27015');";
            try {
                command.Connection.Open();
                int result = command.ExecuteNonQuery();
                if (result <= 0)
                    throw new DatabaseException($"Result was {result}");
            } catch (Exception e) {
                throw new DatabaseException("Could not create default schema", e);
            } finally {
                command.Connection.Close();
            }
        }

        private void Fetch() { }

        public class DatabaseException : Exception {
            public DatabaseException(string message) : base(message) { }
            public DatabaseException(string message, Exception inner) : base(message, inner) { }
        }
    }
}