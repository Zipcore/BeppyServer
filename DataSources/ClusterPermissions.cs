using System;
using System.Collections.Generic;
using System.Data.Odbc;

namespace BeppyServer.DataSources {
    
    public class ClusterPermissions : IDataSource {
        private OdbcConnection dbConnection;
        private readonly string driver = "";

        public ClusterPermissions(string dbConnectionString) {
            string[] args = dbConnectionString.Split(';');
            foreach (var arg in args) {
                Console.Log(arg);
                if (arg.ContainsCaseInsensitive("driver") || arg.ContainsCaseInsensitive("dsn"))
                    driver = arg.Split('=')[1];
            }

            dbConnection = new OdbcConnection(dbConnectionString);
        }

        private bool VerifyDatabase() {
            try {
                OdbcCommand command = new OdbcCommand("SHOW TABLES", dbConnection);
                dbConnection.Open();

                OdbcDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                    return false;

                List<string> tableNames = new List<string>();

                while (reader.Read()) {
                    string name = reader.GetString(0);
                    tableNames.Add(name);
                }

                bool hasGroups = false;
                bool hasUsers = false;
                bool hasCluster = false;
                foreach (string tableName in tableNames) {
                    if (tableName.ContainsCaseInsensitive("groups"))
                        hasGroups = true;
                    if (tableName.ContainsCaseInsensitive("users"))
                        hasUsers = true;
                    if (tableName.ContainsCaseInsensitive("cluster"))
                        hasCluster = true;
                }

                if (!hasUsers || !hasGroups || !hasCluster)
                    return false;
            } catch (Exception e) {
                throw new Exception("Could not verify database.", e);
            } finally {
                dbConnection.Close();
            }

            return true;
        }

        private Dictionary<string, PermissionGroup> GetGroups() {
            Dictionary<string, PermissionGroup> groups = new Dictionary<string, PermissionGroup>();

            try {
                OdbcCommand command = new OdbcCommand("SELECT Name, Permissions FROM `groups`", dbConnection);
                dbConnection.Open();
                OdbcDataReader reader = command.ExecuteReader();

                while (reader.Read()) {
                    string name = reader.GetString(0);
                    string permissions = reader.GetString(1);

                    groups.Add(name, new PermissionGroup(permissions.Split(',')));
                }
            
                return groups;
            } catch (Exception e) {
                throw new Exception("Could not get groups from database.", e);
            } finally {
                dbConnection.Close();
            }
        }

        private List<Player> GetPlayers() {
            List<Player> players = new List<Player>();

            try {
                OdbcCommand command = new OdbcCommand(
                    "SELECT SteamID, Name, GroupName, Banned, LastPlayed, AdditionalPermissions FROM `users`");
                dbConnection.Open();
                OdbcDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    string steamId = reader.GetString(0);
                    string name = reader.GetString(1);
                    string groupName = reader.GetString(2);
                    bool banned = reader.GetBoolean(3);
                    DateTime lastPlayed = reader.GetDateTime(4);
                    string additionalPermissions = reader.GetString(5);
                    players.Add(new Player(
                        steamId,
                        name,
                        groupName,
                        banned,
                        lastPlayed,
                        additionalPermissions.Split(','))
                    );
                }
            } catch (Exception e) {
                throw new Exception("Could not get users from database", e);
            } finally {
                dbConnection.Close();
            }

            return players;
        }

        private void SaveGroups(Dictionary<string, PermissionGroup> groups) {
            try {
                dbConnection.Open();
            } catch (Exception e) {
                throw new Exception("Could not save groups");
            } finally {
                dbConnection.Close();
            }
        }

        private void SaveUsers(List<Player> players) {
            try {
                dbConnection.Open();
                
            } catch (Exception e) {
                throw new Exception("Could not save users");
            } finally {
                dbConnection.Close();
            }
        }
        
        private void Fetch() { }
        
        public Permissions Load() {
            if (!VerifyDatabase())
                throw new Exception("Could not verify cluster database.");
            
            Permissions permissions = new Permissions();
            try {
                permissions.Groups = GetGroups();
                permissions.Players = GetPlayers();
            } catch (Exception e) {
                throw new Exception("Could not load from cluster", e);
            }
            
            return permissions;
        }

        public void Save(Permissions permissions) {
            try {
                SaveGroups(permissions.Groups);
                SaveUsers(permissions.Players);
            } catch (Exception e) {
                // Create a backup json file
                FilePermissions.SaveToFile("backup_permissions.json", permissions);
                throw new PermissionException("Could not save permissions to cluster." +
                                              "A backup JSON file was created.", e);
            }
        }
    }
}