﻿using System;
using db.connections;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using db.plants;
using db.gardens;
using db.users;

namespace db.groups
{

    public class Groups : Connect
    {
        //Constructor call to base case - selecting 'u_gardens' database
        public Groups() : base("plants") { }

        //This function needs to be called after each query function to close the connection
        public new void Close() { base.Close(); }

        //public wrapper for 'Select * FROM __' in SelectALL in Connection - return type may need to be changed after talking to Zack
        public MySqlDataReader ShowAll(string tableName) { return SelectAll(tableName); }

        //fix - include garden ID to so group names can be shared throughout gardens
        //This has a bug in it
        public string Convert(string userID, string name)
        {

            //constructing query
            string query = String.Format("SELECT groupID FROM hasGroups WHERE userID = '{0}' AND groupName = '{1}';", userID, name);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);

            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            string returnString = string.Empty;

            while (dataReader.Read())
            {
                returnString = dataReader.GetString("groupID");
            }

            Close();

            return returnString;
        }

        public void initNotifications(string userID, string name)
        {
            
        }

        //Checks if a group name exists for a user and returns a bool 
        public bool Exists(string userID, string groupName)
        {
            //Converting gardenName to gardenID
            string groupID = Convert(userID, groupName);

            string query = string.Format("SELECT * FROM hasGroups WHERE userID = '{0}' AND groupID = '{1}'", userID, groupID);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);

            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            bool returnBool = dataReader.HasRows;

            Close();

            if (returnBool == true)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public void updateNotification(IEnumerable<(string, string)> userIDAndpgName)
        {
            var now = DateTime.Now;
            var future = now.AddDays(2); // datetime

            Users update = new Users();

            foreach (var t in userIDAndpgName)
            {
                var (userID, pname) = t;
                update.UpdateDate(userID, pname, future);
            }
        }

        //pgName / token
        public List<(string, string, string)> getNotificationData()
        {
            var now = DateTime.Now;

            //return all rows where current date is less than now

            Users get = new Users();

            return get.getDates(now);
        }

        //Adds a hardware given the hardware ID and the gardengroup name for it to be added to
        public void AddHardware(string userID, string groupName, string MACaddress)
        {
            string groupID = Convert(userID, groupName);

            string query = String.Format("INSERT INTO hasHardware VALUES('{0}','{1}')", MACaddress, groupID);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();

            Close();

        }

        //Deletes a hardware given the hardware ID and the gardengroup name for it to be added to
        public bool DeleteHardware(string MACaddress)
        {
            string query = String.Format(" DELETE FROM hasHardware WHERE MACaddress = '{0}';", MACaddress);

            //Open connection
            Open();

            try
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                Close();
                //Query failed - no hardware to be deleted 
                return false;
            }

            Close();

            //Delete was successful
            return true;
        }

        //Deletes a hardware given the hardware ID and the gardengroup name for it to be added to
        public bool DeleteHardware_byGroup(string userName, string groupName)
        {

            var groupID = Convert(userName, groupName);

            string query = String.Format(" DELETE FROM hasHardware WHERE groupID = '{0}';", groupID);

            //Open connection
            Open();

            try
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                Close();
                //Query failed - no hardware to be deleted 
                return false;
            }

            Close();

            //Delete was successful
            return true;
        }

        //Returns a string hardwareID given a garden groupName
        public string GetHardwareID(string userID, string groupName)
        {
            //convert groupName into groupID
            string groupID = Convert(userID, groupName);

            //construct query
            string query = String.Format("SELECT hardwareID FROM hasHardware WHERE groupID = '{0}';", groupID);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);

            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            string returnString = string.Empty;

            while (dataReader.Read())
            {
                returnString = dataReader.GetString("hardwareID");
            }

            Close();

            return (returnString);
        }

        //Returns a list of MACaddresses that are registered for the given user
        public List<List<string>> ListHardware(string userName)
        {
            //constructing query
            string query = String.Format("SELECT pg.groupName, hh.hardwareID FROM hasHardware hh, hasGroups pg WHERE hh.groupID = pg.groupID AND pg.userID = '{0}';", userName);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);

            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            List<List<string>> returnList = new List<List<string>>();

            while (dataReader.Read())
            {
                // Index 1: groupName, Index 2: Hardwareaddress
                returnList.Add(new List<string> { dataReader.GetString("groupName"), dataReader.GetString("hardwareID") });
            }

            Close();

            return (returnList);
        }

        //Adds a plant group to a user given the userID and gardenName
        public void AddGroup(string userID, string gardenName, Guid groupID_g, string groupName)
        {
            //convert garden name to gardenID
            Gardens tempgarden = new Gardens();
            string gardenID = tempgarden.Convert(userID, gardenName);
            //convert groupID_g to string
            string groupID = groupID_g.ToString();

            //constructing query
            string query = String.Format("INSERT INTO hasGroups VALUES('{0}','{1}','{2}','{3}');", userID, gardenID, groupID, groupName); 

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();

            Close();
        }

        // Deletes a garden group from a garden
        public bool DeleteGroup(string userID, string gardenName, string groupName)
        {
            //convert garden name to gardenID
            Gardens tempgarden = new Gardens();
            string gardenID = tempgarden.Convert(userID, gardenName);
            //convert groupID_g to string
            string groupID = Convert(userID, groupName);

            //constructing query - deletes the group from the the garden
            string query_delete_plant = String.Format(" DELETE FROM hasPlants WHERE userID ='{0}' AND groupID = '{1}';", userID, groupID);
            string query_delete_hardware = String.Format(" DELETE FROM hasHardware WHERE groupID = '{0}';", groupID);
            string query_delete_group = String.Format(" DELETE FROM hasGroups WHERE userID ='{0}' AND  gardenID = '{1}' AND groupID = '{2}';", userID, gardenID, groupID);


            //Open connection
            Open();

            try
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query_delete_plant, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                Close();
                //Query failed - issue with deleting plant
                return false;
            }
            try
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query_delete_hardware, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                Close();
                //Query failed - no group to be deleted 
                return false;
            }
            try
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query_delete_group, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                Close();
                //Query failed - no group to be deleted 
                return false;
            }


            Close();

            //Delete was successful
            return true;
        }

        //This function adds the given plant to the given garden group for the given user
        public void AddPlant(string userID, string groupName, string plantName)
        {
            //converting to plantID - doing this to allow adding by plantName
            Plants tempPlant = new Plants();
            string plantID = tempPlant.Convert(plantName);

            //Convert groupName to groupID given the user
            string groupID = Convert(userID, groupName);

            //constructing query - adds the plant to the group or if it is already there increases count by one
            string query = String.Format("INSERT INTO hasPlants(userID, groupID, plantID) VALUES('{0}','{1}',{2}) ON DUPLICATE KEY UPDATE quantity = quantity + 1;", userID, groupID, plantID);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();

            Close();
        }

        //This function deletes a plant from a plant group
        public bool DeletePlant(string userID, string groupName, string plantName)
        {
            //converting to plantID - doing this to allow adding by plantName
            Plants tempPlant = new Plants();
            string plantID = tempPlant.Convert(plantName);

            //Convert groupName to groupID given the user
            string groupID = Convert(userID, groupName);

            //constructing query - deletes the plant from the the plant group
            string query = String.Format("DELETE FROM hasPlants WHERE userID ='{0}' AND  groupID = '{1}' AND plantID = {2};", userID, groupID, plantID);

            //Open connection
            Open();

            try
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                Close();
                //Query failed - no plant to be deleted 
                return false;
            }
            Close();

            //Delete was successful
            return true;
        }

        //This function returns a list of plant names inside a group
        public List<string> ListPlants(string userID, string groupName)
        {
            //constructing query
            string groupID = Convert(userID, groupName);
            string query = String.Format("SELECT p.name FROM masterPlants p, hasPlants h WHERE p.plantID = h.plantID AND groupID = '{0}';", groupID);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);

            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            List<string> returnList = new List<string>();

            while (dataReader.Read())
            {
                returnList.Add(dataReader.GetString("name"));
            }

            Close();

            return (returnList);
        }

        //This function returns a list of all plants a user has - duplicates removed
        public List<string> ListAllPlants(string userID)
        {
            //constructing query
            string query = String.Format("SELECT p.name FROM masterPlants p, hasPlants h WHERE p.plantID = h.plantID AND userID = '{0}';", userID);

            //Open connection
            Open();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);

            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            List<string> returnList = new List<string>();
            string temp;

            while (dataReader.Read())
            {
                //stores the current plant name
                temp = dataReader.GetString("name");
                //If this plant is not in the return list
                if(!returnList.Contains(temp))
                {
                    returnList.Add(temp);
                }
            }

            Close();

            return (returnList);
        }
    }
}