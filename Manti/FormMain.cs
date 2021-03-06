﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace Manti
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        // - - - - - - - - - - - MUST READ - - - - - - - - - - -
        //   Hello Viewer and welcome to Manti-TC Source Code!
        // - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // This project contains tons of code and functions.
        // For a better overview (especially in Visual Studio 14),
        // Do the command: CTRL + M + O, simultaneously.
        // Great feature, I know.
        // - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // The newest sourcecode can be found at GITHUB:
        // LINK: https://github.com/Heitx/Manti-TC

        #region Global

        #region Functions
        private void SetOfflineMode(bool enable)
        {
            FormMySQL.Offline = enable;

            // Search Buttons
            Button[] dButtons = new Button[]
            {
                    buttonAccountSearchSearch,
                    buttonCharacterSearchSearch,
                    buttonCreatureSearchSearch,
                    buttonQuestSearchSearch,
                    buttonGameObjectSearchSearch,
                    buttonItemSearchSearch
            };

            // Execute Buttons
            ToolStripSplitButton[] dStripButton = new ToolStripSplitButton[]
            {
                    toolStripSplitButtonAccountScriptUpdate,
                    toolStripSplitButtonCharacterScriptUpdate,
                    toolStripSplitButtonCreatureScriptUpdate,
                    toolStripSplitButtonQuestScriptUpdate,
                    toolStripSplitButtonGOScriptUpdate,
                    toolStripSplitButtonItemScriptUpdate
            };

            foreach (Button btn in dButtons)
            {
                btn.Enabled = !enable;
            }

            foreach (ToolStripSplitButton btn in dStripButton)
            {
                btn.Enabled = !enable;
            }
        }
        #region GlobalFunctions
        /// <summary>
        /// Function looks for a specific .CSV extension file and turns it into a DataTable (used for FormTools).
        /// </summary>
        /// <param name="csvName">The specific .CSV File</param>
        /// <param name="ID">What column the ID is</param>
        /// <param name="value">Where the value/name is in the CSV extension</param>
        /// <returns>DataTable with all the rows from the ID and Value</returns>
        private DataTable ReadExcelCSV(string csvName, int ID, int value, int value2 = 0)
        {
            var reader = new System.IO.StreamReader(@".\CSV\" + csvName + ".dbc.csv");
            var forgetFirst = true;

            var newTable = new DataTable();

            if (reader != null)
            {
                newTable.Columns.Add("id", typeof(string));
                newTable.Columns.Add("value", typeof(string));
                if (value2 != 0) { newTable.Columns.Add("value2", typeof(string)); }

                string line; string[] words;

                while ((line = reader.ReadLine()) != null)
                {
                    words = line.Split(';');

                    if (forgetFirst == false)
                    {
                        if (words.Length > value && words[value] != null)
                        {
                            DataRow newRow = newTable.NewRow();

                            // adds the id and value to the row
                            newRow["id"] = words[ID].Trim('"'); newRow["value"] = words[value].Trim('"');
                            // if value2 is above 0, add another column value
                            if (value2 != 0) { newRow["value2"] = words[value2].Trim('"'); }

                            newTable.Rows.Add(newRow);
                        }
                    }

                    forgetFirst = false;
                }

                reader.Close();
            }
            else
            {
                MessageBox.Show(csvName + " Could not been found in the CSV folder.\n It has to be same location as the program.", "File Directory : CSV ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return newTable;
        }
        /// <summary>
        /// It filters, if the character '%' is in the beginning or end of the string (value).
        /// If it is, it turns the SQL query to LIKE (a sort of containing method for MySQL).
        /// </summary>
        /// <param name="value">What the user is searching for</param>
        /// <param name="columnName">Column name for the query</param>
        /// <returns>Returns either LIKE or equal to the search</returns>
        private string DatabaseQueryFilter(string value, string columnName)
        {
            if (value != string.Empty)
            {
                if (value.Trim().StartsWith("%", StringComparison.InvariantCultureIgnoreCase) || value.Trim().EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = " AND " + columnName + " LIKE '" + value + "'";
                }
                else
                {
                    value = " AND " + columnName + " = '" + value + "'";
                }
            }

            return value;
        }
        /// <summary>
        /// Only used in search tabs. It checks if all the textboxes are empty.
        /// </summary>
        /// <param name="control">The selected control to check</param>
        /// <returns>Returns a boolean. Textboxes empty = true</returns>
        private bool CheckEmptyControls(Control control)
        {
            foreach (Control ct in control.Controls)
            {
                if (ct is TextBox || ct is ComboBox)
                {
                    if (ct.Text != "")
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// Converts all the ColumnHeader to string header (holds strings).
        /// Required to add new rows.
        /// </summary>
        /// <param name="datatable">The specific table to convert</param>
        /// <returns>Returns the transformed DataTable with string headers</returns>
        private DataTable ConvertColumnsToString(DataTable datatable)
        {
            var newTable = datatable.Clone();

            for (var i = 0; i < newTable.Columns.Count; i++)
            {
                if (newTable.Columns[i].DataType != typeof(string))
                {
                    newTable.Columns[i].DataType = typeof(string);
                }
            }

            foreach (DataRow row in datatable.Rows)
            {
                newTable.ImportRow(row);
            }

            return newTable;
        }
        /// <summary>
        /// Creates a popup, where the user can select only one row.
        /// </summary>
        /// <param name="formTitle">Changes the popup title</param>
        /// <param name="data">This sends the data to the listview/datagrid used in the popup</param>
        /// <param name="currentValue">Highlights the current value</param>
        /// <returns>It returns the selected /= current value (the ID)</returns>
        private string CreatePopupSelection(string formTitle, DataTable data, string currentValue)
        {
            var popupDialog = new FormPopup.FormPopupSelection();

            popupDialog.setFormTitle = formTitle;
            popupDialog.changeSelection = currentValue;
            popupDialog.setDataTable = data;
            popupDialog.Owner = this;
            popupDialog.ShowDialog();

            currentValue = (popupDialog.changeSelection == "") ? currentValue : popupDialog.changeSelection;
            popupDialog.Dispose();

            GC.Collect();
            this.Activate();
            return currentValue;
        }
        /// <summary>
        /// Same as the Selection popup, except it has checkboxes (multiple selections).
        /// </summary>
        /// <param name="formTitle">Changes the popup title</param>
        /// <param name="data">This sends the data to the listview/datagrid used in the popup</param>
        /// <param name="currentValue">Highlights the current value</param>
        /// <param name="bitMask">If the data is 2^n based (1, 2, 4, 8, 16 so on)</param>
        /// <returns>It returns the selected /= current value (the ID)</returns>
        private string CreatePopupChecklist(string formTitle, DataTable data, string currentValue, bool bitMask = false)
        {
            var popupDialog = new FormPopup.FormPopupCheckboxList();

            popupDialog.setFormTitle = formTitle;
            popupDialog.setDataTable = data;
            popupDialog.usedValue = (currentValue == string.Empty) ? "0" : currentValue;
            popupDialog.setBitmask = bitMask;
            popupDialog.Owner = this;
            popupDialog.ShowDialog();

            currentValue = (popupDialog.usedValue.ToString() == "") ? currentValue : popupDialog.usedValue.ToString();
            popupDialog.Dispose();

            GC.Collect();

            this.Activate();
            return currentValue;
        }
        /// <summary>
        /// Similar to selection and checklist popup, however, is it used for entities (items, creatures & gameobjects)
        /// </summary>
        /// <param name="currentValue">Highlights the current value</param>
        /// <param name="disableEntity">Used to disable or enable radiobuttons {items, creatures, gameobjects} in that order</param>
        /// <returns>It returns the selected /= current value (the ID)</returns>
        private string CreatePopupEntity(string currentValue, bool[] disableEntity, bool outputID = true)
        {
            var popupDialog = new FormPopup.FormPopupEntities();
            DataSet entities = new DataSet();

            popupDialog.changeSelection = (currentValue == "") ? "0" : currentValue;
            popupDialog.changeOutput = outputID;
            popupDialog.disableEntity = disableEntity;
            popupDialog.Owner = this;

            if (FormMySQL.Offline)
            {
                // Popup Entity follows order: 0: ID, 1: displayID, 2: Name
                entities.Tables.Add(ReadExcelCSV("ItemTemplate", 0, 2, 1));
                entities.Tables.Add(ReadExcelCSV("CreatureTemplate", 0, 1, 2));
                entities.Tables.Add(ReadExcelCSV("GameObjectTemplate", 0, 1, 2));
            }
            else
            {
                var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

                if (ConnectionOpen(connect))
                {
                    string query = "";
                    query += "SELECT entry, displayid, name FROM item_template ORDER BY entry ASC;";
                    query += "SELECT entry, modelid1, name FROM creature_template ORDER BY entry ASC;";
                    query += "SELECT entry, displayId, name FROM gameobject_template ORDER BY entry ASC;";

                    entities = DatabaseSearch(connect, query);

                    ConnectionClose(connect);
                }
            }

            popupDialog.setEntityTable = entities;
            popupDialog.ShowDialog();

            currentValue = (popupDialog.changeSelection == "") ? currentValue : popupDialog.changeSelection;

            entities.Dispose();
            popupDialog.Dispose();

            GC.Collect();

            this.Activate();
            return currentValue;
        }
        /// <summary>
        /// Generates SQL text used to execute. Is used in loot tables (see creature -> loot as a reference).
        /// This might change to string return instead.
        /// </summary>
        /// <param name="table">What database it generates for</param>
        /// <param name="dataGrid">The grid it has to loop through</param>
        /// <param name="output">The textbox it has to 'add' to</param>
        private void GenerateLootSQL(string table, DataGridView dataGrid, TextBox output)
        {
            string query = "DELETE FROM `" + table + "` WHERE entry = '" + dataGrid.Rows[0].Cells[0].Value.ToString() + "';";

            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (row.Cells[0].Value.ToString() != "")
                {
                    query += Environment.NewLine + "INSERT INTO `" + table + "` VALUES (";

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.OwningColumn.DataPropertyName != "name")
                        {
                            query += cell.Value.ToString() + ", ";
                        }
                    }

                    query += "0);";


                }
            }

            output.AppendText(query);
        }
        /// <summary>
        /// Generates a SQL File and saves it in the SQL folder.
        /// </summary>
        /// <param name="fileStart">The beginning of the fileName</param>
        /// <param name="fileName">FileName after fileStart (usually entry & name)</param>
        /// <param name="tb">The textbox to create from (text)</param>
        private void GenerateSQLFile(string fileStart, string fileName, TextBox tb)
        {
            // Save location / path
            string path = @".\SQL\" + fileStart + fileName + ".SQL";

            // Checks if the path file exists
            if (File.Exists(path))
            {
                // Creates a messagebox with a warning
                DialogResult dr = MessageBox.Show("File already exists.\n Replace it?", "Warning ...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // If the feedback is no, stop the program from running
                if (dr == DialogResult.No)
                {
                    return;
                }
            }
            else
            {
                DialogResult dr = MessageBox.Show("SQL folder does not exist. \nAutomatically create one for you?", "The folder 'SQL' could not been found.", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (dr == DialogResult.Yes)
                {
                    Directory.CreateDirectory(@".\SQL\");
                }
                else
                {
                    return;
                }
            }

            // Checks if textbox is empty OR fileName is empty
            if (tb.TextLength == 0 || fileName == string.Empty)
            {
                return;
            }

            // StreamWriter is used to write the SQL.
            StreamWriter sw = new StreamWriter(path);

            // Puts every line of the selected textbox in an array.
            int lineCount = tb.GetLineFromCharIndex(tb.Text.Length) + 1;

            for (var i = 0; i < lineCount; i++)
            {
                int startIndex = tb.GetFirstCharIndexFromLine(i);

                int endIndex = (i < lineCount - 1) ?
                    tb.GetFirstCharIndexFromLine(i + 1) : tb.Text.Length;

                sw.WriteLine(tb.Text.Substring(startIndex, endIndex - startIndex));
            }

            // Closes the StreamWriter.
            sw.Close();

        }
        /// <summary>
        /// Sets all textboxes in a control to '0'.
        /// Mostly/only used on 'new button' in most tabs.
        /// </summary>
        /// <param name="parent">The beginning (usually a tabpage)</param>
        private void DefaultValuesGenerate(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is TextBox)
                {
                    child.Text = "0";
                }
                else
                {
                    DefaultValuesGenerate(child);
                }
            }
        }
        /// <summary>
        /// Overrides the selected database with a value, if textbox don't need a '0'.
        /// An example could be item name, it needs to be empty.
        /// </summary>
        /// <param name="exclude">A tuple with two variables, the textbox to target and replacement string.</param>
        private void DefaultValuesOverride(List<Tuple<TextBox, string>> exclude)
        {
            foreach (var data in exclude)
            {
                data.Item1.Text = data.Item2.ToString();
            }
        }
        /// <summary>
        /// The function name says it all. Takes the selected/selections of a datagrid and creates a deleting SQL(s) to execute.
        /// </summary>
        /// <param name="gv">The tageting DataGridView</param>
        /// <param name="sqlTable">What table is it targeting in the database</param>
        /// <param name="uniqueIndex">The specific row to delete</param>
        /// <param name="output">The textbox to output the SQL</param>
        private void GenerateDeleteSelectedRow(DataGridView gv, string sqlTable, string uniqueIndex, TextBox output)
        {
            if (gv.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow gvR in gv.SelectedRows)
                {
                    output.AppendText("DELETE FROM `" + sqlTable + "` WHERE `" + uniqueIndex + "` = '" + gvR.Cells[0].Value.ToString() + "';");
                }
            }
        }
        /// <summary>
        /// Converts an unix stamp to a datatime used for calenders and readable.
        /// </summary>
        /// <param name="unixStamp">The unixstamp to convert</param>
        /// <returns>Datatime based on unix stamp</returns>
        private DateTime UnixStampToDateTime(double unixStamp)
        {
            var DateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime = DateTime.AddSeconds(unixStamp).ToLocalTime();

            return DateTime;
        }
        /// <summary>
        /// A reverse of the UnixStampToDateTime function.
        /// </summary>
        /// <param name="dateTime">A datatime to convert</param>
        /// <returns>Outputs an unixstamp based on the datatime</returns>
        private double DateTimeToUnixStamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1)).TotalSeconds;
        }
        #endregion
        #region DatabaseFunctions

        // Generates the string required to create a connection
        private static string DatabaseString(string database)
        {
            var builder = new MySqlConnectionStringBuilder();

            builder.Server = FormMySQL.Address;
            builder.UserID = FormMySQL.Username;
            builder.Password = FormMySQL.Password;
            builder.Port = FormMySQL.Port;
            builder.Database = database;

            return builder.ToString();
        }
        // Tries to open the connection between the program and database.
        private bool ConnectionOpen(MySqlConnection connect)
        {
            try
            {
                connect.Open();
                return true;
            }
            catch (MySqlException)
            {
                return false;
                throw;
            }
        }
        // Tries to close the connection between the program and database.
        private bool ConnectionClose(MySqlConnection connect)
        {
            try
            {
                connect.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "MySQL Error: " + ex.Number, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
        // Searching the database with a specific query, then saves in a DataSet.
        private DataSet DatabaseSearch(MySqlConnection connect, string sqlQuery)
        {
            var ds = new DataSet();

            if (connect.State == ConnectionState.Open)
            {
                var da = new MySqlDataAdapter(sqlQuery, connect);

                da.Fill(ds);
            }

            return ds;
        }
        // Updates the database with a specific query and returns the row affected.
        private int DatabaseUpdate(MySqlConnection connect, string sqlQuery)
        {
            if (connect.State == ConnectionState.Open && sqlQuery != "")
            {
                var query = new MySqlCommand(sqlQuery, connect);
                return query.ExecuteNonQuery();
            }

            return 0;
        }
        // Get all rows from a search, searches for items names. if itemid is false, it tries for item identifier.
        private DataTable DatabaseItemNameColumn(string table, string where, string id, int itemColumn, bool isItemID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            var searchTable = new DataTable();

            if (ConnectionOpen(connect))
            {
                // Create the query depending on the paramenters
                string query = "SELECT * FROM " + table + " WHERE " + where + " = '" + id + "';";

                // Searches in mySQL.
                var datatable = DatabaseSearch(connect, query);

                // Sets all the columns to string.
                searchTable = ConvertColumnsToString(datatable.Tables[0]);

                if (searchTable.Rows.Count != 0)
                {
                    // Adds a new column to the existing one, called 'name'.
                    searchTable.Columns.Add("name", typeof(string));

                    // Loops through all rows
                    for (int i = 0; i < searchTable.Rows.Count; i++)
                    {

                        if (isItemID)
                        {
                            searchTable.Rows[i]["name"] = DatabaseItemGetName(Convert.ToUInt32(searchTable.Rows[i][itemColumn]));
                        }
                        else
                        {
                            searchTable.Rows[i]["name"] = DatabaseItemGetName(DatabaseItemGetEntry(Convert.ToUInt32(searchTable.Rows[i][itemColumn])));
                        }

                    }
                }

                ConnectionClose(connect);
            }

            return searchTable;
        }
        // Gets the item entry with global item identifier.
        private uint DatabaseItemGetEntry(uint itemIdentifier)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseCharacters));

            if (ConnectionOpen(connect))
            {
                // Get the ItemID
                string instanceQuery = "SELECT itemEntry FROM item_instance WHERE guid = '" + itemIdentifier + "';";

                // Item_instance Table
                DataSet iiTable = DatabaseSearch(connect, instanceQuery);

                if (iiTable.Tables[0].Rows.Count != 0) { return Convert.ToUInt32(iiTable.Tables[0].Rows[0][0]); }
            }

            ConnectionClose(connect);
            return 0;
        }
        // Gets the item name from item entry.
        private string DatabaseItemGetName(uint itemEntry)
        {

            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {

                // Get the ItemID
                string nameQuery = "SELECT name FROM item_template WHERE entry = '" + itemEntry + "';";

                // item_template
                DataSet itTable = DatabaseSearch(connect, nameQuery);

                return (itTable.Tables[0].Rows.Count > 0) ? itTable.Tables[0].Rows[0][0].ToString() : "";
            }

            ConnectionClose(connect);

            return "";
        }

        #endregion
        #region DataTables
        private DataTable DataItemClass()
        {
            var iClass = new DataTable();
            iClass.Columns.Add("id", typeof(string));
            iClass.Columns.Add("name", typeof(string));

            iClass.Rows.Add(0, "Consumables");
            iClass.Rows.Add(1, "Container");
            iClass.Rows.Add(2, "Weapon");
            iClass.Rows.Add(3, "Gem");
            iClass.Rows.Add(4, "Armor");
            iClass.Rows.Add(5, "Reagent");
            iClass.Rows.Add(6, "Projectile");
            iClass.Rows.Add(7, "Trade Goods");
            iClass.Rows.Add(8, "Generic (OBSOLETE)");
            iClass.Rows.Add(9, "Recipe");
            iClass.Rows.Add(10, "Money (OBSOLETE)");
            iClass.Rows.Add(11, "Quiver");
            iClass.Rows.Add(12, "Quest");
            iClass.Rows.Add(13, "Key");
            iClass.Rows.Add(14, "Permanent (OBSOLETE)");
            iClass.Rows.Add(15, "Miscellaneous");
            iClass.Rows.Add(16, "Glyph");

            return iClass;
        }
        private DataTable DataItemSubclass(string classID)
        {
            var iSubclass = new DataTable();

            iSubclass.Columns.Add("id", typeof(string));
            iSubclass.Columns.Add("name", typeof(string));

            switch (classID)
            {
                case "0": // Consumable
                    iSubclass.Rows.Add(0, "Consumbable");
                    iSubclass.Rows.Add(1, "Potion");
                    iSubclass.Rows.Add(2, "Elixir");
                    iSubclass.Rows.Add(3, "Flask");
                    iSubclass.Rows.Add(4, "Scroll");
                    iSubclass.Rows.Add(5, "Food & Drink");
                    iSubclass.Rows.Add(6, "Item Enhancement");
                    iSubclass.Rows.Add(7, "Bandage");
                    iSubclass.Rows.Add(8, "Other");
                    break;
                case "1": // Container
                    iSubclass.Rows.Add(0, "Bag");
                    iSubclass.Rows.Add(1, "Soul Bag");
                    iSubclass.Rows.Add(2, "Herb Bag");
                    iSubclass.Rows.Add(3, "Enchanting Bag");
                    iSubclass.Rows.Add(4, "Engineering Bag");
                    iSubclass.Rows.Add(5, "Gem Bag");
                    iSubclass.Rows.Add(7, "Mining Bag");
                    iSubclass.Rows.Add(8, "Inscription Bag");
                    break;
                case "2": // Weapon
                    iSubclass.Rows.Add(0, "Axe (one-hand)");
                    iSubclass.Rows.Add(1, "Axe (two-hand)");
                    iSubclass.Rows.Add(2, "Bow");
                    iSubclass.Rows.Add(3, "Gun");
                    iSubclass.Rows.Add(4, "Mace (one-hand)");
                    iSubclass.Rows.Add(5, "Mace (two-hand)");
                    iSubclass.Rows.Add(6, "Polearm");
                    iSubclass.Rows.Add(7, "Sword (one-hand)");
                    iSubclass.Rows.Add(8, "Sword (two-hand)");
                    iSubclass.Rows.Add(9, "Obsolete");
                    iSubclass.Rows.Add(10, "Staff");
                    iSubclass.Rows.Add(11, "Exotic");
                    iSubclass.Rows.Add(12, "Exotic");
                    iSubclass.Rows.Add(13, "Fist Weapon");
                    iSubclass.Rows.Add(14, "Miscellaneous");
                    iSubclass.Rows.Add(15, "Dagger");
                    iSubclass.Rows.Add(16, "Thrown");
                    iSubclass.Rows.Add(17, "Spear");
                    iSubclass.Rows.Add(18, "Crossbow");
                    iSubclass.Rows.Add(19, "Wand");
                    iSubclass.Rows.Add(20, "Fishing Pole");
                    break;
                case "3": // Gem
                    iSubclass.Rows.Add(0, "Red");
                    iSubclass.Rows.Add(1, "Blue");
                    iSubclass.Rows.Add(2, "Yellow");
                    iSubclass.Rows.Add(3, "Purple");
                    iSubclass.Rows.Add(4, "Green");
                    iSubclass.Rows.Add(5, "Orange");
                    iSubclass.Rows.Add(7, "Meta");
                    iSubclass.Rows.Add(8, "Simple");
                    iSubclass.Rows.Add(9, "Prismatic");
                    break;
                case "4": // Armor
                    iSubclass.Rows.Add(0, "Miscellaneous");
                    iSubclass.Rows.Add(1, "Cloth");
                    iSubclass.Rows.Add(2, "Leather");
                    iSubclass.Rows.Add(3, "Mail");
                    iSubclass.Rows.Add(4, "Plate");
                    iSubclass.Rows.Add(5, "Buckler (OBSOLETE)");
                    iSubclass.Rows.Add(6, "Shield");
                    iSubclass.Rows.Add(7, "Libram");
                    iSubclass.Rows.Add(8, "Idol");
                    iSubclass.Rows.Add(9, "Totel");
                    iSubclass.Rows.Add(10, "Sigil");
                    break;
                case "5": // Reagent
                    iSubclass.Rows.Add(0, "Reagent");
                    break;
                case "6": // Projectile
                    iSubclass.Rows.Add(0, "Wand (OBSOLETE)");
                    iSubclass.Rows.Add(1, "Bolt (OBSOLETE)");
                    iSubclass.Rows.Add(2, "Arrow");
                    iSubclass.Rows.Add(3, "Bullet");
                    iSubclass.Rows.Add(4, "Thrown (OBSOLETE)");
                    break;
                case "7": // Trade Goods
                    iSubclass.Rows.Add(0, "Trade Goods");
                    iSubclass.Rows.Add(1, "Parts");
                    iSubclass.Rows.Add(2, "Explosives");
                    iSubclass.Rows.Add(3, "Devices");
                    iSubclass.Rows.Add(4, "Jewelcrafting");
                    iSubclass.Rows.Add(5, "Cloth");
                    iSubclass.Rows.Add(6, "Leather");
                    iSubclass.Rows.Add(7, "Metal & Stone");
                    iSubclass.Rows.Add(8, "Meat");
                    iSubclass.Rows.Add(9, "Herb");
                    iSubclass.Rows.Add(10, "Elemental");
                    iSubclass.Rows.Add(11, "Other");
                    iSubclass.Rows.Add(12, "Enchanting");
                    iSubclass.Rows.Add(13, "Materials");
                    iSubclass.Rows.Add(14, "Armor Enchantment");
                    iSubclass.Rows.Add(15, "Weapon Enchantment");
                    break;
                case "8": // Generic (OBSOLETE)
                    iSubclass.Rows.Add(0, "Generic (OBSOLETE)");
                    break;
                case "9": // Recipe
                    iSubclass.Rows.Add(0, "Book");
                    iSubclass.Rows.Add(1, "Leatherworking");
                    iSubclass.Rows.Add(2, "Tailoring");
                    iSubclass.Rows.Add(3, "Engineering");
                    iSubclass.Rows.Add(4, "Blacksmithing");
                    iSubclass.Rows.Add(5, "Cooking");
                    iSubclass.Rows.Add(6, "Alchemy");
                    iSubclass.Rows.Add(7, "First Aid");
                    iSubclass.Rows.Add(8, "Enchanting");
                    iSubclass.Rows.Add(9, "Fishing");
                    iSubclass.Rows.Add(10, "Jewelcrafting");
                    break;
                case "10": // Money (OBSOLETE)
                    iSubclass.Rows.Add(0, "Money");
                    break;
                case "11": // Quiver
                    iSubclass.Rows.Add(0, "Quiver (OBSOLETE)");
                    iSubclass.Rows.Add(1, "Quiver (OBSOLETE)");
                    iSubclass.Rows.Add(2, "Quiver (can hold arrows)");
                    iSubclass.Rows.Add(3, "Ammo Pouch (can hold bullets)");
                    break;
                case "12": // Quest
                    iSubclass.Rows.Add(0, "Quest");
                    break;
                case "13": // Key
                    iSubclass.Rows.Add(0, "Key");
                    iSubclass.Rows.Add(1, "Lockpick");
                    break;
                case "14": // Permanent (OBSOLETE)
                    iSubclass.Rows.Add(0, "Permanent");
                    break;
                case "15": // Miscellaneous
                    iSubclass.Rows.Add(0, "Junk");
                    iSubclass.Rows.Add(1, "Reagent");
                    iSubclass.Rows.Add(2, "Pet");
                    iSubclass.Rows.Add(3, "Holiday");
                    iSubclass.Rows.Add(4, "Other");
                    iSubclass.Rows.Add(5, "Mount");
                    break;
                case "16": // Glyph
                    iSubclass.Rows.Add(1, "Warrior");
                    iSubclass.Rows.Add(2, "Paladin");
                    iSubclass.Rows.Add(3, "Hunter");
                    iSubclass.Rows.Add(4, "Rogue");
                    iSubclass.Rows.Add(5, "Priest");
                    iSubclass.Rows.Add(6, "Death Knight");
                    iSubclass.Rows.Add(7, "Shaman");
                    iSubclass.Rows.Add(8, "Mage");
                    iSubclass.Rows.Add(9, "Warlock");
                    iSubclass.Rows.Add(11, "Druid");
                    break;
            }

            return iSubclass;
        }
        #endregion
        #endregion
        #region Events
        private void FormMain_Load(object sender, EventArgs e)
        {
            tabControlCategory.Focus();

            this.Icon = Properties.Resources.iconManti;

            dataGridViewCharacterInventory.AutoGenerateColumns = false;

            dataGridViewItemLoot.AutoGenerateColumns = false;
            dataGridViewItemProspect.AutoGenerateColumns = false;
            dataGridViewItemMill.AutoGenerateColumns = false;
            dataGridViewItemDE.AutoGenerateColumns = false;

            dataGridViewQuestGivers.AutoGenerateColumns = false;
            
            SetOfflineMode(FormMySQL.Offline);

            var textboxToolTip = new ToolTip();

            textboxToolTip.InitialDelay = 100;
            textboxToolTip.ShowAlways = true;

            textboxToolTip.SetToolTip(textBoxAccountSearchID, "Account Identifier.");

        }
        private void tabControlCategory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                if (tabControlCategory.SelectedTab == tabPageAccount) // Account Tab
                {
                    if (tabControlCategoryAccount.SelectedTab == tabPageAccountSearch)
                    {
                        buttonAccountSearchSearch_Click(this, new EventArgs());
                    }
                }
                else if (tabControlCategory.SelectedTab == tabPageCharacter) // Character Tab
                {
                    if (tabControlCategoryCharacter.SelectedTab == tabPageCharacterSearch)
                    {
                        buttonCharacterSearchSearch_Click(this, new EventArgs());
                    }
                }
                else if (tabControlCategory.SelectedTab == tabPageCreature) // Creature Tab
                {
                    if (tabControlCategoryCreature.SelectedTab == tabPageCreatureSearch)
                    {
                        buttonCreatureSearchSearch_Click(this, new EventArgs());
                    }
                }
                else if (tabControlCategory.SelectedTab == tabPageQuest) // Quest Tab
                {
                    if (tabControlCategoryQuest.SelectedTab == tabPageQuestSearch)
                    {
                        buttonQuestSearchSearch_Click(this, new EventArgs());
                    }
                }
                else if (tabControlCategory.SelectedTab == tabPageGameObject) // Game Object Tab
                {
                    if (tabControlCategoryGameObject.SelectedTab == tabPageGameObjectSearch)
                    {
                        buttonGameObjectSearchSearch_Click(this, new EventArgs());
                    }
                }
                else if (tabControlCategory.SelectedTab == tabPageItem) // Item Tab
                {
                    if (tabControlCategoryItem.SelectedTab == tabPageItemSearch)
                    {
                        buttonItemSearchSearch_Click(this, new EventArgs());
                    }
                }
            }
        }
        #region ToolStrip
        private void newConnectionToolStripMenuItemFile_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ExecutablePath);
            Application.Exit();
        }
        private void offlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FormMySQL.Offline)
            {
                SetOfflineMode(false);
            }
            else
            {
                SetOfflineMode(true);
            }
        }
        private void controlPanelToolStripMenuTools_Click(object sender, EventArgs e)
        {
            Form CP = new FormTools.FormControlPanel();

            CP.StartPosition = FormStartPosition.CenterScreen;
            CP.Show();
        }
        private void aboutToolStripMenuHelp_Click(object sender, EventArgs e)
        {
            var fa = new FormAbout();
            fa.ShowDialog();
        }
        #endregion
        #endregion

        #endregion

        #region Account

        #region Functions
        // Searches the data for a specific account.
        private void DatabaseAccountSearch(string accountID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseAuth));

            if (ConnectionOpen(connect))
            {
                string accountQuery = "SELECT * FROM `account` WHERE id='" + accountID + "';";
                string banQuery = "SELECT * FROM `account_banned` WHERE id='" + accountID + "';";
                string muteQuery = "SELECT * FROM `account_muted` WHERE guid='" + accountID + "';";
                string accessQuery = "SELECT * FROM `account_access` WHERE id='" + accountID + "';";

                string finalQuery = accountQuery + banQuery + muteQuery + accessQuery;

                DataSet AccountTable = DatabaseSearch(connect, finalQuery);

                // account data
                if (AccountTable.Tables[0].Rows.Count != 0)
                {
                    textBoxAccountAccountID.Text = AccountTable.Tables[0].Rows[0]["id"].ToString();
                    textBoxAccountAccountUsername.Text = AccountTable.Tables[0].Rows[0]["username"].ToString();
                    textBoxAccountAccountEmail.Text = AccountTable.Tables[0].Rows[0]["email"].ToString();
                    textBoxAccountAccountRegmail.Text = AccountTable.Tables[0].Rows[0]["reg_mail"].ToString();
                    textBoxAccountAccountJoindate.Text = AccountTable.Tables[0].Rows[0]["joindate"].ToString();
                    textBoxAccountAccountLastIP.Text = AccountTable.Tables[0].Rows[0]["last_ip"].ToString();
                    checkBoxAccountAccountLocked.Checked = Convert.ToBoolean(AccountTable.Tables[0].Rows[0]["locked"]);
                    checkBoxAccountAccountOnline.Checked = Convert.ToBoolean(AccountTable.Tables[0].Rows[0]["online"]);
                    textBoxAccountAccountExpansion.Text = AccountTable.Tables[0].Rows[0]["expansion"].ToString();
                }

                // ban data
                if (AccountTable.Tables[1].Rows.Count != 0)
                {
                    monthCalendarAccountAccountBanDate.AddMonthlyBoldedDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[1].Rows[0]["bandate"])));
                    monthCalendarAccountAccountBanDate.SetDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[1].Rows[0]["bandate"])));
                    monthCalendarAccountAccountUnbanDate.AddMonthlyBoldedDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[1].Rows[0]["unbandate"])));
                    monthCalendarAccountAccountUnbanDate.SetDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[1].Rows[0]["unbandate"])));

                    textBoxAccountAccountBandate.Text = UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[1].Rows[0]["bandate"])).ToString();
                    textBoxAccountAccountUnbandate.Text = UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[1].Rows[0]["unbandate"])).ToString();
                    textBoxAccountAccountBanreason.Text = AccountTable.Tables[1].Rows[0]["banreason"].ToString();
                    textBoxAccountAccountBannedby.Text = AccountTable.Tables[1].Rows[0]["bannedby"].ToString();
                    checkBoxAccountAccountBanActive.Checked = Convert.ToBoolean(AccountTable.Tables[1].Rows[0]["active"]);
                }

                // mute data
                if (AccountTable.Tables[2].Rows.Count != 0)
                {
                    monthCalendarAccountAccountMuteDate.AddMonthlyBoldedDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[2].Rows[0]["mutedate"])));
                    monthCalendarAccountAccountMuteDate.SetDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[2].Rows[0]["mutedate"])));
                    monthCalendarAccountAccountUnmuteDate.AddMonthlyBoldedDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[2].Rows[0]["mutedate"])).AddMinutes(Convert.ToDouble(AccountTable.Tables[2].Rows[0]["mutetime"])));
                    monthCalendarAccountAccountUnmuteDate.SetDate(UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[2].Rows[0]["mutedate"])).AddMinutes(Convert.ToDouble(AccountTable.Tables[2].Rows[0]["mutetime"])));

                    textBoxAccountAccountMutedate.Text = UnixStampToDateTime(Convert.ToDouble(AccountTable.Tables[2].Rows[0]["mutedate"])).ToString();
                    textBoxAccountAccountMutetime.Text = AccountTable.Tables[2].Rows[0]["mutetime"].ToString();
                    textBoxAccountAccountMutereason.Text = AccountTable.Tables[2].Rows[0]["mutereason"].ToString();
                    textBoxAccountAccountMutedby.Text = AccountTable.Tables[2].Rows[0]["mutedby"].ToString();
                }

                // acces data
                if (AccountTable.Tables[3].Rows.Count != 0)
                {
                    dataGridViewAccountAccess.Rows.Clear();

                    foreach (DataRow row in AccountTable.Tables[3].Rows)
                    {
                        dataGridViewAccountAccess.Rows.Add(row.ItemArray);
                    }
                }

                AccountTable.Dispose();

                ConnectionClose(connect);
            }
        }
        private string DatabaseAccountGenerate()
        {
            string query = "";

            #region Account Details
            if (textBoxAccountAccountID.Text != "")
            {
                var controls = new List<Tuple<string, string>>
                {
                    Tuple.Create(textBoxAccountAccountID.Text, "id"),
                    Tuple.Create(textBoxAccountAccountUsername.Text, "username"),
                    Tuple.Create(textBoxAccountAccountEmail.Text, "email"),
                    Tuple.Create(textBoxAccountAccountRegmail.Text, "reg_mail"),
                    Tuple.Create(textBoxAccountAccountLastIP.Text, "last_ip"),
                    Tuple.Create(checkBoxAccountAccountLocked.Checked ? "1" : "0", "locked"),
                    Tuple.Create(checkBoxAccountAccountOnline.Checked ? "1" : "0", "online"),
                    Tuple.Create(textBoxAccountAccountExpansion.Text, "expansion")
                };

                query += "UPDATE `account` SET ";

                for (var i = 0; i < controls.Count; i++)
                {
                    query += (i != controls.Count - 1) ?
                        "`" + controls[i].Item2 + "` = '" + controls[i].Item1 + "', " :
                        "`" + controls[i].Item2 + "` = '" + controls[i].Item1 + "'";
                }

                query += " WHERE `id` = '" + textBoxAccountAccountID.Text + "';";
            }
            #endregion

            #region Ban & Mute
            if (textBoxAccountAccountBandate.Text.Trim() != "" && textBoxAccountAccountMutedate.Text.Trim() != "")
            {
                DateTimeToUnixStamp(Convert.ToDateTime(textBoxAccountAccountBandate.Text));
                var banControls = new List<Tuple<string, string>>
                {
                    Tuple.Create(DateTimeToUnixStamp(Convert.ToDateTime(textBoxAccountAccountBandate.Text)).ToString(), "bandate"),
                    Tuple.Create(DateTimeToUnixStamp(Convert.ToDateTime(textBoxAccountAccountUnbandate.Text)).ToString(), "unbandate"),
                    Tuple.Create(textBoxAccountAccountBannedby.Text, "bannedby"),
                    Tuple.Create(textBoxAccountAccountBanreason.Text, "banreason"),
                    Tuple.Create(checkBoxAccountAccountBanActive.Checked ? "1" : "0", "active")
                };

                var muteControls = new List<Tuple<string, string>>
                {
                    Tuple.Create(DateTimeToUnixStamp(Convert.ToDateTime(textBoxAccountAccountMutedate.Text)).ToString(), "mutedate"),
                    Tuple.Create(textBoxAccountAccountMutetime.Text, "mutetime"),
                    Tuple.Create(textBoxAccountAccountMutedby.Text, "mutedby"),
                    Tuple.Create(textBoxAccountAccountMutereason.Text, "mutereason")
                };

                query += Environment.NewLine + Environment.NewLine;
                query += "UPDATE `account_banned` SET ";

                for (var i = 0; i < banControls.Count; i++)
                {
                    query += (i != banControls.Count - 1) ?
                        "`" + banControls[i].Item2 + "` = '" + banControls[i].Item1 + "', " :
                        "`" + banControls[i].Item2 + "` = '" + banControls[i].Item1 + "'";
                }

                query += " WHERE `id` = '" + textBoxAccountAccountID.Text + "';";

                // MUTE
                query += Environment.NewLine;
                query += "UPDATE `account_muted` SET ";

                for (var i = 0; i < muteControls.Count; i++)
                {
                    query += (i != muteControls.Count - 1) ?
                        "`" + muteControls[i].Item2 + "` = '" + muteControls[i].Item1 + "', " :
                        "`" + muteControls[i].Item2 + "` = '" + muteControls[i].Item1 + "'";
                }

                query += " WHERE `guid` = '" + textBoxAccountAccountID.Text + "';";

            }
            #endregion

            #region Access
            if (dataGridViewAccountAccess.Rows.Count > 0)
            {
                query += Environment.NewLine + Environment.NewLine;
                query += "DELETE FROM `account_access` WHERE `id` = '" + textBoxAccountAccountID.Text + "';";

                foreach (DataGridViewRow row in dataGridViewAccountAccess.Rows)
                {
                    query += Environment.NewLine;
                    query += "INSERT INTO `account_access` VALUES ('" + row.Cells[0].Value.ToString() + "', '" + row.Cells[1].Value.ToString() + "', '" + row.Cells[2].Value.ToString() + "');";
                }
            }
            #endregion

            return query;
        }
        #endregion
        #region Events
        private void buttonAccountSearchSearch_Click(object sender, EventArgs e)
        {
            bool totalSearch = CheckEmptyControls(tabPageAccountSearch); DialogResult dr;

            string query = "SELECT id, username, email, expansion FROM account WHERE '1' = '1'";

            if (totalSearch)
            {
                dr = MessageBox.Show("You sure, you want to load them all?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
            else
            {
                query += DatabaseQueryFilter(textBoxAccountSearchID.Text, "id");
                query += DatabaseQueryFilter(textBoxAccountSearchUsername.Text, "username");

                dr = DialogResult.OK;
            }

            if (dr == DialogResult.Cancel)
            {
                return;
            }

            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseAuth));

            if (ConnectionOpen(connect))
            {
                query += " ORDER BY id;";
                // Combined DataSet with all the tables.
                DataSet combinedTable = DatabaseSearch(connect, query);

                dataGridViewAccountSearch.DataSource = combinedTable.Tables[0];
                toolStripStatusLabelAccountSearchRows.Text = "Account(s) found: " + combinedTable.Tables[0].Rows.Count.ToString();

                ConnectionClose(connect);
            }
        }
        private void dataGridViewAccountSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewAccountSearch.SelectedRows.Count > 0)
            {
                DatabaseAccountSearch(dataGridViewAccountSearch.SelectedCells[0].Value.ToString());

                tabControlCategoryAccount.SelectedTab = tabPageAccountAccount;
            }
        }

        private void monthCalendarAccountAccountBanDate_DateChanged(object sender, DateRangeEventArgs e)
        {
            textBoxAccountAccountBandate.Text = monthCalendarAccountAccountBanDate.SelectionStart.ToString();
        }
        private void monthCalendarAccountAccountUnbanDate_DateChanged(object sender, DateRangeEventArgs e)
        {
            textBoxAccountAccountUnbandate.Text = monthCalendarAccountAccountUnbanDate.SelectionStart.ToString();
        }
        private void monthCalendarAccountAccountMuteDate_DateChanged(object sender, DateRangeEventArgs e)
        {
            textBoxAccountAccountMutedate.Text = monthCalendarAccountAccountMuteDate.SelectionStart.ToString();

            if (textBoxAccountAccountMutedate.Text.Trim() != "")
            {
                DateTime unmuteDay = monthCalendarAccountAccountUnmuteDate.SelectionStart;
                DateTime muteDay = Convert.ToDateTime(textBoxAccountAccountMutedate.Text);

                double muteTime = (unmuteDay - muteDay).TotalMinutes >= 0 ? (unmuteDay - muteDay).TotalMinutes : 0;

                textBoxAccountAccountMutetime.Text = Convert.ToInt64(muteTime).ToString();
            }
        }
        private void monthCalendarAccountAccountUnmuteDate_DateChanged(object sender, DateRangeEventArgs e)
        {
            if (textBoxAccountAccountMutedate.Text.Trim() != "")
            {
                DateTime unmuteDay = monthCalendarAccountAccountUnmuteDate.SelectionStart;
                DateTime muteDay = Convert.ToDateTime(textBoxAccountAccountMutedate.Text);

                double muteTime = (unmuteDay - muteDay).TotalMinutes >= 0 ? (unmuteDay - muteDay).TotalMinutes : 0;

                textBoxAccountAccountMutetime.Text = Convert.ToInt64(muteTime).ToString();
            }
        }
        private void buttonAccountAccountGenerateScript_Click(object sender, EventArgs e)
        {
            if (textBoxAccountAccountID.Text != string.Empty)
            {
                textBoxAccountScriptOutput.AppendText(DatabaseAccountGenerate());

                tabControlCategoryAccount.SelectedTab = tabPageAccountScript;
            }
        }

        private void buttonAccountAccountAccessAdd_Click(object sender, EventArgs e)
        {
            string[] acces = {
                textBoxAccountAccountID.Text,
                textBoxAccountAccountAccessGM.Text,
                textBoxAccountAccountAccessRID.Text,
                };

            if (textBoxAccountAccountAccessGM.Text != string.Empty && textBoxAccountAccountAccessRID.Text != string.Empty)
            {
                dataGridViewAccountAccess.Rows.Add(acces);
            }
        }
        private void buttonAccountAccountAccessDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewAccountAccess.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewAccountAccess.SelectedRows)
                {
                    dataGridViewAccountAccess.Rows.RemoveAt(row.Index);
                }
            }
        }

        private void toolStripSplitButtonAccountScriptUpdate_ButtonClick(object sender, EventArgs e)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseAuth));

            if (ConnectionOpen(connect))
            {
                int rows = DatabaseUpdate(connect, textBoxAccountScriptOutput.Text);
                toolStripStatusLabelAccountScriptRows.Text = "Row(s) affected: " + rows.ToString();
                ConnectionClose(connect);
            }
        }
        #endregion

        #endregion

        #region Character

        #region Functions
        // Searches for data for a specific character.
        private void DatabaseCharacterSearch(string characterGUID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseCharacters));

            if (ConnectionOpen(connect))
            {
                // Line 1 -> General Information : Line 2 -> Location : Line 3 ->  : Line 4 -> Stats : Line 5 -> Unknown.
                string characterQuery = "SELECT * FROM characters WHERE guid = '" + characterGUID + "';";

                DataSet CharacterTable = DatabaseSearch(connect, characterQuery);

                if (CharacterTable.Tables[0].Rows.Count != 0)
                {
                    // General Information
                    textBoxCharacterCharacterGUID.Text = CharacterTable.Tables[0].Rows[0]["guid"].ToString();
                    textBoxCharacterCharacterAccount.Text = CharacterTable.Tables[0].Rows[0]["account"].ToString();
                    textBoxCharacterCharacterName.Text = CharacterTable.Tables[0].Rows[0]["NAME"].ToString();
                    textBoxCharacterCharacterRace.Text = CharacterTable.Tables[0].Rows[0]["race"].ToString();
                    textBoxCharacterCharacterClass.Text = CharacterTable.Tables[0].Rows[0]["class"].ToString();
                    textBoxCharacterCharacterGender.Text = CharacterTable.Tables[0].Rows[0]["gender"].ToString();
                    textBoxCharacterCharacterLevel.Text = CharacterTable.Tables[0].Rows[0]["level"].ToString();
                    textBoxCharacterCharacterMoney.Text = CharacterTable.Tables[0].Rows[0]["money"].ToString();
                    textBoxCharacterCharacterXP.Text = CharacterTable.Tables[0].Rows[0]["xp"].ToString();
                    textBoxCharacterCharacterTitle.Text = CharacterTable.Tables[0].Rows[0]["chosentitle"].ToString();
                    checkBoxCharacterCharacterOnline.Checked = Convert.ToBoolean(CharacterTable.Tables[0].Rows[0]["online"]);
                    checkBoxCharacterCharacterCinematic.Checked = Convert.ToBoolean(CharacterTable.Tables[0].Rows[0]["cinematic"]);
                    checkBoxCharacterCharacterRest.Checked = Convert.ToBoolean(CharacterTable.Tables[0].Rows[0]["is_logout_resting"]);
                    // Location
                    textBoxCharacterCharacterMapID.Text = CharacterTable.Tables[0].Rows[0]["map"].ToString();
                    textBoxCharacterCharacterInstanceID.Text = CharacterTable.Tables[0].Rows[0]["instance_id"].ToString();
                    textBoxCharacterCharacterZoneID.Text = CharacterTable.Tables[0].Rows[0]["zone"].ToString();
                    textBoxCharacterCharacterCoordO.Text = CharacterTable.Tables[0].Rows[0]["orientation"].ToString();
                    textBoxCharacterCharacterCoordX.Text = CharacterTable.Tables[0].Rows[0]["position_x"].ToString();
                    textBoxCharacterCharacterCoordY.Text = CharacterTable.Tables[0].Rows[0]["position_y"].ToString();
                    textBoxCharacterCharacterCoordZ.Text = CharacterTable.Tables[0].Rows[0]["position_z"].ToString();
                    // Player vs Player
                    textBoxCharacterCharacterHonorPoints.Text = CharacterTable.Tables[0].Rows[0]["totalHonorPoints"].ToString();
                    textBoxCharacterCharacterArenaPoints.Text = CharacterTable.Tables[0].Rows[0]["arenaPoints"].ToString();
                    textBoxCharacterCharacterTotalKills.Text = CharacterTable.Tables[0].Rows[0]["totalKills"].ToString();
                    // Stats
                    textBoxCharacterCharacterHealth.Text = CharacterTable.Tables[0].Rows[0]["health"].ToString();
                    textBoxCharacterCharacterPower1.Text = CharacterTable.Tables[0].Rows[0]["power1"].ToString();
                    textBoxCharacterCharacterPower2.Text = CharacterTable.Tables[0].Rows[0]["power2"].ToString();
                    textBoxCharacterCharacterPower3.Text = CharacterTable.Tables[0].Rows[0]["power3"].ToString();
                    textBoxCharacterCharacterPower4.Text = CharacterTable.Tables[0].Rows[0]["power4"].ToString();
                    textBoxCharacterCharacterPower5.Text = CharacterTable.Tables[0].Rows[0]["power5"].ToString();
                    textBoxCharacterCharacterPower6.Text = CharacterTable.Tables[0].Rows[0]["power6"].ToString();
                    textBoxCharacterCharacterPower7.Text = CharacterTable.Tables[0].Rows[0]["power7"].ToString();
                    // Unknown
                    textBoxCharacterCharacterEquipmentCache.Text = CharacterTable.Tables[0].Rows[0]["equipmentCache"].ToString();
                    textBoxCharacterCharacterKnownTitles.Text = CharacterTable.Tables[0].Rows[0]["knownTitles"].ToString();
                    textBoxCharacterCharacterExploredZones.Text = CharacterTable.Tables[0].Rows[0]["exploredZones"].ToString();
                    textBoxCharacterCharacterTaxiMask.Text = CharacterTable.Tables[0].Rows[0]["taximask"].ToString();
                }

                ConnectionClose(connect);
            }
        }
        // Outputs the inventory for a specific player.
        private void DatabaseCharacterInventory(string characterGUID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseCharacters));

            if (ConnectionOpen(connect))
            {
                dataGridViewCharacterInventory.Rows.Clear();

                string inventoryQuery = "SELECT * FROM character_inventory WHERE guid = '" + characterGUID + "';";

                var datatable = DatabaseSearch(connect, inventoryQuery);

                var newTable = ConvertColumnsToString(datatable.Tables[0]);

                if (newTable.Rows.Count != 0)
                {
                    // Adds a new column 'name'
                    newTable.Columns.Add("name", typeof(string));

                    // loops every inventory item for name
                    for (int i = 0; i < newTable.Rows.Count; i++)
                    {
                        // sets the column 'name' to the itemname
                        newTable.Rows[i]["name"] = DatabaseItemGetName(DatabaseItemGetEntry(Convert.ToUInt32(newTable.Rows[i][3])));
                    }
                }

                foreach (DataRow row in newTable.Rows)
                {
                    dataGridViewCharacterInventory.Rows.Add(row.ItemArray);
                }
            }

            ConnectionClose(connect);
        }
        private string DatabaseCharacterInventoryGenerate()
        {
            string query = "";

            if (dataGridViewCharacterInventory.Rows.Count > 0)
            {
                query = "DELETE FROM `character_inventory` WHERE guid = '" + dataGridViewCharacterInventory.Rows[0].Cells[0].Value.ToString() + "';";

                foreach (DataGridViewRow row in dataGridViewCharacterInventory.Rows)
                {
                    if (row.Cells[0].Value.ToString() != "")
                    {
                        query += Environment.NewLine;

                        query += "INSERT INTO `character_inventory` VALUES (" +
                            row.Cells[0].Value.ToString() + ", " + row.Cells[1].Value.ToString() + ", " +
                            row.Cells[2].Value.ToString() + ", " + row.Cells[3].Value.ToString() + ");";
                    }
                }

            }

            return query;
        }
        private string DatabaseCharacterCharacterGenerate()
        {
            string query = "";

            #region Controls
                // Textbox values & corresponding column name.
                var controls = new List<Tuple<string, string>>
                {
                    Tuple.Create(textBoxCharacterCharacterGUID.Text, "guid"),
                    Tuple.Create(textBoxCharacterCharacterAccount.Text, "account"),
                    Tuple.Create(textBoxCharacterCharacterName.Text, "name"),
                    Tuple.Create(textBoxCharacterCharacterRace.Text, "class"),
                    Tuple.Create(textBoxCharacterCharacterClass.Text, "class"),
                    Tuple.Create(textBoxCharacterCharacterGender.Text, "gender"),
                    Tuple.Create(textBoxCharacterCharacterLevel.Text, "level"),
                    Tuple.Create(textBoxCharacterCharacterMoney.Text, "money"),
                    Tuple.Create(textBoxCharacterCharacterXP.Text, "xp"),
                    Tuple.Create(textBoxCharacterCharacterTitle.Text, "chosentitle"),
                    Tuple.Create(checkBoxCharacterCharacterOnline.Checked ? "1" : "0", "online"),
                    Tuple.Create(checkBoxCharacterCharacterCinematic.Checked ? "1" : "0", "cinematic"),
                    Tuple.Create(checkBoxCharacterCharacterRest.Checked ? "1" : "0", "is_logout_resting"),

                    Tuple.Create(textBoxCharacterCharacterMapID.Text, "map"),
                    Tuple.Create(textBoxCharacterCharacterInstanceID.Text, "instance_id"),
                    Tuple.Create(textBoxCharacterCharacterZoneID.Text, "zone"),
                    Tuple.Create(textBoxCharacterCharacterCoordO.Text, "orientation"),
                    Tuple.Create(textBoxCharacterCharacterCoordX.Text, "position_x"),
                    Tuple.Create(textBoxCharacterCharacterCoordY.Text, "position_y"),
                    Tuple.Create(textBoxCharacterCharacterCoordZ.Text, "position_z"),

                    Tuple.Create(textBoxCharacterCharacterHonorPoints.Text, "totalHonorPoints"),
                    Tuple.Create(textBoxCharacterCharacterArenaPoints.Text, "arenaPoints"),
                    Tuple.Create(textBoxCharacterCharacterTotalKills.Text, "totalKills"),

                    Tuple.Create(textBoxCharacterCharacterHealth.Text, "health"),
                    Tuple.Create(textBoxCharacterCharacterPower1.Text, "power1"),
                    Tuple.Create(textBoxCharacterCharacterPower2.Text, "power2"),
                    Tuple.Create(textBoxCharacterCharacterPower3.Text, "power3"),
                    Tuple.Create(textBoxCharacterCharacterPower4.Text, "power4"),
                    Tuple.Create(textBoxCharacterCharacterPower5.Text, "power5"),
                    Tuple.Create(textBoxCharacterCharacterPower6.Text, "power6"),
                    Tuple.Create(textBoxCharacterCharacterPower7.Text, "power7"),

                    Tuple.Create(textBoxCharacterCharacterEquipmentCache.Text, "equipmentCache"),
                    Tuple.Create(textBoxCharacterCharacterKnownTitles.Text, "knownTitles"),
                    Tuple.Create(textBoxCharacterCharacterExploredZones.Text, "exploredZones"),
                    Tuple.Create(textBoxCharacterCharacterTaxiMask.Text, "taximask"),
                };
                #endregion

            query += "UPDATE `characters` SET ";

            // column names & column values;
            foreach (var tuble in controls)
            {
                // UPDATE characters SET column1=value1 ...'
                query += (tuble == controls.Last()) ? $"`{tuble.Item2}` = '{tuble.Item1}'" : $"`{tuble.Item2}` = '{tuble.Item1}', ";
            }

            query += " WHERE `guid` = " + textBoxCharacterCharacterGUID.Text + ";";

            return query;
        }
        #endregion
        #region Events
        private void buttonCharacterSearchSearch_Click(object sender, EventArgs e)
        {
            bool totalSearch = CheckEmptyControls(tabPageCharacterSearch); DialogResult dr;

            string query = "SELECT guid, account, name, race, class, level FROM characters WHERE '1' = '1'";

            if (totalSearch)
            {
                dr = MessageBox.Show("You sure, you want to load them all?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
            else
            {
                query += DatabaseQueryFilter(textBoxCharacterSearchID.Text, "guid");
                query += DatabaseQueryFilter(textBoxCharacterSearchAccount.Text, "account");
                query += DatabaseQueryFilter(textBoxCharacterSearchUsername.Text, "name");

                dr = DialogResult.OK;
            }

            if (dr == DialogResult.Cancel)
            {
                return;
            }

            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseCharacters));

            if (ConnectionOpen(connect))
            {
                query += " ORDER BY guid;";
                // Combined DataSet with all the tables.
                DataSet combinedTable = DatabaseSearch(connect, query);

                dataGridViewCharacterSearch.DataSource = combinedTable.Tables[0];
                toolStripStatusLabelCharacterSearchRows.Text = "Character(s) found: " + combinedTable.Tables[0].Rows.Count.ToString();

                ConnectionClose(connect);
            }
        }
        private void dataGridViewCharacterSearchSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewCharacterSearch.RowCount != 0)
            {
                DatabaseCharacterSearch(dataGridViewCharacterSearch.SelectedCells[0].Value.ToString());
                DatabaseCharacterInventory(dataGridViewCharacterSearch.SelectedCells[0].Value.ToString());

                tabControlCategoryCharacter.SelectedTab = tabPageCharacterCharacter;
            }
        }
        private void buttonCharacterCharacterGenerate_Click(object sender, EventArgs e)
        {
            if (textBoxCharacterCharacterGUID.Text != string.Empty)
            {
                textBoxCharacterScriptOutput.AppendText(DatabaseCharacterCharacterGenerate());

                tabControlCategoryCharacter.SelectedTab = tabPageCharacterScript;
            }

        }

        private void toolStripSplitButtonCharacterScriptUpdate_ButtonClick(object sender, EventArgs e)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseCharacters));

            if (ConnectionOpen(connect))
            {
                toolStripStatusLabelCharacterScriptRows.Text = "Row(s) Affected: " + DatabaseUpdate(connect, textBoxCharacterScriptOutput.Text).ToString();

                ConnectionClose(connect);
            }
        }

        private void buttonCharacterInventoryAdd_Click(object sender, EventArgs e)
        {
            var values = new string[] {
                textBoxCharacterInventoryGUID.Text,
                textBoxCharacterInventoryBag.Text,
                textBoxCharacterInventorySlot.Text,
                textBoxCharacterInventoryItemID.Text
            };

            if (textBoxCharacterInventoryGUID.Text.Trim() != "")
            {
                dataGridViewCharacterInventory.Rows.Add(values);
            }
        }
        private void buttonCharacterInventoryRefresh_Click(object sender, EventArgs e)
        {
            DatabaseCharacterInventory((textBoxCharacterInventoryGUID.Text.Trim() != "") ? textBoxCharacterInventoryGUID.Text : textBoxCharacterCharacterGUID.Text);
        }
        private void buttonCharacterInventoryDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCharacterInventory.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewCharacterInventory.SelectedRows)
                {
                    dataGridViewCharacterInventory.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonCharacterInventoryGenerate_Click(object sender, EventArgs e)
        {
            textBoxCharacterScriptOutput.Text = DatabaseCharacterInventoryGenerate();
        }
        #endregion
        #region POPUPS
        private void buttonCharacterCharacterRace_Click(object sender, EventArgs e)
        {
            textBoxCharacterCharacterRace.Text = CreatePopupSelection("Character Race", ReadExcelCSV("ChrRaces", 0, 14), textBoxCharacterCharacterRace.Text);
        }
        private void buttonCharacterCharacterClass_Click(object sender, EventArgs e)
        {
            textBoxCharacterCharacterClass.Text = CreatePopupSelection("Character Class", ReadExcelCSV("ChrClasses", 0, 4), textBoxCharacterCharacterClass.Text);
        }
        #endregion

        #endregion

        #region Creature

        #region Functions
        // Searches the database for the creature's information.
        private void DatabaseCreatureSearch(string creatureEntryID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {

                string query = "SELECT * FROM creature_template WHERE entry = '" + creatureEntryID + "'; ";

                var ctTable = DatabaseSearch(connect, query);

                textBoxCreatureTemplateEntry.Text = ctTable.Tables[0].Rows[0]["entry"].ToString();
                textBoxCreatureTemplateDifEntry1.Text = ctTable.Tables[0].Rows[0]["difficulty_entry_1"].ToString();
                textBoxCreatureTemplateDifEntry2.Text = ctTable.Tables[0].Rows[0]["difficulty_entry_2"].ToString();
                textBoxCreatureTemplateDifEntry3.Text = ctTable.Tables[0].Rows[0]["difficulty_entry_3"].ToString();
                textBoxCreatureTemplateName.Text = ctTable.Tables[0].Rows[0]["NAME"].ToString();
                textBoxCreatureTemplateSubname.Text = ctTable.Tables[0].Rows[0]["subname"].ToString();

                textBoxCreatureTemplateModelID1.Text = ctTable.Tables[0].Rows[0]["modelid1"].ToString();
                textBoxCreatureTemplateModelID2.Text = ctTable.Tables[0].Rows[0]["modelid2"].ToString();
                textBoxCreatureTemplateModelID3.Text = ctTable.Tables[0].Rows[0]["modelid3"].ToString();
                textBoxCreatureTemplateModelID4.Text = ctTable.Tables[0].Rows[0]["modelid4"].ToString();
                textBoxCreatureTemplateLevelMin.Text = ctTable.Tables[0].Rows[0]["minlevel"].ToString();
                textBoxCreatureTemplateLevelMax.Text = ctTable.Tables[0].Rows[0]["maxlevel"].ToString();
                textBoxCreatureTemplateGoldMin.Text = ctTable.Tables[0].Rows[0]["mingold"].ToString();
                textBoxCreatureTemplateGoldMax.Text = ctTable.Tables[0].Rows[0]["maxgold"].ToString();
                textBoxCreatureTemplateKillCredit1.Text = ctTable.Tables[0].Rows[0]["KillCredit1"].ToString();
                textBoxCreatureTemplateKillCredit2.Text = ctTable.Tables[0].Rows[0]["KillCredit2"].ToString();
                textBoxCreatureTemplateRank.Text = ctTable.Tables[0].Rows[0]["rank"].ToString();
                textBoxCreatureTemplateScale.Text = ctTable.Tables[0].Rows[0]["scale"].ToString();
                textBoxCreatureTemplateFaction.Text = ctTable.Tables[0].Rows[0]["faction"].ToString();
                textBoxCreatureTemplateNPCFlags.Text = ctTable.Tables[0].Rows[0]["npcflag"].ToString();

                textBoxCreatureTemplateModHealth.Text = ctTable.Tables[0].Rows[0]["HealthModifier"].ToString();
                textBoxCreatureTemplateModMana.Text = ctTable.Tables[0].Rows[0]["ManaModifier"].ToString();
                textBoxCreatureTemplateModArmor.Text = ctTable.Tables[0].Rows[0]["ArmorModifier"].ToString();
                textBoxCreatureTemplateModDamage.Text = ctTable.Tables[0].Rows[0]["DamageModifier"].ToString();
                textBoxCreatureTemplateModExperience.Text = ctTable.Tables[0].Rows[0]["ExperienceModifier"].ToString();

                textBoxCreatureTemplateBaseAttack.Text = ctTable.Tables[0].Rows[0]["BaseAttackTime"].ToString();
                textBoxCreatureTemplateRangedAttack.Text = ctTable.Tables[0].Rows[0]["RangeAttackTime"].ToString();
                textBoxCreatureTemplateBV.Text = ctTable.Tables[0].Rows[0]["BaseVariance"].ToString();
                textBoxCreatureTemplateRV.Text = ctTable.Tables[0].Rows[0]["RangeVariance"].ToString();
                textBoxCreatureTemplateDS.Text = ctTable.Tables[0].Rows[0]["dmgschool"].ToString();

                textBoxCreatureTemplateAIName.Text = ctTable.Tables[0].Rows[0]["AIName"].ToString();
                textBoxCreatureTemplateMType.Text = ctTable.Tables[0].Rows[0]["MovementType"].ToString();
                textBoxCreatureTemplateInhabitType.Text = ctTable.Tables[0].Rows[0]["InhabitType"].ToString();
                textBoxCreatureTemplateHH.Text = ctTable.Tables[0].Rows[0]["HoverHeight"].ToString();
                textBoxCreatureTemplateGMID.Text = ctTable.Tables[0].Rows[0]["gossip_menu_id"].ToString();
                textBoxCreatureTemplateMID.Text = ctTable.Tables[0].Rows[0]["movementId"].ToString();
                textBoxCreatureTemplateScriptName.Text = ctTable.Tables[0].Rows[0]["ScriptName"].ToString();
                textBoxCreatureTemplateVID.Text = ctTable.Tables[0].Rows[0]["VehicleId"].ToString();

                textBoxCreatureTemplateTType.Text = ctTable.Tables[0].Rows[0]["trainer_type"].ToString();
                textBoxCreatureTemplateTSpell.Text = ctTable.Tables[0].Rows[0]["trainer_spell"].ToString();
                textBoxCreatureTemplateTRace.Text = ctTable.Tables[0].Rows[0]["trainer_class"].ToString();
                textBoxCreatureTemplateTClass.Text = ctTable.Tables[0].Rows[0]["trainer_race"].ToString();

                textBoxCreatureTemplateLootID.Text = ctTable.Tables[0].Rows[0]["lootid"].ToString();
                textBoxCreatureTemplatePickID.Text = ctTable.Tables[0].Rows[0]["pickpocketloot"].ToString();
                textBoxCreatureTemplateSkinID.Text = ctTable.Tables[0].Rows[0]["skinloot"].ToString();

                textBoxCreatureTemplateResis1.Text = ctTable.Tables[0].Rows[0]["resistance1"].ToString();
                textBoxCreatureTemplateResis2.Text = ctTable.Tables[0].Rows[0]["resistance2"].ToString();
                textBoxCreatureTemplateResis3.Text = ctTable.Tables[0].Rows[0]["resistance3"].ToString();
                textBoxCreatureTemplateResis4.Text = ctTable.Tables[0].Rows[0]["resistance4"].ToString();
                textBoxCreatureTemplateResis5.Text = ctTable.Tables[0].Rows[0]["resistance5"].ToString();
                textBoxCreatureTemplateResis6.Text = ctTable.Tables[0].Rows[0]["resistance6"].ToString();

                checkBoxCreatureTemplateHR.Checked = Convert.ToBoolean(ctTable.Tables[0].Rows[0]["RegenHealth"]);
                textBoxCreatureTemplateMechanic.Text = ctTable.Tables[0].Rows[0]["mechanic_immune_mask"].ToString();
                textBoxCreatureTemplateFamily.Text = ctTable.Tables[0].Rows[0]["family"].ToString();
                textBoxCreatureTemplateType.Text = ctTable.Tables[0].Rows[0]["TYPE"].ToString();
                textBoxCreatureTemplateTypeFlags.Text = ctTable.Tables[0].Rows[0]["type_flags"].ToString();
                textBoxCreatureTemplateFlagsExtra.Text = ctTable.Tables[0].Rows[0]["flags_extra"].ToString();
                textBoxCreatureTemplateUnitClass.Text = ctTable.Tables[0].Rows[0]["unit_class"].ToString();
                textBoxCreatureTemplateUnitflags.Text = ctTable.Tables[0].Rows[0]["unit_flags"].ToString();
                textBoxCreatureTemplateUnitflags2.Text = ctTable.Tables[0].Rows[0]["unit_flags2"].ToString();
                textBoxCreatureTemplateDynamic.Text = ctTable.Tables[0].Rows[0]["dynamicflags"].ToString();

                textBoxCreatureTemplateSpeedWalk.Text = ctTable.Tables[0].Rows[0]["speed_walk"].ToString();
                textBoxCreatureTemplateSpeedRun.Text = ctTable.Tables[0].Rows[0]["speed_run"].ToString();

                textBoxCreatureTemplateSpell1.Text = ctTable.Tables[0].Rows[0]["spell1"].ToString();
                textBoxCreatureTemplateSpell2.Text = ctTable.Tables[0].Rows[0]["spell2"].ToString();
                textBoxCreatureTemplateSpell3.Text = ctTable.Tables[0].Rows[0]["spell3"].ToString();
                textBoxCreatureTemplateSpell4.Text = ctTable.Tables[0].Rows[0]["spell4"].ToString();
                textBoxCreatureTemplateSpell5.Text = ctTable.Tables[0].Rows[0]["spell5"].ToString();
                textBoxCreatureTemplateSpell6.Text = ctTable.Tables[0].Rows[0]["spell6"].ToString();
                textBoxCreatureTemplateSpell7.Text = ctTable.Tables[0].Rows[0]["spell7"].ToString();
                textBoxCreatureTemplateSpell8.Text = ctTable.Tables[0].Rows[0]["spell8"].ToString();

                ConnectionClose(connect);
            }


        }
        // Template Generation
        private string DatabaseCreatureTempGenerate()
        {
            // Create three strings: finalQuery, query & values.
            string finalQuery = "", query = "REPLACE INTO `creature_template` (", values = "";

            #region CreatureTemplate
            var creatureTemplate = new List<Tuple<TextBox, string>>
            {
                Tuple.Create(textBoxCreatureTemplateEntry, "entry"),
                Tuple.Create(textBoxCreatureTemplateDifEntry1, "difficulty_entry_1"),
                Tuple.Create(textBoxCreatureTemplateDifEntry2, "difficulty_entry_2"),
                Tuple.Create(textBoxCreatureTemplateDifEntry3, "difficulty_entry_3"),
                Tuple.Create(textBoxCreatureTemplateName, "name"),
                Tuple.Create(textBoxCreatureTemplateSubname, "subname"),
                Tuple.Create(textBoxCreatureTemplateModelID1, "modelid1"),
                Tuple.Create(textBoxCreatureTemplateModelID2, "modelid2"),
                Tuple.Create(textBoxCreatureTemplateModelID3, "modelid3"),
                Tuple.Create(textBoxCreatureTemplateModelID4, "modelid4"),
                Tuple.Create(textBoxCreatureTemplateLevelMin, "minlevel"),
                Tuple.Create(textBoxCreatureTemplateLevelMax, "maxlevel"),
                Tuple.Create(textBoxCreatureTemplateGoldMin, "mingold"),
                Tuple.Create(textBoxCreatureTemplateGoldMax, "maxgold"),
                Tuple.Create(textBoxCreatureTemplateKillCredit1, "KillCredit1"),
                Tuple.Create(textBoxCreatureTemplateKillCredit2, "KillCredit2"),
                Tuple.Create(textBoxCreatureTemplateRank, "rank"),
                Tuple.Create(textBoxCreatureTemplateScale, "scale"),
                Tuple.Create(textBoxCreatureTemplateFaction, "faction"),
                Tuple.Create(textBoxCreatureTemplateNPCFlags, "npcflag"),
                Tuple.Create(textBoxCreatureTemplateModHealth, "HealthModifier"),
                Tuple.Create(textBoxCreatureTemplateModMana, "ManaModifier"),
                Tuple.Create(textBoxCreatureTemplateModArmor, "ArmorModifier"),
                Tuple.Create(textBoxCreatureTemplateModDamage, "DamageModifier"),
                Tuple.Create(textBoxCreatureTemplateModExperience, "ExperienceModifier"),
                Tuple.Create(textBoxCreatureTemplateBaseAttack, "BaseAttackTime"),
                Tuple.Create(textBoxCreatureTemplateRangedAttack, "RangeAttackTime"),
                Tuple.Create(textBoxCreatureTemplateBV, "BaseVariance"),
                Tuple.Create(textBoxCreatureTemplateRV, "RangeVariance"),
                Tuple.Create(textBoxCreatureTemplateDS, "dmgschool"),
                Tuple.Create(textBoxCreatureTemplateAIName, "AIName"),
                Tuple.Create(textBoxCreatureTemplateMType, "MovementType"),
                Tuple.Create(textBoxCreatureTemplateInhabitType, "InhabitType"),
                Tuple.Create(textBoxCreatureTemplateHH, "HoverHeight"),
                Tuple.Create(textBoxCreatureTemplateGMID, "gossip_menu_id"),
                Tuple.Create(textBoxCreatureTemplateMID, "movementId"),
                Tuple.Create(textBoxCreatureTemplateScriptName, "ScriptName"),
                Tuple.Create(textBoxCreatureTemplateVID, "VehicleId"),
                Tuple.Create(textBoxCreatureTemplateTType, "trainer_type"),
                Tuple.Create(textBoxCreatureTemplateTSpell, "trainer_spell"),
                Tuple.Create(textBoxCreatureTemplateTRace, "trainer_class"),
                Tuple.Create(textBoxCreatureTemplateTClass, "trainer_race"),
                Tuple.Create(textBoxCreatureTemplateLootID, "lootid"),
                Tuple.Create(textBoxCreatureTemplatePickID, "pickpocketloot"),
                Tuple.Create(textBoxCreatureTemplateSkinID, "skinloot"),
                Tuple.Create(textBoxCreatureTemplateResis1, "resistance1"),
                Tuple.Create(textBoxCreatureTemplateResis2, "resistance2"),
                Tuple.Create(textBoxCreatureTemplateResis3, "resistance3"),
                Tuple.Create(textBoxCreatureTemplateResis4, "resistance4"),
                Tuple.Create(textBoxCreatureTemplateResis5, "resistance5"),
                Tuple.Create(textBoxCreatureTemplateResis6, "resistance6"),
                Tuple.Create(textBoxCreatureTemplateMechanic, "mechanic_immune_mask"),
                Tuple.Create(textBoxCreatureTemplateFamily, "family"),
                Tuple.Create(textBoxCreatureTemplateType, "type"),
                Tuple.Create(textBoxCreatureTemplateTypeFlags, "type_flags"),
                Tuple.Create(textBoxCreatureTemplateFlagsExtra, "flags_extra"),
                Tuple.Create(textBoxCreatureTemplateUnitClass, "unit_class"),
                Tuple.Create(textBoxCreatureTemplateUnitflags, "unit_flags"),
                Tuple.Create(textBoxCreatureTemplateUnitflags2, "unit_flags2"),
                Tuple.Create(textBoxCreatureTemplateDynamic, "dynamicflags"),
                Tuple.Create(textBoxCreatureTemplateSpeedWalk, "speed_walk"),
                Tuple.Create(textBoxCreatureTemplateSpeedRun, "speed_run"),
                Tuple.Create(textBoxCreatureTemplateSpell1, "spell1"),
                Tuple.Create(textBoxCreatureTemplateSpell2, "spell2"),
                Tuple.Create(textBoxCreatureTemplateSpell3, "spell3"),
                Tuple.Create(textBoxCreatureTemplateSpell4, "spell4"),
                Tuple.Create(textBoxCreatureTemplateSpell5, "spell5"),
                Tuple.Create(textBoxCreatureTemplateSpell6, "spell6"),
                Tuple.Create(textBoxCreatureTemplateSpell7, "spell7"),
                Tuple.Create(textBoxCreatureTemplateSpell8, "spell8")
            };
            #endregion

            // Variables used in foreach loop.
            string quote; double deci; long integer;
            var lastTuple = creatureTemplate.Last();

            // Checks if value is a string/sentence or if it's a number (integer or decimal)
            // Stores the information in query (column names) & 'values' from textboxes.
            foreach (var temp in creatureTemplate)
            {
                quote = (double.TryParse(temp.Item1.Text, out deci) || long.TryParse(temp.Item1.Text, out integer)) ? "'" : "\"";

                if (temp.Equals(lastTuple))
                {
                    values += checkBoxCreatureTemplateHR.Checked.ToString() + ", ";
                    query += "`RegenHealth`, ";

                    values += $"{quote}{temp.Item1.Text.Trim()}{quote}";
                    query += "`" + temp.Item2.ToString() + "`";
                }
                else
                {
                    values += $"{quote}{temp.Item1.Text.Trim()}{quote}, ";
                    query += "`" + temp.Item2.ToString() + "`, ";
                }
            }

            finalQuery += $"{query}) VALUES ({values});";
            creatureTemplate = null; query = null; values = null;

            return finalQuery;
        }
        // Searches the database for the creature's spawnlocations
        private void DatabaseCreatureLocation(string creatureEntryID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                string query = "SELECT id, guid, map, zoneId, areaId, position_x, position_y, position_z, orientation, spawntimesecs, spawndist " +
                    "FROM creature WHERE id = '" + creatureEntryID + "';";

                // CreatureTable
                DataSet ctTable = DatabaseSearch(connect, query);

                dataGridViewCreatureLocation.DataSource = ctTable.Tables[0];

                ConnectionClose(connect);
            }
        }

        private void DatabaseCreatureVendor()
        {

        }
        #endregion
        #region Events
        private void buttonCreatureSearchSearch_Click(object sender, EventArgs e)
        {
            bool totalSearch = CheckEmptyControls(tabPageCreatureSearch); DialogResult dr;

            string query = "SELECT entry, NAME, subname, minlevel, maxlevel, rank, lootid FROM creature_template WHERE '1' = '1'";

            if (totalSearch)
            {
                dr = MessageBox.Show("You sure, you want to load them all?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
            else
            {
                query += DatabaseQueryFilter(textBoxCreatureSearchEntry.Text, "entry");
                query += DatabaseQueryFilter(textBoxCreatureSearchName.Text, "name");
                query += DatabaseQueryFilter(textBoxCreatureSearchSubname.Text, "subname");
                query += (textBoxCreatureSearchLevelMin.Text != string.Empty) ? " AND minlevel >= '" + textBoxCreatureSearchLevelMin.Text + "'" : "";
                query += (textBoxCreatureSearchLevelMax.Text != string.Empty) ? " AND maxlevel <= '" + textBoxCreatureSearchLevelMax.Text + "'" : "";
                query += DatabaseQueryFilter(textBoxCreatureSearchRank.Text, "rank");

                dr = DialogResult.OK;
            }

            if (dr == DialogResult.Cancel)
            {
                return;
            }

            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                query += " ORDER BY entry;";
                // Creature Template
                DataSet ctTable = DatabaseSearch(connect, query);

                dataGridViewCreatureSearch.DataSource = ctTable.Tables[0];
                toolStripStatusLabelCreatureSearchRows.Text = "Creature(s) found: " + ctTable.Tables[0].Rows.Count.ToString();

                ConnectionClose(connect);
            }
        }
        private void dataGridViewCreatureSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewCreatureSearch.Rows.Count != 0)
            {
                DatabaseCreatureSearch(dataGridViewCreatureSearch.SelectedCells[0].Value.ToString());
                DatabaseCreatureLocation(dataGridViewCreatureSearch.SelectedCells[0].Value.ToString());

                dataGridViewCreatureVendor.DataSource = DatabaseItemNameColumn("npc_vendor", "entry", textBoxCreatureTemplateEntry.Text.Trim(), 2, true);
                dataGridViewCreatureLoot.DataSource = DatabaseItemNameColumn("creature_loot_template", "Entry", textBoxCreatureTemplateLootID.Text.Trim(), 1, true);
                dataGridViewCreaturePickpocketLoot.DataSource = DatabaseItemNameColumn("pickpocketing_loot_template", "Entry", textBoxCreatureTemplatePickID.Text.Trim(), 1, true);
                dataGridViewCreatureSkinLoot.DataSource = DatabaseItemNameColumn("skinning_loot_template", "Entry", textBoxCreatureTemplateSkinID.Text.Trim(), 1, true);
            }

            tabControlCategoryCreature.SelectedTab = tabPageCreatureTemplate;
        }
        private void buttonCreatureTempGenerate_Click(object sender, EventArgs e)
        {
            if (textBoxCreatureTemplateEntry.Text != string.Empty)
            {
                textBoxCreatureScriptOutput.AppendText(DatabaseCreatureTempGenerate());

                tabControlCategoryCreature.SelectedTab = tabPageCreatureScript;
            }
        }

        private void toolStripSplitButtonCreatureNew_ButtonClick(object sender, EventArgs e)
        {
            var list = new List<Tuple<TextBox, string>>
            {
                Tuple.Create(textBoxCreatureTemplateName, ""),

                Tuple.Create(textBoxCreatureTemplateName, ""),
                Tuple.Create(textBoxCreatureTemplateSubname, ""),
                Tuple.Create(textBoxCreatureTemplateBaseAttack, "2000"),
                Tuple.Create(textBoxCreatureTemplateRangedAttack, "2000"),
                Tuple.Create(textBoxCreatureTemplateBV, "1"),
                Tuple.Create(textBoxCreatureTemplateRV, "1"),
                Tuple.Create(textBoxCreatureTemplateSpeedWalk, "1"),
                Tuple.Create(textBoxCreatureTemplateSpeedRun, "1.4286"),
                Tuple.Create(textBoxCreatureTemplateAIName, ""),
                Tuple.Create(textBoxCreatureTemplateScriptName, "")
            };

            DefaultValuesGenerate(tabPageCreatureTemplate);
            DefaultValuesOverride(list);

            checkBoxCreatureTemplateHR.Checked = true;

            tabControlCategoryCreature.SelectedTab = tabPageCreatureTemplate;
        }
        private void toolStripSplitButtonCreatureDelete_ButtonClick(object sender, EventArgs e)
        {
            GenerateDeleteSelectedRow(dataGridViewCreatureSearch, "creature_template", "entry", textBoxCreatureScriptOutput);
        }

        private void toolStripSplitButtonCreatureScriptSQLGenerate_ButtonClick(object sender, EventArgs e)
        {
            GenerateSQLFile("Creature_", textBoxCreatureTemplateEntry.Text + "-" + textBoxCreatureTemplateName.Text, textBoxCreatureScriptOutput);
        }
        private void toolStripSplitButtonCreatureScriptUpdate_ButtonClick(object sender, EventArgs e)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                toolStripStatusLabelCreatureScriptRows.Text = "Row(s) Affected: " + DatabaseUpdate(connect, textBoxCreatureScriptOutput.Text).ToString();

                ConnectionClose(connect);
            }
        }

        private void buttonCreatureVendorEC_Click(object sender, EventArgs e)
        {
            textBoxCreatureVendorEC.Text = CreatePopupSelection("Extended Cost Selection", ReadExcelCSV("ItemExtendedCost", 0, 1), textBoxCreatureVendorEC.Text);
        }

        #region Loot
        private void buttonCreatureLootAdd_Click(object sender, EventArgs e)
        {
            var values = new object[] {
                    textBoxCreatureLootEntry.Text,
                    textBoxCreatureLootItemID.Text,
                    textBoxCreatureLootReference.Text,
                    textBoxCreatureLootChance.Text,
                    textBoxCreatureLootQR.Text,
                    textBoxCreatureLootLM.Text,
                    textBoxCreatureLootGID.Text,
                    textBoxCreatureLootMIC.Text,
                    textBoxCreatureLootMAC.Text
                };

            if (textBoxCreatureLootEntry.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewCreatureLoot.DataSource;
                existingData.Rows.Add(values);
                dataGridViewCreatureLoot.DataSource = existingData;
                dataGridViewCreatureLoot.FirstDisplayedScrollingRowIndex = dataGridViewCreatureLoot.Rows.Count - 1;
            }
        }
        private void buttonCreatureLootRefresh_Click(object sender, EventArgs e)
        {
            dataGridViewItemLoot.DataSource = DatabaseItemNameColumn("item_loot_template", "entry", (textBoxCreatureLootEntry.Text.Trim() != "") ? textBoxCreatureLootEntry.Text : textBoxCreatureTemplateEntry.Text, 1, true);
        }
        private void buttonCreatureLootDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCreatureLoot.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewCreatureLoot.SelectedRows)
                {
                    dataGridViewCreatureLoot.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonCreatureLootGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("creature_loot_template", dataGridViewCreatureLoot, textBoxCreatureScriptOutput);
        }
        #endregion
        #region Pickpocket
        private void buttonCreaturePickpocketAdd_Click(object sender, EventArgs e)
        {
            var values = new object[] {
                    textBoxCreaturePickpocketEntry.Text,
                    textBoxCreaturePickpocketItemID.Text,
                    textBoxCreaturePickpocketReference.Text,
                    textBoxCreaturePickpocketChance.Text,
                    textBoxCreaturePickpocketQR.Text,
                    textBoxCreaturePickpocketLM.Text,
                    textBoxCreaturePickpocketGID.Text,
                    textBoxCreaturePickpocketMIC.Text,
                    textBoxCreaturePickpocketMAC.Text
                };

            if (textBoxCreaturePickpocketEntry.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewCreaturePickpocketLoot.DataSource;
                existingData.Rows.Add(values);
                dataGridViewCreaturePickpocketLoot.DataSource = existingData;
                dataGridViewCreaturePickpocketLoot.FirstDisplayedScrollingRowIndex = dataGridViewCreaturePickpocketLoot.Rows.Count - 1;
            }
        }
        private void buttonCreaturePickpocketRefresh_Click(object sender, EventArgs e)
        {
            dataGridViewCreaturePickpocketLoot.DataSource = DatabaseItemNameColumn("pickpocketing_loot_template", "Entry", (textBoxCreaturePickpocketEntry.Text.Trim() != "") ? textBoxCreaturePickpocketEntry.Text : textBoxCreatureTemplateEntry.Text, 1, true);
        }
        private void buttonCreaturePickpocketDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCreaturePickpocketLoot.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewCreaturePickpocketLoot.SelectedRows)
                {
                    dataGridViewCreaturePickpocketLoot.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonCreaturePickpocketGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("pickpocketing_loot_template", dataGridViewCreaturePickpocketLoot, textBoxCreatureScriptOutput);
        }
        #endregion
        #region Skin
        private void buttonCreatureSkinAdd_Click(object sender, EventArgs e)
        {
            var values = new object[] {
                    textBoxCreatureSkinEntry.Text,
                    textBoxCreatureSkinItemID.Text,
                    textBoxCreatureSkinReference.Text,
                    textBoxCreatureSkinChance.Text,
                    textBoxCreatureSkinQR.Text,
                    textBoxCreatureSkinLM.Text,
                    textBoxCreatureSkinGID.Text,
                    textBoxCreatureSkinMIC.Text,
                    textBoxCreatureSkinMAC.Text
                };

            if (textBoxCreatureSkinEntry.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewCreatureSkinLoot.DataSource;
                existingData.Rows.Add(values);
                dataGridViewCreatureSkinLoot.DataSource = existingData;
                dataGridViewCreatureSkinLoot.FirstDisplayedScrollingRowIndex = dataGridViewCreatureSkinLoot.Rows.Count - 1;
            }
        }
        private void buttonCreatureSkinRefresh_Click(object sender, EventArgs e)
        {
            dataGridViewCreatureSkinLoot.DataSource = DatabaseItemNameColumn("skinning_loot_template", "Entry", (textBoxCreatureSkinEntry.Text.Trim() != "") ? textBoxCreatureSkinEntry.Text : textBoxCreatureTemplateEntry.Text, 1, true);
        }
        private void buttonCreatureSkinDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCreatureSkinLoot.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewCreatureSkinLoot.SelectedRows)
                {
                    dataGridViewCreatureSkinLoot.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonCreatureSkinGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("skinning_loot_template", dataGridViewCreatureSkinLoot, textBoxCreatureScriptOutput);
        }
        #endregion
        #region Vendor
        private void buttonCreatureVendorAdd_Click(object sender, EventArgs e)
        {
            var values = new object[] {
                    textBoxCreatureVendorEntry.Text,
                    textBoxCreatureVendorSlot.Text,
                    textBoxCreatureVendorItemID.Text,
                    textBoxCreatureVendorMAC.Text,
                    textBoxCreatureVendorIncrtime.Text,
                    textBoxCreatureVendorEC.Text
                };

            if (textBoxCreatureVendorEntry.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewCreatureVendor.DataSource;
                existingData.Rows.Add(values);
                dataGridViewCreatureVendor.DataSource = existingData;
                dataGridViewCreatureVendor.FirstDisplayedScrollingRowIndex = dataGridViewCreatureVendor.Rows.Count - 1;
            }
        }
        private void buttonCreatureVendorRefresh_Click(object sender, EventArgs e)
        {

            dataGridViewCreatureVendor.DataSource = DatabaseItemNameColumn("npc_vendor", "entry", (textBoxCreatureVendorEntry.Text.Trim() != "") ? textBoxCreatureVendorEntry.Text.Trim() : textBoxCreatureTemplateEntry.Text.Trim(), 2, true);
        }
        private void buttonCreatureVendorDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCreatureVendor.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewCreatureVendor.SelectedRows)
                {
                    dataGridViewCreatureVendor.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonCreatureVendorGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("npc_vendor", dataGridViewCreatureVendor, textBoxCreatureScriptOutput);
        }
        #endregion
        #endregion
        #region POPUPS
        private void buttonCreatureTemplateModelID1_Click(object sender, EventArgs e)
        {
            bool[] rButtons = { false, true, false };

            textBoxCreatureTemplateModelID1.Text = CreatePopupEntity(textBoxCreatureTemplateModelID1.Text, rButtons, false);
        }
        private void buttonCreatureTemplateRank_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateRank.Text = CreatePopupSelection("Creature Rank", ReadExcelCSV("CreatureRanks", 0, 1), textBoxCreatureTemplateRank.Text);
        }
        private void buttonCreatureTemplateNPCFlags_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateNPCFlags.Text = CreatePopupChecklist("Creature NPC Flags", ReadExcelCSV("CreatureNPCFlags", 0, 1), textBoxCreatureTemplateNPCFlags.Text, true);
        }
        private void buttonCreatureTemplateSpell1_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateSpell1.Text = CreatePopupSelection("Spells I", ReadExcelCSV("Spells", 0, 1), textBoxCreatureTemplateSpell1.Text);
        }
        private void buttonCreatureTemplateDS_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateDS.Text = CreatePopupSelection("Damage School (Type)", ReadExcelCSV("CreatureDmgSchool", 0, 1), textBoxCreatureTemplateDS.Text);
        }
        private void buttonCreatureTemplateMType_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateMType.Text = CreatePopupSelection("Movement Type", ReadExcelCSV("CreatureMovementType", 0, 1), textBoxCreatureTemplateMType.Text);
        }
        private void buttonCreatureTemplateInhabitType_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateInhabitType.Text = CreatePopupChecklist("Inhabit Types", ReadExcelCSV("CreatureInhabitTypes", 0, 1), textBoxCreatureTemplateInhabitType.Text, true); ;
        }
        private void buttonCreatureTemplateMechanic_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateMechanic.Text = CreatePopupChecklist("Creature's Immunity", ReadExcelCSV("CreatureMechanic", 0, 1), textBoxCreatureTemplateMechanic.Text, true);
        }
        private void buttonCreatureTemplateFamily_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateFamily.Text = CreatePopupSelection("Creature's Family", ReadExcelCSV("CreatureFamily", 0, 1), textBoxCreatureTemplateFamily.Text);
        }
        private void buttonCreatureTemplateType_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateType.Text = CreatePopupSelection("Creature's Type", ReadExcelCSV("CreatureFamilyType", 0, 1), textBoxCreatureTemplateType.Text);
        }
        private void buttonCreatureTemplateTypeFlags_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateTypeFlags.Text = CreatePopupChecklist("Unit Flags I", ReadExcelCSV("CreatureTypeFlags", 0, 1), textBoxCreatureTemplateTypeFlags.Text, true);
        }
        private void buttonCreatureTemplateFlagsExtra_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateFlagsExtra.Text = CreatePopupChecklist("Extra Flags", ReadExcelCSV("CreatureFlagsExtra", 0, 1), textBoxCreatureTemplateFlagsExtra.Text, true);
        }
        private void buttonCreatureTemplateUnitClass_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateUnitClass.Text = CreatePopupSelection("Creature's Class", ReadExcelCSV("CreatureUnitClass", 0, 1), textBoxCreatureTemplateUnitClass.Text);
        }
        private void buttonCreatureTemplateUnitflags_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateUnitflags.Text = CreatePopupChecklist("Unit Flags I", ReadExcelCSV("CreatureUnitFlags", 0, 1), textBoxCreatureTemplateUnitflags.Text, true);
        }
        private void buttonCreatureTemplateUnitflags2_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateUnitflags2.Text = CreatePopupChecklist("Unit Flags II", ReadExcelCSV("CreatureUnitFlags2", 0, 1), textBoxCreatureTemplateUnitflags2.Text, true);
        }
        private void buttonCreatureTemplateDynamic_Click(object sender, EventArgs e)
        {
            textBoxCreatureTemplateDynamic.Text = CreatePopupChecklist("Dynamic Flags", ReadExcelCSV("CreatureDynamicFlags", 0, 1), textBoxCreatureTemplateDynamic.Text, true);
        }
        #endregion

        #endregion

        #region Quest

        #region Functions
        private void DatabaseQuestSearch(string questEntryID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                var query = $"SELECT * FROM quest_template WHERE ID = '{questEntryID}';" +
                            $"SELECT * FROM quest_template_addon WHERE ID = '{questEntryID}';" +
                            $"SELECT entry, name, subname FROM creature_template WHERE entry IN (SELECT id FROM creature_queststarter WHERE quest = '{questEntryID}');" +
                            $"SELECT entry, name, subname FROM creature_template WHERE entry IN (SELECT id FROM creature_questender WHERE quest = '{questEntryID}');" +
                            $"SELECT entry, name FROM gameobject_template WHERE entry IN (SELECT id FROM gameobject_queststarter WHERE quest = '{questEntryID}');" +
                            $"SELECT entry, name FROM gameobject_template WHERE entry IN (SELECT id FROM gameobject_questender WHERE quest = '{questEntryID}');";

                var qtTable = DatabaseSearch(connect, query);

                #region QuestTemplate
                var questTemplate = new List<Tuple<TextBox, string>>
                {
                    Tuple.Create(textBoxQuestSectionID, "ID"),
                    Tuple.Create(textBoxQuestSectionQuestType, "QuestType"),
                    Tuple.Create(textBoxQuestSectionQuestLevel, "QuestLevel"),
                    Tuple.Create(textBoxQuestSectionReqLevelMin, "MinLevel"),
                    Tuple.Create(textBoxQuestSectionReqQSort, "QuestSortID"),
                    Tuple.Create(textBoxQuestSectionQuestInfo, "QuestInfoID"),
                    Tuple.Create(textBoxQuestSectionOtherSP, "SuggestedGroupNum"),
                    Tuple.Create(textBoxQuestSectionReqFaction1, "RequiredFactionId1"),
                    Tuple.Create(textBoxQuestSectionReqFaction2, "RequiredFactionId2"),
                    Tuple.Create(textBoxQuestSectionReqValue1, "RequiredFactionValue1"),
                    Tuple.Create(textBoxQuestSectionReqValue2, "RequiredFactionValue2"),
                    Tuple.Create(textBoxQuestSectionRewOtherMoney, "RewardMoney"),
                    Tuple.Create(textBoxQuestSectionRewOtherMoneyML, "RewardBonusMoney"),
                    Tuple.Create(textBoxQuestSectionRewSpellDisplay, "RewardDisplaySpell"),
                    Tuple.Create(textBoxQuestSectionRewSpell, "RewardSpell"),
                    Tuple.Create(textBoxQuestSectionRewOtherHP, "RewardHonor"),
                    Tuple.Create(textBoxQuestSectionSourceItemID, "StartItem"),
                    Tuple.Create(textBoxQuestSectionQuestFlags, "Flags"),
                    Tuple.Create(textBoxQuestSectionReqPK, "RequiredPlayerKills"),
                    Tuple.Create(textBoxQuestSectionRewItemID1, "RewardItem1"),
                    Tuple.Create(textBoxQuestSectionRewItemC1, "RewardAmount1"),
                    Tuple.Create(textBoxQuestSectionRewItemID2, "RewardItem2"),
                    Tuple.Create(textBoxQuestSectionRewItemC2, "RewardAmount2"),
                    Tuple.Create(textBoxQuestSectionRewItemID3, "RewardItem3"),
                    Tuple.Create(textBoxQuestSectionRewItemC3, "RewardAmount3"),
                    Tuple.Create(textBoxQuestSectionRewItemID4, "RewardItem4"),
                    Tuple.Create(textBoxQuestSectionRewItemC4, "RewardAmount4"),
                    Tuple.Create(textBoxQuestSectionRewChoiceID1, "RewardChoiceItemID1"),
                    Tuple.Create(textBoxQuestSectionRewChoiceC1, "RewardChoiceItemQuantity1"),
                    Tuple.Create(textBoxQuestSectionRewChoiceID2, "RewardChoiceItemID2"),
                    Tuple.Create(textBoxQuestSectionRewChoiceC2, "RewardChoiceItemQuantity2"),
                    Tuple.Create(textBoxQuestSectionRewChoiceID3, "RewardChoiceItemID3"),
                    Tuple.Create(textBoxQuestSectionRewChoiceC3, "RewardChoiceItemQuantity3"),
                    Tuple.Create(textBoxQuestSectionRewChoiceID4, "RewardChoiceItemID4"),
                    Tuple.Create(textBoxQuestSectionRewChoiceC4, "RewardChoiceItemQuantity4"),
                    Tuple.Create(textBoxQuestSectionRewChoiceID5, "RewardChoiceItemID5"),
                    Tuple.Create(textBoxQuestSectionRewChoiceC5, "RewardChoiceItemQuantity5"),
                    Tuple.Create(textBoxQuestSectionRewChoiceID6, "RewardChoiceItemID6"),
                    Tuple.Create(textBoxQuestSectionRewChoiceC6, "RewardChoiceItemQuantity6"),
                    Tuple.Create(textBoxQuestSectionRewOtherTitleID, "RewardTitle"),
                    Tuple.Create(textBoxQuestSectionRewOtherTP, "RewardTalents"),
                    Tuple.Create(textBoxQuestSectionRewOtherAP, "RewardArenaPoints"),
                    Tuple.Create(textBoxQuestSectionRewFactionID1, "RewardFactionID1"),
                    Tuple.Create(textBoxQuestSectionRewFactionV1, "RewardFactionValue1"),
                    Tuple.Create(textBoxQuestSectionRewFactionOID1, "RewardFactionOverride1"),
                    Tuple.Create(textBoxQuestSectionRewFactionID2, "RewardFactionID2"),
                    Tuple.Create(textBoxQuestSectionRewFactionV2, "RewardFactionValue2"),
                    Tuple.Create(textBoxQuestSectionRewFactionOID2, "RewardFactionOverride2"),
                    Tuple.Create(textBoxQuestSectionRewFactionID3, "RewardFactionID3"),
                    Tuple.Create(textBoxQuestSectionRewFactionV3, "RewardFactionValue3"),
                    Tuple.Create(textBoxQuestSectionRewFactionOID3, "RewardFactionOverride3"),
                    Tuple.Create(textBoxQuestSectionRewFactionID4, "RewardFactionID4"),
                    Tuple.Create(textBoxQuestSectionRewFactionV4, "RewardFactionValue4"),
                    Tuple.Create(textBoxQuestSectionRewFactionOID4, "RewardFactionOverride4"),
                    Tuple.Create(textBoxQuestSectionRewFactionID5, "RewardFactionID5"),
                    Tuple.Create(textBoxQuestSectionRewFactionV5, "RewardFactionValue5"),
                    Tuple.Create(textBoxQuestSectionRewFactionOID5, "RewardFactionOverride5"),
                    Tuple.Create(textBoxQuestSectionTimeAllowed, "TimeAllowed"),
                    Tuple.Create(textBoxQuestSectionReqRace, "AllowableRaces"),
                    Tuple.Create(textBoxQuestSectionTitle, "LogTitle"),
                    Tuple.Create(textBoxQuestSectionLDescription, "LogDescription"),
                    Tuple.Create(textBoxQuestSectionQDescription, "QuestDescription"),
                    Tuple.Create(textBoxQuestSectionAreaDescription, "AreaDescription"),
                    Tuple.Create(textBoxQuestSectionCompleted, "QuestCompletionLog"),
                    Tuple.Create(textBoxQuestSectionReqNPCID1, "RequiredNpcOrGo1"),
                    Tuple.Create(textBoxQuestSectionReqNPCID2, "RequiredNpcOrGo2"),
                    Tuple.Create(textBoxQuestSectionReqNPCID3, "RequiredNpcOrGo3"),
                    Tuple.Create(textBoxQuestSectionReqNPCID4, "RequiredNpcOrGo4"),
                    Tuple.Create(textBoxQuestSectionReqNPCC1, "RequiredNpcOrGoCount1"),
                    Tuple.Create(textBoxQuestSectionReqNPCC2, "RequiredNpcOrGoCount2"),
                    Tuple.Create(textBoxQuestSectionReqNPCC3, "RequiredNpcOrGoCount3"),
                    Tuple.Create(textBoxQuestSectionReqNPCC4, "RequiredNpcOrGoCount4"),
                    Tuple.Create(textBoxQuestSectionReqItemID1, "RequiredItemId1"),
                    Tuple.Create(textBoxQuestSectionReqItemID2, "RequiredItemId2"),
                    Tuple.Create(textBoxQuestSectionReqItemID3, "RequiredItemId3"),
                    Tuple.Create(textBoxQuestSectionReqItemID4, "RequiredItemId4"),
                    Tuple.Create(textBoxQuestSectionReqItemID5, "RequiredItemId5"),
                    Tuple.Create(textBoxQuestSectionReqItemID6, "RequiredItemId6"),
                    Tuple.Create(textBoxQuestSectionReqItemC1, "RequiredItemCount1"),
                    Tuple.Create(textBoxQuestSectionReqItemC2, "RequiredItemCount2"),
                    Tuple.Create(textBoxQuestSectionReqItemC3, "RequiredItemCount3"),
                    Tuple.Create(textBoxQuestSectionReqItemC4, "RequiredItemCount4"),
                    Tuple.Create(textBoxQuestSectionReqItemC5, "RequiredItemCount5"),
                    Tuple.Create(textBoxQuestSectionReqItemC6, "RequiredItemCount6"),
                    Tuple.Create(textBoxQuestSectionObjectives1, "ObjectiveText1"),
                    Tuple.Create(textBoxQuestSectionObjectives2, "ObjectiveText2"),
                    Tuple.Create(textBoxQuestSectionObjectives3, "ObjectiveText3"),
                    Tuple.Create(textBoxQuestSectionObjectives4, "ObjectiveText4")
                };
                #endregion

                foreach (var tuple in questTemplate)
                {
                    tuple.Item1.Text = qtTable.Tables[0].Rows[0][tuple.Item2.ToString()].ToString();
                }

                #region QuestTemplateAddon
                questTemplate.Clear();
                questTemplate = new List<Tuple<TextBox, string>>
                {
                    Tuple.Create(textBoxQuestSectionID, "ID"),
                    Tuple.Create(textBoxQuestSectionReqLevelMax, "MaxLevel"),
                    Tuple.Create(textBoxQuestSectionReqClass, "AllowableClasses"),
                    Tuple.Create(textBoxQuestSectionSourceSpellID, "SourceSpellId"),
                    Tuple.Create(textBoxQuestSectionPrevQuest, "PrevQuestId"),
                    Tuple.Create(textBoxQuestSectionNextQuest, "NextQuestId"),
                    Tuple.Create(textBoxQuestSectionExclusive, "ExclusiveGroup"),
                    Tuple.Create(textBoxQuestSectionRewOtherMailID, "RewardMailTemplateId"),
                    Tuple.Create(textBoxQuestSectionRewOtherMailDelay, "RewardMailDelay"),
                    Tuple.Create(textBoxQuestSectionReqSkillID, "RequiredSkillID"),
                    Tuple.Create(textBoxQuestSectionReqSkillPoints, "RequiredSkillPoints"),
                    Tuple.Create(textBoxQuestSectionReqMinRepF, "RequiredMinRepFaction"),
                    Tuple.Create(textBoxQuestSectionReqMaxRepF, "RequiredMaxRepFaction"),
                    Tuple.Create(textBoxQuestSectionReqMinRepV, "RequiredMinRepValue"),
                    Tuple.Create(textBoxQuestSectionReqMaxRepV, "RequiredMaxRepValue"),
                    Tuple.Create(textBoxQuestSectionSourceItemCount, "ProvidedItemCount"),
                    Tuple.Create(textBoxQuestSectionOtherSF, "SpecialFlags")
                };
                #endregion

                foreach (var tuple in questTemplate)
                {
                    tuple.Item1.Text = qtTable.Tables[1].Rows[0][tuple.Item2.ToString()].ToString();
                }

                questTemplate = null;

                // Givers & Takers
                DataColumn cGiver = new DataColumn("entityType", typeof(string)); cGiver.DefaultValue = "creature";
                DataColumn cTaker = new DataColumn("entityType", typeof(string)); cTaker.DefaultValue = "creature";
                DataColumn gGiver = new DataColumn("entityType", typeof(string)); gGiver.DefaultValue = "game object";
                DataColumn gTaker = new DataColumn("entityType", typeof(string)); gTaker.DefaultValue = "game object";

                if (qtTable.Tables[2].Rows.Count > 0) { qtTable.Tables[2].Columns.Add(cGiver); } // Givers
                if (qtTable.Tables[3].Rows.Count > 0) { qtTable.Tables[3].Columns.Add(cTaker); } // Takers
                if (qtTable.Tables[4].Rows.Count > 0) { qtTable.Tables[4].Columns.Add(gGiver); } // Givers
                if (qtTable.Tables[5].Rows.Count > 0) { qtTable.Tables[5].Columns.Add(gTaker); } // Takers

                DataTable givers = new DataTable();
                givers = qtTable.Tables[2].Copy();
                givers.Merge(qtTable.Tables[4]);

                dataGridViewQuestGivers.DataSource = givers;

                ConnectionClose(connect);
            }
        }
        private string DatabaseQuestSectionGenerate()
        {
            // Create three strings: finalQuery, query & values.
            string finalQuery = "", query = "REPLACE INTO `quest_template` (", values = "";

            // Stores every column name and textbox for the quest_template table.
            #region QuestTemplate
            var questTemplate = new List<Tuple<TextBox, string>>
            {
                Tuple.Create(textBoxQuestSectionID, "ID"),
                Tuple.Create(textBoxQuestSectionQuestType, "QuestType"),
                Tuple.Create(textBoxQuestSectionQuestLevel, "QuestLevel"),
                Tuple.Create(textBoxQuestSectionReqLevelMin, "MinLevel"),
                Tuple.Create(textBoxQuestSectionReqQSort, "QuestSortID"),
                Tuple.Create(textBoxQuestSectionQuestInfo, "QuestInfoID"),
                Tuple.Create(textBoxQuestSectionOtherSP, "SuggestedGroupNum"),
                Tuple.Create(textBoxQuestSectionReqFaction1, "RequiredFactionId1"),
                Tuple.Create(textBoxQuestSectionReqFaction2, "RequiredFactionId2"),
                Tuple.Create(textBoxQuestSectionReqValue1, "RequiredFactionValue1"),
                Tuple.Create(textBoxQuestSectionReqValue2, "RequiredFactionValue2"),
                Tuple.Create(textBoxQuestSectionRewOtherMoney, "RewardMoney"),
                Tuple.Create(textBoxQuestSectionRewOtherMoneyML, "RewardBonusMoney"),
                Tuple.Create(textBoxQuestSectionRewSpellDisplay, "RewardDisplaySpell"),
                Tuple.Create(textBoxQuestSectionRewSpell, "RewardSpell"),
                Tuple.Create(textBoxQuestSectionRewOtherHP, "RewardHonor"),
                Tuple.Create(textBoxQuestSectionSourceItemID, "StartItem"),
                Tuple.Create(textBoxQuestSectionQuestFlags, "Flags"),
                Tuple.Create(textBoxQuestSectionReqPK, "RequiredPlayerKills"),
                Tuple.Create(textBoxQuestSectionRewItemID1, "RewardItem1"),
                Tuple.Create(textBoxQuestSectionRewItemC1, "RewardAmount1"),
                Tuple.Create(textBoxQuestSectionRewItemID2, "RewardItem2"),
                Tuple.Create(textBoxQuestSectionRewItemC2, "RewardAmount2"),
                Tuple.Create(textBoxQuestSectionRewItemID3, "RewardItem3"),
                Tuple.Create(textBoxQuestSectionRewItemC3, "RewardAmount3"),
                Tuple.Create(textBoxQuestSectionRewItemID4, "RewardItem4"),
                Tuple.Create(textBoxQuestSectionRewItemC4, "RewardAmount4"),
                Tuple.Create(textBoxQuestSectionRewChoiceID1, "RewardChoiceItemID1"),
                Tuple.Create(textBoxQuestSectionRewChoiceC1, "RewardChoiceItemQuantity1"),
                Tuple.Create(textBoxQuestSectionRewChoiceID2, "RewardChoiceItemID2"),
                Tuple.Create(textBoxQuestSectionRewChoiceC2, "RewardChoiceItemQuantity2"),
                Tuple.Create(textBoxQuestSectionRewChoiceID3, "RewardChoiceItemID3"),
                Tuple.Create(textBoxQuestSectionRewChoiceC3, "RewardChoiceItemQuantity3"),
                Tuple.Create(textBoxQuestSectionRewChoiceID4, "RewardChoiceItemID4"),
                Tuple.Create(textBoxQuestSectionRewChoiceC4, "RewardChoiceItemQuantity4"),
                Tuple.Create(textBoxQuestSectionRewChoiceID5, "RewardChoiceItemID5"),
                Tuple.Create(textBoxQuestSectionRewChoiceC5, "RewardChoiceItemQuantity5"),
                Tuple.Create(textBoxQuestSectionRewChoiceID6, "RewardChoiceItemID6"),
                Tuple.Create(textBoxQuestSectionRewChoiceC6, "RewardChoiceItemQuantity6"),
                Tuple.Create(textBoxQuestSectionRewOtherTitleID, "RewardTitle"),
                Tuple.Create(textBoxQuestSectionRewOtherTP, "RewardTalents"),
                Tuple.Create(textBoxQuestSectionRewOtherAP, "RewardArenaPoints"),
                Tuple.Create(textBoxQuestSectionRewFactionID1, "RewardFactionID1"),
                Tuple.Create(textBoxQuestSectionRewFactionV1, "RewardFactionValue1"),
                Tuple.Create(textBoxQuestSectionRewFactionOID1, "RewardFactionOverride1"),
                Tuple.Create(textBoxQuestSectionRewFactionID2, "RewardFactionID2"),
                Tuple.Create(textBoxQuestSectionRewFactionV2, "RewardFactionValue2"),
                Tuple.Create(textBoxQuestSectionRewFactionOID2, "RewardFactionOverride2"),
                Tuple.Create(textBoxQuestSectionRewFactionID3, "RewardFactionID3"),
                Tuple.Create(textBoxQuestSectionRewFactionV3, "RewardFactionValue3"),
                Tuple.Create(textBoxQuestSectionRewFactionOID3, "RewardFactionOverride3"),
                Tuple.Create(textBoxQuestSectionRewFactionID4, "RewardFactionID4"),
                Tuple.Create(textBoxQuestSectionRewFactionV4, "RewardFactionValue4"),
                Tuple.Create(textBoxQuestSectionRewFactionOID4, "RewardFactionOverride4"),
                Tuple.Create(textBoxQuestSectionRewFactionID5, "RewardFactionID5"),
                Tuple.Create(textBoxQuestSectionRewFactionV5, "RewardFactionValue5"),
                Tuple.Create(textBoxQuestSectionRewFactionOID5, "RewardFactionOverride5"),
                Tuple.Create(textBoxQuestSectionTimeAllowed, "TimeAllowed"),
                Tuple.Create(textBoxQuestSectionReqRace, "AllowableRaces"),
                Tuple.Create(textBoxQuestSectionTitle, "LogTitle"),
                Tuple.Create(textBoxQuestSectionLDescription, "LogDescription"),
                Tuple.Create(textBoxQuestSectionQDescription, "QuestDescription"),
                Tuple.Create(textBoxQuestSectionAreaDescription, "AreaDescription"),
                Tuple.Create(textBoxQuestSectionCompleted, "QuestCompletionLog"),
                Tuple.Create(textBoxQuestSectionReqNPCID1, "RequiredNpcOrGo1"),
                Tuple.Create(textBoxQuestSectionReqNPCID2, "RequiredNpcOrGo2"),
                Tuple.Create(textBoxQuestSectionReqNPCID3, "RequiredNpcOrGo3"),
                Tuple.Create(textBoxQuestSectionReqNPCID4, "RequiredNpcOrGo4"),
                Tuple.Create(textBoxQuestSectionReqNPCC1, "RequiredNpcOrGoCount1"),
                Tuple.Create(textBoxQuestSectionReqNPCC2, "RequiredNpcOrGoCount2"),
                Tuple.Create(textBoxQuestSectionReqNPCC3, "RequiredNpcOrGoCount3"),
                Tuple.Create(textBoxQuestSectionReqNPCC4, "RequiredNpcOrGoCount4"),
                Tuple.Create(textBoxQuestSectionReqItemID1, "RequiredItemId1"),
                Tuple.Create(textBoxQuestSectionReqItemID2, "RequiredItemId2"),
                Tuple.Create(textBoxQuestSectionReqItemID3, "RequiredItemId3"),
                Tuple.Create(textBoxQuestSectionReqItemID4, "RequiredItemId4"),
                Tuple.Create(textBoxQuestSectionReqItemID5, "RequiredItemId5"),
                Tuple.Create(textBoxQuestSectionReqItemID6, "RequiredItemId6"),
                Tuple.Create(textBoxQuestSectionReqItemC1, "RequiredItemCount1"),
                Tuple.Create(textBoxQuestSectionReqItemC2, "RequiredItemCount2"),
                Tuple.Create(textBoxQuestSectionReqItemC3, "RequiredItemCount3"),
                Tuple.Create(textBoxQuestSectionReqItemC4, "RequiredItemCount4"),
                Tuple.Create(textBoxQuestSectionReqItemC5, "RequiredItemCount5"),
                Tuple.Create(textBoxQuestSectionReqItemC6, "RequiredItemCount6"),
                Tuple.Create(textBoxQuestSectionObjectives1, "ObjectiveText1"),
                Tuple.Create(textBoxQuestSectionObjectives2, "ObjectiveText2"),
                Tuple.Create(textBoxQuestSectionObjectives3, "ObjectiveText3"),
                Tuple.Create(textBoxQuestSectionObjectives4, "ObjectiveText4")
            };
            #endregion

            // Variables used in foreach loop.
            string quote; double deci; long integer;
            var lastTuple = questTemplate.Last();

            // Checks if value is a string/sentence or if it's a number (integer or decimal)
            // Stores the information in query (column names) & 'values' from textboxes.
            foreach (var temp in questTemplate)
            {
                quote = (double.TryParse(temp.Item1.Text, out deci) || long.TryParse(temp.Item1.Text, out integer)) ? "'" : "\"";

                if (temp.Equals(lastTuple))
                {
                    values += $"{quote}{temp.Item1.Text.Trim()}{quote}";
                    query += "`" + temp.Item2.ToString() + "`";
                }
                else
                {
                    values += $"{quote}{temp.Item1.Text.Trim()}{quote}, ";
                    query += "`" + temp.Item2.ToString() + "`, ";
                }
            }

            finalQuery += $"{query}) VALUES ({values});";
            finalQuery += Environment.NewLine + Environment.NewLine;

            questTemplate = null; query = null; values = null; lastTuple = null;

            query = "REPLACE INTO `quest_template_addon` (";

            // Stores every column name and textbox for the quest_template_addon table.
            #region QuestTemplateAddon
            questTemplate = new List<Tuple<TextBox, string>>
            {
                Tuple.Create(textBoxQuestSectionID, "ID"),
                Tuple.Create(textBoxQuestSectionReqLevelMax, "MaxLevel"),
                Tuple.Create(textBoxQuestSectionReqClass, "AllowableClasses"),
                Tuple.Create(textBoxQuestSectionSourceSpellID, "SourceSpellId"),
                Tuple.Create(textBoxQuestSectionPrevQuest, "PrevQuestId"),
                Tuple.Create(textBoxQuestSectionNextQuest, "NextQuestId"),
                Tuple.Create(textBoxQuestSectionExclusive, "ExclusiveGroup"),
                Tuple.Create(textBoxQuestSectionRewOtherMailID, "RewardMailTemplateId"),
                Tuple.Create(textBoxQuestSectionRewOtherMailDelay, "RewardMailDelay"),
                Tuple.Create(textBoxQuestSectionReqSkillID, "RequiredSkillID"),
                Tuple.Create(textBoxQuestSectionReqSkillPoints, "RequiredSkillPoints"),
                Tuple.Create(textBoxQuestSectionReqMinRepF, "RequiredMinRepFaction"),
                Tuple.Create(textBoxQuestSectionReqMaxRepF, "RequiredMaxRepFaction"),
                Tuple.Create(textBoxQuestSectionReqMinRepV, "RequiredMinRepValue"),
                Tuple.Create(textBoxQuestSectionReqMaxRepV, "RequiredMaxRepValue"),
                Tuple.Create(textBoxQuestSectionSourceItemCount, "ProvidedItemCount"),
                Tuple.Create(textBoxQuestSectionOtherSF, "SpecialFlags")
            };
            #endregion

            lastTuple = questTemplate.Last();

            foreach (var temp in questTemplate)
            {
                quote = (double.TryParse(temp.Item1.Text, out deci) || long.TryParse(temp.Item1.Text, out integer)) ? "'" : "\"";

                if (temp.Equals(lastTuple))
                {
                    values += $"{quote}" + temp.Item1.Text.Trim() + $"{quote}";
                    query += "`" + temp.Item2.ToString() + "`";
                }
                else
                {
                    values += $"{quote}" + temp.Item1.Text.Trim() + $"{quote}, ";
                    query += "`" + temp.Item2.ToString() + "`, ";
                }
            }

            finalQuery += $"{query}) VALUES ({values});";

            questTemplate = null; query = null; values = null;

            return finalQuery;
        }
        #endregion
        #region Events
        private void buttonQuestSearchSearch_Click(object sender, EventArgs e)
        {
            bool totalSearch = CheckEmptyControls(tabPageQuestSearch); DialogResult dr;

            string query = "SELECT ID, LogTitle, LogDescription FROM quest_template WHERE '1' = '1'";
            string qsQuery = " AND ID IN (SELECT quest FROM creature_queststarter WHERE id = '" + textBoxQuestSearchGiver.Text + "')"; // queststart query
            string qeQuery = " AND ID IN (SELECT quest FROM creature_questender WHERE id = '" + textBoxQuestSearchTaker.Text + "')"; // questender query
            string prevQuery = " AND ID IN (SELECT ID FROM quest_template_addon WHERE PrevQuestID = '" + textBoxQuestSearchPQID.Text + "')"; // quest template addon -> prevquestid
            string nextQuery = " AND ID IN (SELECT ID FROM quest_template_addon WHERE NextQuestID = '" + textBoxQuestSearchNQID.Text + "')"; // quest template addon -> nextquestid

            if (totalSearch)
            {
                dr = MessageBox.Show("You sure, you want to load them all?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
            else
            {
                if (textBoxQuestSearchID.Text != "" || textBoxQuestSearchTitle.Text != "" || textBoxQuestSearchInfo.Text != "")
                {
                    query += DatabaseQueryFilter(textBoxQuestSearchID.Text, "ID");
                    query += DatabaseQueryFilter(textBoxQuestSearchTitle.Text, "logTitle");
                    query += DatabaseQueryFilter(textBoxQuestSearchInfo.Text, "QuestInfoID");
                }

                if (textBoxQuestSearchGiver.Text.Trim() != "")
                {
                    query += qsQuery;
                }

                if (textBoxQuestSearchTaker.Text.Trim() != "")
                {
                    query += qeQuery;
                }

                if (textBoxQuestSearchPQID.Text.Trim() != "")
                {
                    query += prevQuery;
                }

                if (textBoxQuestSearchNQID.Text.Trim() != "")
                {
                    query += nextQuery;
                }

                dr = DialogResult.OK;
            }

            if (dr == DialogResult.Cancel)
            {
                return;
            }

            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                query += " ORDER BY ID;";
                DataSet combinedTable = DatabaseSearch(connect, query);

                dataGridViewQuestSearch.DataSource = combinedTable.Tables[0];
                toolStripStatusLabelQuestSearchRows.Text = "Quest(s) found: " + combinedTable.Tables[0].Rows.Count.ToString();

                ConnectionClose(connect);
            }
        }
        private void dataGridViewQuestSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewQuestSearch.SelectedRows.Count > 0)
            {
                DatabaseQuestSearch(dataGridViewQuestSearch.SelectedCells[0].Value.ToString());

                tabControlCategoryQuest.SelectedTab = tabPageQuestSection1;
            }
        }
        private void buttonQuestSectionGenerate_Click(object sender, EventArgs e)
        {
            if (textBoxQuestSectionID.Text != string.Empty)
            {
                textBoxQuestScriptOutput.AppendText(DatabaseQuestSectionGenerate());

                tabControlCategoryQuest.SelectedTab = tabPageQuestScript;
            }
        }

        private void toolStripSplitButtonQuestNew_ButtonClick(object sender, EventArgs e)
        {
            var list = new List<Tuple<TextBox, string>>();

            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionTitle, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionLDescription, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionQDescription, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionAreaDescription, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionCompleted, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionObjectives1, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionObjectives2, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionObjectives3, ""));
            list.Add(new Tuple<TextBox, string>(textBoxQuestSectionObjectives4, ""));

            DefaultValuesGenerate(tabPageQuestSection1);
            DefaultValuesGenerate(tabPageQuestSection2);
            DefaultValuesOverride(list);

            tabControlCategoryQuest.SelectedTab = tabPageQuestSection1;
        }
        private void toolStripSplitButtonQuestDelete_ButtonClick(object sender, EventArgs e)
        {
            GenerateDeleteSelectedRow(dataGridViewQuestSearch, "quest_template", "ID", textBoxQuestScriptOutput);
        }

        private void toolStripSplitButtonQuestScriptSQLGenerate_ButtonClick(object sender, EventArgs e)
        {
            GenerateSQLFile("QUEST_", textBoxQuestSectionID.Text + "-" + textBoxQuestSearchTitle.Text, textBoxQuestScriptOutput);
        }
        private void toolStripSplitButtonQuestScriptUpdate_ButtonClick(object sender, EventArgs e)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                toolStripStatusLabelQuestScriptRows.Text = "Row(s) Affected: " + DatabaseUpdate(connect, textBoxQuestScriptOutput.Text).ToString();

                ConnectionClose(connect);
            }
        }
        #endregion
        #region POPUPS
        private void buttonQuestSearchInfo_Click(object sender, EventArgs e)
        {
            textBoxQuestSearchInfo.Text = CreatePopupSelection("Quest Info", ReadExcelCSV("QuestInfo", 0, 1), textBoxQuestSearchInfo.Text);
        }
        // Section 1
        private void buttonQuestSectionSourceItemID_Click(object sender, EventArgs e)
        {
            bool[] rButton = { true, false, false };

            textBoxQuestSectionSourceItemID.Text = CreatePopupEntity(textBoxQuestSectionSourceItemID.Text, rButton);
        }
        private void buttonQuestSectionReqRace_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionReqRace.Text = CreatePopupChecklist("Requirement: Races", ReadExcelCSV("ChrRaces", 0, 14), textBoxQuestSectionReqRace.Text, true);
        }
        private void buttonQuestSectionReqClass_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionReqClass.Text = CreatePopupChecklist("Requirement: Classes", ReadExcelCSV("ChrClasses", 0, 4), textBoxQuestSectionReqClass.Text, true);
        }
        private void buttonQuestSectionQSort_Click(object sender, EventArgs e)
        {
            if (radioButtonQuestSectionZID.Checked)
            {
                textBoxQuestSectionReqQSort.Text = CreatePopupSelection("Zone ID Selection", ReadExcelCSV("AreaTable", 0, 11), textBoxQuestSectionReqQSort.Text);
            } else
            {
                string newValue = CreatePopupSelection("Quest Sort Selection", ReadExcelCSV("QuestSort", 0, 1), textBoxQuestSectionReqQSort.Text.Trim('-'));

                textBoxQuestSectionReqQSort.Text = (textBoxQuestSectionReqQSort.Text == newValue || newValue == "0") ? textBoxQuestSectionReqQSort.Text : "-" + newValue;
            }
        }
        private void buttonQuestSectionReqFaction1_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionReqFaction1.Text = CreatePopupSelection("Objective Faction ID I", ReadExcelCSV("Faction", 0, 23), textBoxQuestSectionReqFaction1.Text);
        }
        private void buttonQuestSectionReqFaction2_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionReqFaction2.Text = CreatePopupSelection("Objective Faction ID II", ReadExcelCSV("Faction", 0, 23), textBoxQuestSectionReqFaction2.Text);
        }
        private void buttonQuestSectionReqMinRepF_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionReqMinRepF.Text = CreatePopupSelection("Minimum Reputation Faction", ReadExcelCSV("Faction", 0, 23), textBoxQuestSectionReqMinRepF.Text);
        }
        private void buttonQuestSectionReqMaxRepF_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionReqMaxRepF.Text = CreatePopupSelection("Maximum Reputation Faction", ReadExcelCSV("Faction", 0, 23), textBoxQuestSectionReqMaxRepF.Text);
        }
        private void buttonQuestSectionReqSkillID_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionReqSkillID.Text = CreatePopupSelection("Required Skill ID", ReadExcelCSV("SkillLine", 0, 3), textBoxQuestSectionReqSkillID.Text);
        }
        private void buttonQuestSectionQuestType_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionQuestType.Text = CreatePopupSelection("Quest Type", ReadExcelCSV("QuestType", 0, 1), textBoxQuestSectionQuestType.Text);
        }
        private void buttonQuestSectionQuestFlags_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionQuestFlags.Text = CreatePopupChecklist("Quest : Flags", ReadExcelCSV("QuestFlags", 0, 1), textBoxQuestSectionQuestFlags.Text, true);
        }
        private void buttonQuestSectionOtherSF_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionOtherSF.Text = CreatePopupChecklist("Quest : Special Flags", ReadExcelCSV("QuestSpecialFlags", 0, 1), textBoxQuestSectionOtherSF.Text, true);
        }
        private void buttonQuestSectionQuestInfo_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionQuestInfo.Text = CreatePopupSelection("Quest Info", ReadExcelCSV("QuestInfo", 0, 1), textBoxQuestSectionQuestInfo.Text);
        }
        private void buttonQuestSectionSourceSpellID_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionSourceSpellID.Text = CreatePopupSelection("Spells", ReadExcelCSV("Spells", 0, 1), textBoxQuestSectionSourceSpellID.Text);
        }
        // Section 2
        private void buttonQuestSectionReqNPCID1_Click(object sender, EventArgs e)
        {
            bool[] rButton = { false, true, false };

            textBoxQuestSectionReqNPCID1.Text = CreatePopupEntity(textBoxQuestSectionReqNPCID1.Text, rButton);
        }
        private void buttonQuestSectionReqItemID1_Click(object sender, EventArgs e)
        {
            bool[] rButton = { true, false, false };

            textBoxQuestSectionReqItemID1.Text = CreatePopupEntity(textBoxQuestSectionReqItemID1.Text, rButton);
        }
        private void buttonQuestSectionRewChoiceID1_Click(object sender, EventArgs e)
        {
            bool[] rButton = { true, false, false };

            textBoxQuestSectionRewChoiceID1.Text = CreatePopupEntity(textBoxQuestSectionRewChoiceID1.Text, rButton);
        }
        private void buttonQuestSectionRewItemID1_Click(object sender, EventArgs e)
        {
            bool[] rButton = { true, false, false };

            textBoxQuestSectionRewItemID1.Text = CreatePopupEntity(textBoxQuestSectionRewItemID1.Text, rButton);
        }
        private void buttonQuestSectionRewFactionID1_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionRewFactionID1.Text = CreatePopupSelection("Faction Selection", ReadExcelCSV("Faction", 0, 23), textBoxQuestSectionRewFactionID1.Text);
        }
        private void buttonQuestSectionRewOtherTitleID_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionRewOtherTitleID.Text = CreatePopupSelection("Title Selection", ReadExcelCSV("CharTitles", 0, 2), textBoxQuestSectionRewOtherTitleID.Text);
        }
        private void buttonQuestSectionRewSpell_Click(object sender, EventArgs e)
        {
            textBoxQuestSectionRewSpell.Text = CreatePopupSelection("Spell Selection", ReadExcelCSV("Spells", 0, 1), textBoxQuestSectionRewSpell.Text);
        }
        #endregion

        #endregion

        #region Game Object

        #region Functions
        private void DatabaseGameObjectSearch(string GameobjectEntryID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                var query = "SELECT * FROM gameobject_template WHERE entry = '" + GameobjectEntryID + "';";

                var gotTable = DatabaseSearch(connect, query);

                #region General
                textBoxGameObjectTempEntry.Text = gotTable.Tables[0].Rows[0]["entry"].ToString();
                textBoxGameObjectTempType.Text = gotTable.Tables[0].Rows[0]["type"].ToString();
                textBoxGameObjectTempDID.Text = gotTable.Tables[0].Rows[0]["displayId"].ToString();
                textBoxGameObjectTempName.Text = gotTable.Tables[0].Rows[0]["name"].ToString();
                textBoxGameObjectTempFaction.Text = gotTable.Tables[0].Rows[0]["faction"].ToString();
                textBoxGameObjectTempFlags.Text = gotTable.Tables[0].Rows[0]["flags"].ToString();
                textBoxGameObjectTempSize.Text = gotTable.Tables[0].Rows[0]["size"].ToString();
                #endregion
                #region Datas
                textBoxGameObjectTempD0.Text = gotTable.Tables[0].Rows[0]["Data0"].ToString();
                textBoxGameObjectTempD1.Text = gotTable.Tables[0].Rows[0]["Data1"].ToString();
                textBoxGameObjectTempD2.Text = gotTable.Tables[0].Rows[0]["Data2"].ToString();
                textBoxGameObjectTempD3.Text = gotTable.Tables[0].Rows[0]["Data3"].ToString();
                textBoxGameObjectTempD4.Text = gotTable.Tables[0].Rows[0]["Data4"].ToString();
                textBoxGameObjectTempD5.Text = gotTable.Tables[0].Rows[0]["Data5"].ToString();
                textBoxGameObjectTempD6.Text = gotTable.Tables[0].Rows[0]["Data6"].ToString();
                textBoxGameObjectTempD7.Text = gotTable.Tables[0].Rows[0]["Data7"].ToString();
                textBoxGameObjectTempD8.Text = gotTable.Tables[0].Rows[0]["Data8"].ToString();
                textBoxGameObjectTempD9.Text = gotTable.Tables[0].Rows[0]["Data9"].ToString();
                textBoxGameObjectTempD10.Text = gotTable.Tables[0].Rows[0]["Data10"].ToString();
                textBoxGameObjectTempD11.Text = gotTable.Tables[0].Rows[0]["Data11"].ToString();
                textBoxGameObjectTempD12.Text = gotTable.Tables[0].Rows[0]["Data12"].ToString();
                textBoxGameObjectTempD13.Text = gotTable.Tables[0].Rows[0]["Data13"].ToString();
                textBoxGameObjectTempD14.Text = gotTable.Tables[0].Rows[0]["Data14"].ToString();
                textBoxGameObjectTempD15.Text = gotTable.Tables[0].Rows[0]["Data15"].ToString();
                textBoxGameObjectTempD16.Text = gotTable.Tables[0].Rows[0]["Data16"].ToString();
                textBoxGameObjectTempD17.Text = gotTable.Tables[0].Rows[0]["Data17"].ToString();
                textBoxGameObjectTempD18.Text = gotTable.Tables[0].Rows[0]["Data18"].ToString();
                textBoxGameObjectTempD19.Text = gotTable.Tables[0].Rows[0]["Data19"].ToString();
                textBoxGameObjectTempD20.Text = gotTable.Tables[0].Rows[0]["Data20"].ToString();
                textBoxGameObjectTempD21.Text = gotTable.Tables[0].Rows[0]["Data21"].ToString();
                textBoxGameObjectTempD22.Text = gotTable.Tables[0].Rows[0]["Data22"].ToString();
                textBoxGameObjectTempD23.Text = gotTable.Tables[0].Rows[0]["Data23"].ToString();
                #endregion

                ConnectionClose(connect);
            }
        }
        private string DatabaseGameObjectTemplateGenerate()
        {
            var controls = new List<Tuple<string, string>>
            {
                Tuple.Create(textBoxGameObjectTempEntry.Text, "entry"),
                Tuple.Create(textBoxGameObjectTempType.Text, "type"),
                Tuple.Create(textBoxGameObjectTempDID.Text, "displayId"),
                Tuple.Create(textBoxGameObjectTempName.Text, "name"),
                Tuple.Create(textBoxGameObjectTempFaction.Text, "faction"),
                Tuple.Create(textBoxGameObjectTempFlags.Text, "flags"),
                Tuple.Create(textBoxGameObjectTempSize.Text, "size"),

                Tuple.Create(textBoxGameObjectTempD1.Text, "Data1"),
                Tuple.Create(textBoxGameObjectTempD2.Text, "Data2"),
                Tuple.Create(textBoxGameObjectTempD3.Text, "Data3"),
                Tuple.Create(textBoxGameObjectTempD4.Text, "Data4"),
                Tuple.Create(textBoxGameObjectTempD5.Text, "Data5"),
                Tuple.Create(textBoxGameObjectTempD6.Text, "Data6"),
                Tuple.Create(textBoxGameObjectTempD7.Text, "Data7"),
                Tuple.Create(textBoxGameObjectTempD8.Text, "Data8"),
                Tuple.Create(textBoxGameObjectTempD9.Text, "Data9"),
                Tuple.Create(textBoxGameObjectTempD10.Text, "Data10"),
                Tuple.Create(textBoxGameObjectTempD11.Text, "Data11"),
                Tuple.Create(textBoxGameObjectTempD12.Text, "Data12"),
                Tuple.Create(textBoxGameObjectTempD13.Text, "Data13"),
                Tuple.Create(textBoxGameObjectTempD14.Text, "Data14"),
                Tuple.Create(textBoxGameObjectTempD15.Text, "Data15"),
                Tuple.Create(textBoxGameObjectTempD16.Text, "Data16"),
                Tuple.Create(textBoxGameObjectTempD17.Text, "Data17"),
                Tuple.Create(textBoxGameObjectTempD18.Text, "Data18"),
                Tuple.Create(textBoxGameObjectTempD19.Text, "Data19"),
                Tuple.Create(textBoxGameObjectTempD20.Text, "Data20"),
                Tuple.Create(textBoxGameObjectTempD21.Text, "Data21"),
                Tuple.Create(textBoxGameObjectTempD22.Text, "Data22"),
                Tuple.Create(textBoxGameObjectTempD23.Text, "Data23")
            };

            string cName = "", values = "";

            foreach (var tuple in controls)
            {
                if (tuple == controls.Last())
                {
                    cName += "`" + tuple.Item2 + "`";
                    values += "'" + tuple.Item1 + "'";
                } else
                {
                    cName += "`" + tuple.Item2 + "`, ";
                    values += "'" + tuple.Item1 + "', ";
                }
            }

            string query = "REPLACE INTO `gameobject_template` (" + cName + ") VALUES (" + values + ");";

            return query;
        }
        #endregion
        #region Events
        private void buttonGameObjectSearchSearch_Click(object sender, EventArgs e)
        {
            bool totalSearch = CheckEmptyControls(tabPageGameObjectSearch); DialogResult dr;

            string query = "SELECT entry, TYPE, NAME FROM gameobject_template WHERE '1' = '1'";

            if (totalSearch)
            {
                dr = MessageBox.Show("You sure, you want to load them all?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
            else
            {
                query += DatabaseQueryFilter(textBoxGameObjectSearchEntry.Text, "entry");
                query += DatabaseQueryFilter(textBoxGameObjectSearchType.Text, "type");
                query += DatabaseQueryFilter(textBoxGameObjectSearchName.Text, "name");

                dr = DialogResult.OK;
            }

            if (dr == DialogResult.Cancel)
            {
                return;
            }

            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                query += " ORDER BY entry;";

                DataSet goTable = DatabaseSearch(connect, query);

                dataGridViewGameObjectSearch.DataSource = goTable.Tables[0];

                toolStripStatusLabelGameObjectSearchRows.Text = "Game Object(s): " + dataGridViewGameObjectSearch.Rows.Count.ToString();
                ConnectionClose(connect);
            }
        }
        private void dataGridViewGameObjectSearch_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewGameObjectSearch.Rows.Count > 0)
            {
                DatabaseGameObjectSearch(dataGridViewGameObjectSearch.SelectedCells[0].Value.ToString());

                tabControlCategoryGameObject.SelectedTab = tabPageGameObjectTemplate;
            }
        }
        private void buttonGameObjectTempGenerate_Click(object sender, EventArgs e)
        {
            if (textBoxGameObjectTempEntry.Text != string.Empty)
            {
                textBoxGameObjectScriptOutput.AppendText(DatabaseGameObjectTemplateGenerate());

                tabControlCategoryGameObject.SelectedTab = tabPageGameObjectScript;
            }
        }

        private void toolStripSplitButtonGONew_ButtonClick(object sender, EventArgs e)
        {
            var list = new List<Tuple<TextBox, string>>();

            list.Add(new Tuple<TextBox, string>(textBoxGameObjectTempName, ""));
            list.Add(new Tuple<TextBox, string>(textBoxGameObjectTempSize, "1"));
            list.Add(new Tuple<TextBox, string>(textBoxGameObjectTempAIName, ""));
            list.Add(new Tuple<TextBox, string>(textBoxGameObjectTempScriptName, ""));

            DefaultValuesGenerate(tabPageGameObjectTemplate);
            DefaultValuesOverride(list);

            tabControlCategoryGameObject.SelectedTab = tabPageGameObjectTemplate;
        }
        private void toolStripSplitButtonGODelete_ButtonClick(object sender, EventArgs e)
        {
            GenerateDeleteSelectedRow(dataGridViewGameObjectSearch, "gameobject_template", "entry", textBoxGameObjectScriptOutput);
        }

        private void toolStripSplitButtonGOScriptSQLGenerate_ButtonClick(object sender, EventArgs e)
        {
            GenerateSQLFile("GO_", textBoxCreatureTemplateEntry.Text + "-" + textBoxCreatureTemplateName.Text, textBoxCreatureScriptOutput);
        }
        private void toolStripSplitButtonGOScriptUpdate_ButtonClick(object sender, EventArgs e)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                toolStripStatusLabelGameObjectScriptRows.Text = "Row(s) Affected: " + DatabaseUpdate(connect, textBoxGameObjectScriptOutput.Text).ToString();

                ConnectionClose(connect);
            }
        }
        #endregion
        #region POPUP
        private void buttonGameObjectTempType_Click(object sender, EventArgs e)
        {
            textBoxGameObjectTempType.Text = CreatePopupSelection("Game Object Type Selection", ReadExcelCSV("GameObjectTypes", 0, 1), textBoxGameObjectTempType.Text);
        }
        private void buttonGameObjectTempFlags_Click(object sender, EventArgs e)
        {
            textBoxGameObjectTempFlags.Text = CreatePopupChecklist("Game Object Flags Selection", ReadExcelCSV("GameObjectFlags", 0, 1), textBoxGameObjectTempFlags.Text, true);
        }
        #endregion

        #endregion

        #region Item

        #region Functions
        private void DatabaseItemSearch(string itemEntryID)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                var query = "SELECT * FROM item_template WHERE entry = '" + itemEntryID + "';";

                var itTable = DatabaseSearch(connect, query);

                textBoxItemTempEntry.Text           = itTable.Tables[0].Rows[0]["entry"].ToString();
                textBoxItemTempTypeClass.Text       = itTable.Tables[0].Rows[0]["class"].ToString();
                textBoxItemTempSubclass.Text        = itTable.Tables[0].Rows[0]["subclass"].ToString();
                textBoxItemTempName.Text            = itTable.Tables[0].Rows[0]["name"].ToString();
                textBoxItemTempDisplayID.Text       = itTable.Tables[0].Rows[0]["displayid"].ToString();
                textBoxItemTempQuality.Text         = itTable.Tables[0].Rows[0]["Quality"].ToString();
                textBoxItemTempFlags.Text           = itTable.Tables[0].Rows[0]["Flags"].ToString();
                textBoxItemTempEFlags.Text          = itTable.Tables[0].Rows[0]["FlagsExtra"].ToString();
                textBoxItemTempBuyC.Text            = itTable.Tables[0].Rows[0]["BuyCount"].ToString();
                textBoxItemTempBuyP.Text            = itTable.Tables[0].Rows[0]["BuyPrice"].ToString();
                textBoxItemTempSellP.Text           = itTable.Tables[0].Rows[0]["SellPrice"].ToString();
                textBoxItemTempInventory.Text       = itTable.Tables[0].Rows[0]["InventoryType"].ToString();
                textBoxItemTempMaxC.Text            = itTable.Tables[0].Rows[0]["maxcount"].ToString();
                textBoxItemTempContainer.Text       = itTable.Tables[0].Rows[0]["ContainerSlots"].ToString();

                textBoxItemTempReqClass.Text        = itTable.Tables[0].Rows[0]["AllowableClass"].ToString();
                textBoxItemTempReqRace.Text         = itTable.Tables[0].Rows[0]["AllowableRace"].ToString();
                textBoxItemTempReqItemLevel.Text    = itTable.Tables[0].Rows[0]["ItemLevel"].ToString();
                textBoxItemTempReqLevel.Text        = itTable.Tables[0].Rows[0]["RequiredLevel"].ToString();
                textBoxItemTempReqSkill.Text        = itTable.Tables[0].Rows[0]["RequiredSkill"].ToString();
                textBoxItemTempReqSkillRank.Text    = itTable.Tables[0].Rows[0]["RequiredSkillRank"].ToString();
                textBoxItemTempReqSpell.Text        = itTable.Tables[0].Rows[0]["requiredspell"].ToString();
                textBoxItemTempReqHonorRank.Text    = itTable.Tables[0].Rows[0]["requiredhonorrank"].ToString();
                textBoxItemTempReqCityRank.Text     = itTable.Tables[0].Rows[0]["RequiredCityRank"].ToString();
                textBoxItemTempReqRepFaction.Text   = itTable.Tables[0].Rows[0]["RequiredReputationFaction"].ToString();
                textBoxItemTempReqRepRank.Text      = itTable.Tables[0].Rows[0]["RequiredReputationRank"].ToString();
                textBoxItemTempReqDisenchant.Text   = itTable.Tables[0].Rows[0]["RequiredDisenchantSkill"].ToString();

                textBoxItemTempStatsC.Text          = itTable.Tables[0].Rows[0]["StatsCount"].ToString();
                textBoxItemTempStatsType1.Text      = itTable.Tables[0].Rows[0]["stat_type1"].ToString();
                textBoxItemTempStatsValue1.Text     = itTable.Tables[0].Rows[0]["stat_value1"].ToString();
                textBoxItemTempStatsType2.Text      = itTable.Tables[0].Rows[0]["stat_type2"].ToString();
                textBoxItemTempStatsValue2.Text     = itTable.Tables[0].Rows[0]["stat_value2"].ToString();
                textBoxItemTempStatsType3.Text      = itTable.Tables[0].Rows[0]["stat_type3"].ToString();
                textBoxItemTempStatsValue3.Text     = itTable.Tables[0].Rows[0]["stat_value3"].ToString();
                textBoxItemTempStatsType4.Text      = itTable.Tables[0].Rows[0]["stat_type4"].ToString();
                textBoxItemTempStatsValue4.Text     = itTable.Tables[0].Rows[0]["stat_value4"].ToString();
                textBoxItemTempStatsType5.Text      = itTable.Tables[0].Rows[0]["stat_type5"].ToString();
                textBoxItemTempStatsValue5.Text     = itTable.Tables[0].Rows[0]["stat_value5"].ToString();
                textBoxItemTempStatsType6.Text      = itTable.Tables[0].Rows[0]["stat_type6"].ToString();
                textBoxItemTempStatsValue6.Text     = itTable.Tables[0].Rows[0]["stat_value6"].ToString();
                textBoxItemTempStatsType7.Text      = itTable.Tables[0].Rows[0]["stat_type7"].ToString();
                textBoxItemTempStatsValue7.Text     = itTable.Tables[0].Rows[0]["stat_value7"].ToString();
                textBoxItemTempStatsType8.Text      = itTable.Tables[0].Rows[0]["stat_type8"].ToString();
                textBoxItemTempStatsValue8.Text     = itTable.Tables[0].Rows[0]["stat_value8"].ToString();
                textBoxItemTempStatsType9.Text      = itTable.Tables[0].Rows[0]["stat_type9"].ToString();
                textBoxItemTempStatsValue9.Text     = itTable.Tables[0].Rows[0]["stat_value9"].ToString();
                textBoxItemTempStatsType10.Text     = itTable.Tables[0].Rows[0]["stat_type10"].ToString();
                textBoxItemTempStatsValue10.Text    = itTable.Tables[0].Rows[0]["stat_value10"].ToString();
                textBoxItemTempStatsScaleDist.Text  = itTable.Tables[0].Rows[0]["ScalingStatDistribution"].ToString();
                textBoxItemTempStatsScaleValue.Text = itTable.Tables[0].Rows[0]["ScalingStatValue"].ToString();

                textBoxItemTempDmgType1.Text        = itTable.Tables[0].Rows[0]["dmg_type1"].ToString();
                textBoxItemTempDmgMin1.Text         = itTable.Tables[0].Rows[0]["dmg_min1"].ToString();
                textBoxItemTempDmgMax1.Text         = itTable.Tables[0].Rows[0]["dmg_max1"].ToString();
                textBoxItemTempDmgType2.Text        = itTable.Tables[0].Rows[0]["dmg_type2"].ToString();
                textBoxItemTempDmgMin2.Text         = itTable.Tables[0].Rows[0]["dmg_min2"].ToString();
                textBoxItemTempDmgMax2.Text         = itTable.Tables[0].Rows[0]["dmg_max2"].ToString();

                textBoxItemTempResisHoly.Text       = itTable.Tables[0].Rows[0]["holy_res"].ToString();
                textBoxItemTempResisFire.Text       = itTable.Tables[0].Rows[0]["fire_res"].ToString();
                textBoxItemTempResisNature.Text     = itTable.Tables[0].Rows[0]["nature_res"].ToString();
                textBoxItemTempResisFrost.Text      = itTable.Tables[0].Rows[0]["frost_res"].ToString();
                textBoxItemTempResisShadow.Text     = itTable.Tables[0].Rows[0]["shadow_res"].ToString();
                textBoxItemTempResisArcane.Text     = itTable.Tables[0].Rows[0]["arcane_res"].ToString();

                textBoxItemTempSpellID1.Text        = itTable.Tables[0].Rows[0]["spellid_1"].ToString();
                textBoxItemTempTrigger1.Text        = itTable.Tables[0].Rows[0]["spelltrigger_1"].ToString();
                textBoxItemTempCharges1.Text        = itTable.Tables[0].Rows[0]["spellcharges_1"].ToString();
                textBoxItemTempRate1.Text           = itTable.Tables[0].Rows[0]["spellppmRate_1"].ToString();
                textBoxItemTempCD1.Text             = itTable.Tables[0].Rows[0]["spellcooldown_1"].ToString();
                textBoxItemTempCategory1.Text       = itTable.Tables[0].Rows[0]["spellcategory_1"].ToString();
                textBoxItemTempCategoryCD1.Text     = itTable.Tables[0].Rows[0]["spellcategorycooldown_1"].ToString();
                textBoxItemTempSpellID2.Text        = itTable.Tables[0].Rows[0]["spellid_2"].ToString();
                textBoxItemTempTrigger2.Text        = itTable.Tables[0].Rows[0]["spelltrigger_2"].ToString();
                textBoxItemTempCharges2.Text        = itTable.Tables[0].Rows[0]["spellcharges_2"].ToString();
                textBoxItemTempRate2.Text           = itTable.Tables[0].Rows[0]["spellppmRate_2"].ToString();
                textBoxItemTempCD2.Text             = itTable.Tables[0].Rows[0]["spellcooldown_2"].ToString();
                textBoxItemTempCategory2.Text       = itTable.Tables[0].Rows[0]["spellcategory_2"].ToString();
                textBoxItemTempCategoryCD2.Text     = itTable.Tables[0].Rows[0]["spellcategorycooldown_2"].ToString();
                textBoxItemTempSpellID3.Text        = itTable.Tables[0].Rows[0]["spellid_3"].ToString();
                textBoxItemTempTrigger3.Text        = itTable.Tables[0].Rows[0]["spelltrigger_3"].ToString();
                textBoxItemTempCharges3.Text        = itTable.Tables[0].Rows[0]["spellcharges_3"].ToString();
                textBoxItemTempRate3.Text           = itTable.Tables[0].Rows[0]["spellppmRate_3"].ToString();
                textBoxItemTempCD3.Text             = itTable.Tables[0].Rows[0]["spellcooldown_3"].ToString();
                textBoxItemTempCategory3.Text       = itTable.Tables[0].Rows[0]["spellcategory_3"].ToString();
                textBoxItemTempCategoryCD3.Text     = itTable.Tables[0].Rows[0]["spellcategorycooldown_3"].ToString();
                textBoxItemTempSpellID4.Text        = itTable.Tables[0].Rows[0]["spellid_4"].ToString();
                textBoxItemTempTrigger4.Text        = itTable.Tables[0].Rows[0]["spelltrigger_4"].ToString();
                textBoxItemTempCharges4.Text        = itTable.Tables[0].Rows[0]["spellcharges_4"].ToString();
                textBoxItemTempRate4.Text           = itTable.Tables[0].Rows[0]["spellppmRate_4"].ToString();
                textBoxItemTempCD4.Text             = itTable.Tables[0].Rows[0]["spellcooldown_4"].ToString();
                textBoxItemTempCategory4.Text       = itTable.Tables[0].Rows[0]["spellcategory_4"].ToString();
                textBoxItemTempCategoryCD4.Text     = itTable.Tables[0].Rows[0]["spellcategorycooldown_4"].ToString();
                textBoxItemTempSpellID5.Text        = itTable.Tables[0].Rows[0]["spellid_5"].ToString();
                textBoxItemTempTrigger5.Text        = itTable.Tables[0].Rows[0]["spelltrigger_5"].ToString();
                textBoxItemTempCharges5.Text        = itTable.Tables[0].Rows[0]["spellcharges_5"].ToString();
                textBoxItemTempRate5.Text           = itTable.Tables[0].Rows[0]["spellppmRate_5"].ToString();
                textBoxItemTempCD5.Text             = itTable.Tables[0].Rows[0]["spellcooldown_5"].ToString();
                textBoxItemTempCategory5.Text       = itTable.Tables[0].Rows[0]["spellcategory_5"].ToString();
                textBoxItemTempCategoryCD5.Text     = itTable.Tables[0].Rows[0]["spellcategorycooldown_5"].ToString();

                textBoxItemTempColor1.Text          = itTable.Tables[0].Rows[0]["socketColor_1"].ToString();
                textBoxItemTempContent1.Text        = itTable.Tables[0].Rows[0]["socketContent_1"].ToString();
                textBoxItemTempColor2.Text          = itTable.Tables[0].Rows[0]["socketColor_2"].ToString();
                textBoxItemTempContent2.Text        = itTable.Tables[0].Rows[0]["socketContent_2"].ToString();
                textBoxItemTempColor3.Text          = itTable.Tables[0].Rows[0]["socketColor_3"].ToString();
                textBoxItemTempContent3.Text        = itTable.Tables[0].Rows[0]["socketContent_3"].ToString();
                textBoxItemTempSocketBonus.Text     = itTable.Tables[0].Rows[0]["socketBonus"].ToString();
                textBoxItemTempGemProper.Text       = itTable.Tables[0].Rows[0]["GemProperties"].ToString();

                textBoxItemTempDelay.Text           = itTable.Tables[0].Rows[0]["delay"].ToString();
                textBoxItemTempAmmoType.Text        = itTable.Tables[0].Rows[0]["ammo_type"].ToString();
                textBoxItemTempRangedMod.Text       = itTable.Tables[0].Rows[0]["RangedModRange"].ToString();
                textBoxItemTempBonding.Text         = itTable.Tables[0].Rows[0]["bonding"].ToString();
                textBoxItemTempDescription.Text     = itTable.Tables[0].Rows[0]["description"].ToString();
                textBoxItemTempPageText.Text        = itTable.Tables[0].Rows[0]["PageText"].ToString();
                textBoxItemTempLanguage.Text        = itTable.Tables[0].Rows[0]["LanguageID"].ToString();
                textBoxItemTempPageMaterial.Text    = itTable.Tables[0].Rows[0]["PageMaterial"].ToString();
                textBoxItemTempStartQuest.Text      = itTable.Tables[0].Rows[0]["startquest"].ToString();
                textBoxItemTempLockID.Text          = itTable.Tables[0].Rows[0]["lockid"].ToString();
                textBoxItemTempMaterial.Text        = itTable.Tables[0].Rows[0]["Material"].ToString();
                textBoxItemTempSheath.Text          = itTable.Tables[0].Rows[0]["sheath"].ToString();
                textBoxItemTempProperty.Text        = itTable.Tables[0].Rows[0]["RandomProperty"].ToString();
                textBoxItemTempSuffix.Text          = itTable.Tables[0].Rows[0]["RandomSuffix"].ToString();
                textBoxItemTempBlock.Text           = itTable.Tables[0].Rows[0]["block"].ToString();
                textBoxItemTempItemSet.Text         = itTable.Tables[0].Rows[0]["itemset"].ToString();
                textBoxItemTempDurability.Text      = itTable.Tables[0].Rows[0]["MaxDurability"].ToString();
                textBoxItemTempArea.Text            = itTable.Tables[0].Rows[0]["area"].ToString();
                textBoxItemTempMap.Text             = itTable.Tables[0].Rows[0]["Map"].ToString();
                textBoxItemTempDisenchantID.Text    = itTable.Tables[0].Rows[0]["DisenchantID"].ToString();
                textBoxItemTempModifier.Text        = itTable.Tables[0].Rows[0]["ArmorDamageModifier"].ToString();
                textBoxItemTempHolidayID.Text       = itTable.Tables[0].Rows[0]["HolidayId"].ToString();
                textBoxItemTempFoodType.Text        = itTable.Tables[0].Rows[0]["FoodType"].ToString();
                textBoxItemTempFlagsC.Text          = itTable.Tables[0].Rows[0]["flagsCustom"].ToString();
                textBoxItemTempDuration.Text        = itTable.Tables[0].Rows[0]["duration"].ToString();
                textBoxItemTempLimitCate.Text       = itTable.Tables[0].Rows[0]["ItemLimitCategory"].ToString();
                textBoxItemTempMoneyMin.Text        = itTable.Tables[0].Rows[0]["minMoneyLoot"].ToString();
                textBoxItemTempMoneyMax.Text        = itTable.Tables[0].Rows[0]["maxMoneyLoot"].ToString();
                textBoxItemTempBagFamily.Text       = itTable.Tables[0].Rows[0]["BagFamily"].ToString();
                textBoxItemTempTotemCategory.Text   = itTable.Tables[0].Rows[0]["TotemCategory"].ToString();

                ConnectionClose(connect);
            }
        }
        private string DatabaseItemTempGenerate()
        {
            #region Controls
            var controls = new List<Tuple<string, string>> {
                Tuple.Create(textBoxItemTempEntry.Text, "entry"),
                Tuple.Create(textBoxItemTempTypeClass.Text, "class"),
                Tuple.Create(textBoxItemTempSubclass.Text, "subclass"),
                Tuple.Create(textBoxItemTempName.Text, "name"),
                Tuple.Create(textBoxItemTempDisplayID.Text, "displayid"),
                Tuple.Create(textBoxItemTempQuality.Text, "Quality"),
                Tuple.Create(textBoxItemTempFlags.Text, "Flags"),
                Tuple.Create(textBoxItemTempEFlags.Text, "FlagsExtra"),
                Tuple.Create(textBoxItemTempBuyC.Text, "BuyCount"),
                Tuple.Create(textBoxItemTempBuyP.Text, "BuyPrice"),
                Tuple.Create(textBoxItemTempSellP.Text, "SellPrice"),
                Tuple.Create(textBoxItemTempInventory.Text, "InventoryType"),
                Tuple.Create(textBoxItemTempMaxC.Text, "maxcount"),
                Tuple.Create(textBoxItemTempContainer.Text, "ContainerSlots"),

                Tuple.Create(textBoxItemTempReqClass.Text, "AllowableClass"),
                Tuple.Create(textBoxItemTempReqRace.Text, "AllowableRace"),
                Tuple.Create(textBoxItemTempReqItemLevel.Text, "ItemLevel"),
                Tuple.Create(textBoxItemTempReqLevel.Text, "RequiredLevel"),
                Tuple.Create(textBoxItemTempReqSkill.Text, "RequiredSkill"),
                Tuple.Create(textBoxItemTempReqSkillRank.Text, "RequiredSkillRank"),
                Tuple.Create(textBoxItemTempReqSpell.Text, "requiredspell"),
                Tuple.Create(textBoxItemTempReqHonorRank.Text, "requiredhonorrank"),
                Tuple.Create(textBoxItemTempReqCityRank.Text, "RequiredCityRank"),
                Tuple.Create(textBoxItemTempReqRepFaction.Text, "RequiredReputationFaction"),
                Tuple.Create(textBoxItemTempReqRepRank.Text, "RequiredReputationRank"),
                Tuple.Create(textBoxItemTempReqDisenchant.Text, "RequiredDisenchantSkill"),

                Tuple.Create(textBoxItemTempStatsC.Text, "StatsCount"),
                Tuple.Create(textBoxItemTempStatsType1.Text, "stat_type1"),
                Tuple.Create(textBoxItemTempStatsValue1.Text, "stat_value1"),
                Tuple.Create(textBoxItemTempStatsType2.Text, "stat_type2"),
                Tuple.Create(textBoxItemTempStatsValue2.Text, "stat_value2"),
                Tuple.Create(textBoxItemTempStatsType3.Text, "stat_type3"),
                Tuple.Create(textBoxItemTempStatsValue3.Text, "stat_value3"),
                Tuple.Create(textBoxItemTempStatsType4.Text, "stat_type4"),
                Tuple.Create(textBoxItemTempStatsValue4.Text, "stat_value4"),
                Tuple.Create(textBoxItemTempStatsType5.Text, "stat_type5"),
                Tuple.Create(textBoxItemTempStatsValue5.Text, "stat_value5"),
                Tuple.Create(textBoxItemTempStatsType6.Text, "stat_type6"),
                Tuple.Create(textBoxItemTempStatsValue6.Text, "stat_value6"),
                Tuple.Create(textBoxItemTempStatsType7.Text, "stat_type7"),
                Tuple.Create(textBoxItemTempStatsValue7.Text, "stat_value7"),
                Tuple.Create(textBoxItemTempStatsType8.Text, "stat_type8"),
                Tuple.Create(textBoxItemTempStatsValue8.Text, "stat_value8"),
                Tuple.Create(textBoxItemTempStatsType9.Text, "stat_type9"),
                Tuple.Create(textBoxItemTempStatsValue9.Text, "stat_value9"),
                Tuple.Create(textBoxItemTempStatsType10.Text, "stat_type10"),
                Tuple.Create(textBoxItemTempStatsValue10.Text, "stat_value10"),
                Tuple.Create(textBoxItemTempStatsScaleDist.Text, "ScalingStatDistribution"),
                Tuple.Create(textBoxItemTempStatsScaleValue.Text, "ScalingStatValue"),

                Tuple.Create(textBoxItemTempSpellID1.Text, "spellid_1"),
                Tuple.Create(textBoxItemTempTrigger1.Text, "spelltrigger_1"),
                Tuple.Create(textBoxItemTempCharges1.Text, "spellcharges_1"),
                Tuple.Create(textBoxItemTempRate1.Text, "spellppmRate_1"),
                Tuple.Create(textBoxItemTempCD1.Text, "spellcooldown_1"),
                Tuple.Create(textBoxItemTempCategory1.Text, "spellcategory_1"),
                Tuple.Create(textBoxItemTempCategoryCD1.Text, "spellcategorycooldown_1"),
                Tuple.Create(textBoxItemTempSpellID2.Text, "spellid_2"),
                Tuple.Create(textBoxItemTempTrigger2.Text, "spelltrigger_2"),
                Tuple.Create(textBoxItemTempCharges2.Text, "spellcharges_2"),
                Tuple.Create(textBoxItemTempRate2.Text, "spellppmRate_2"),
                Tuple.Create(textBoxItemTempCD2.Text, "spellcooldown_2"),
                Tuple.Create(textBoxItemTempCategory2.Text, "spellcategory_2"),
                Tuple.Create(textBoxItemTempCategoryCD2.Text, "spellcategorycooldown_2"),
                Tuple.Create(textBoxItemTempSpellID3.Text, "spellid_3"),
                Tuple.Create(textBoxItemTempTrigger3.Text, "spelltrigger_3"),
                Tuple.Create(textBoxItemTempCharges3.Text, "spellcharges_3"),
                Tuple.Create(textBoxItemTempRate3.Text, "spellppmRate_3"),
                Tuple.Create(textBoxItemTempCD3.Text, "spellcooldown_3"),
                Tuple.Create(textBoxItemTempCategory3.Text, "spellcategory_3"),
                Tuple.Create(textBoxItemTempCategoryCD3.Text, "spellcategorycooldown_3"),
                Tuple.Create(textBoxItemTempSpellID4.Text, "spellid_4"),
                Tuple.Create(textBoxItemTempTrigger4.Text, "spelltrigger_4"),
                Tuple.Create(textBoxItemTempCharges4.Text, "spellcharges_4"),
                Tuple.Create(textBoxItemTempRate4.Text, "spellppmRate_4"),
                Tuple.Create(textBoxItemTempCD4.Text, "spellcooldown_4"),
                Tuple.Create(textBoxItemTempCategory4.Text, "spellcategory_4"),
                Tuple.Create(textBoxItemTempCategoryCD4.Text, "spellcategorycooldown_4"),
                Tuple.Create(textBoxItemTempSpellID5.Text, "spellid_5"),
                Tuple.Create(textBoxItemTempTrigger5.Text, "spelltrigger_5"),
                Tuple.Create(textBoxItemTempCharges5.Text, "spellcharges_5"),
                Tuple.Create(textBoxItemTempRate5.Text, "spellppmRate_5"),
                Tuple.Create(textBoxItemTempCD5.Text, "spellcooldown_5"),
                Tuple.Create(textBoxItemTempCategory5.Text, "spellcategory_5"),
                Tuple.Create(textBoxItemTempCategoryCD5.Text, "spellcategorycooldown_5"),

                Tuple.Create(textBoxItemTempDmgType1.Text, "dmg_type1"),
                Tuple.Create(textBoxItemTempDmgMin1.Text, "dmg_min1"),
                Tuple.Create(textBoxItemTempDmgMax1.Text, "dmg_max1"),
                Tuple.Create(textBoxItemTempDmgType2.Text, "dmg_type2"),
                Tuple.Create(textBoxItemTempDmgMin2.Text, "dmg_min2"),
                Tuple.Create(textBoxItemTempDmgMax2.Text, "dmg_max2"),

                Tuple.Create(textBoxItemTempResisHoly.Text, "holy_res"),
                Tuple.Create(textBoxItemTempResisFire.Text, "fire_res"),
                Tuple.Create(textBoxItemTempResisNature.Text, "nature_res"),
                Tuple.Create(textBoxItemTempResisFrost.Text, "frost_res"),
                Tuple.Create(textBoxItemTempResisShadow.Text, "shadow_res"),
                Tuple.Create(textBoxItemTempResisArcane.Text, "arcane_res"),

                Tuple.Create(textBoxItemTempColor1.Text, "socketColor_1"),
                Tuple.Create(textBoxItemTempContent1.Text, "socketContent_1"),
                Tuple.Create(textBoxItemTempColor2.Text, "socketColor_2"),
                Tuple.Create(textBoxItemTempContent2.Text, "socketContent_2"),
                Tuple.Create(textBoxItemTempColor3.Text, "socketColor_3"),
                Tuple.Create(textBoxItemTempContent3.Text, "socketContent_3"),
                Tuple.Create(textBoxItemTempSocketBonus.Text, "socketBonus"),
                Tuple.Create(textBoxItemTempGemProper.Text, "GemProperties"),

                Tuple.Create(textBoxItemTempDelay.Text, "delay"),
                Tuple.Create(textBoxItemTempAmmoType.Text, "ammo_type"),
                Tuple.Create(textBoxItemTempRangedMod.Text, "RangedModRange"),
                Tuple.Create(textBoxItemTempBonding.Text, "bonding"),
                Tuple.Create(textBoxItemTempDescription.Text, "description"),
                Tuple.Create(textBoxItemTempPageText.Text, "PageText"),
                Tuple.Create(textBoxItemTempLanguage.Text, "LanguageID"),
                Tuple.Create(textBoxItemTempPageMaterial.Text, "PageMaterial"),
                Tuple.Create(textBoxItemTempStartQuest.Text, "startquest"),
                Tuple.Create(textBoxItemTempLockID.Text, "lockid"),
                Tuple.Create(textBoxItemTempMaterial.Text, "Material"),
                Tuple.Create(textBoxItemTempSheath.Text, "sheath"),
                Tuple.Create(textBoxItemTempProperty.Text, "RandomProperty"),
                Tuple.Create(textBoxItemTempSuffix.Text, "RandomSuffix"),
                Tuple.Create(textBoxItemTempBlock.Text, "block"),
                Tuple.Create(textBoxItemTempItemSet.Text, "itemset"),
                Tuple.Create(textBoxItemTempDurability.Text, "MaxDurability"),
                Tuple.Create(textBoxItemTempArea.Text, "area"),
                Tuple.Create(textBoxItemTempMap.Text, "Map"),
                Tuple.Create(textBoxItemTempDisenchantID.Text, "DisenchantID"),
                Tuple.Create(textBoxItemTempModifier.Text, "ArmorDamageModifier"),
                Tuple.Create(textBoxItemTempHolidayID.Text, "HolidayId"),
                Tuple.Create(textBoxItemTempFoodType.Text, "FoodType"),
                Tuple.Create(textBoxItemTempFlagsC.Text, "flagsCustom"),
                Tuple.Create(textBoxItemTempDuration.Text, "duration"),
                Tuple.Create(textBoxItemTempLimitCate.Text, "ItemLimitCategory"),
                Tuple.Create(textBoxItemTempMoneyMin.Text, "minMoneyLoot"),
                Tuple.Create(textBoxItemTempMoneyMax.Text, "maxMoneyLoot"),
                Tuple.Create(textBoxItemTempBagFamily.Text, "BagFamily"),
                Tuple.Create(textBoxItemTempTotemCategory.Text, "TotemCategory")
        };
            #endregion

            string cNames = "", values = "";

            foreach (var tuple in controls)
            {
                if (tuple == controls.Last())
                {
                    cNames += "`" + tuple.Item2 + "`";
                    values += "'" + tuple.Item1 + "'";
                } else
                {
                    cNames += "`" + tuple.Item2 + "`, ";
                    values += "'" + tuple.Item1 + "', ";
                }
            }

            string query = "REPLACE INTO `item_template` (" + cNames + ") VALUES (" + values + ");";

            return query;
        }
        #endregion
        #region Events
        private void buttonItemSearchSearch_Click(object sender, EventArgs e)
        {
            bool totalSearch = CheckEmptyControls(tabPageItemSearch); DialogResult dr;
            string query = "SELECT entry, NAME, class, subclass, quality, description, requiredlevel, itemlevel FROM item_template WHERE '1' = '1'";

            if (totalSearch)
            {
                dr = MessageBox.Show("You sure, you want to load them all?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
            else
            {
                var qualityValue = (comboBoxItemSearchQuality.SelectedIndex.ToString() == "-1") ? "" : comboBoxItemSearchQuality.SelectedIndex.ToString();

                query += DatabaseQueryFilter(textBoxItemSearchEntry.Text, "entry");
                query += DatabaseQueryFilter(textBoxItemSearchName.Text, "name");
                query += DatabaseQueryFilter(textBoxItemSearchClass.Text, "class");
                query += DatabaseQueryFilter(textBoxItemSearchSubclass.Text, "subclass");
                query += DatabaseQueryFilter(qualityValue, "quality");
                query += DatabaseQueryFilter(textBoxItemSearchDescription.Text, "description");
                query += DatabaseQueryFilter(textBoxItemSearchReqLevel.Text, "requiredlevel");
                query += DatabaseQueryFilter(textBoxItemSearchILevel.Text, "itemlevel");

                dr = DialogResult.OK;
            }

            if (dr == DialogResult.Cancel)
            {
                return;
            }

            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                query += " ORDER BY entry;";

                DataSet itTable = DatabaseSearch(connect, query);

                dataGridViewItemSearch.DataSource = itTable.Tables[0];

                toolStripStatusLabelItemSearchRows.Text = "Item(s) found: " + dataGridViewItemSearch.Rows.Count.ToString();
                ConnectionClose(connect);
            }
        }
        private void dataGridViewItemSearch_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewItemSearch.Rows.Count > 0)
            {
                tabControlCategoryItem.SelectedTab = tabPageItemTemplate;

                DatabaseItemSearch(dataGridViewItemSearch.SelectedCells[0].Value.ToString());

                dataGridViewItemLoot.DataSource = DatabaseItemNameColumn("item_loot_template", "entry", dataGridViewItemSearch.SelectedCells[0].Value.ToString(), 1, true);
                dataGridViewItemDE.DataSource = DatabaseItemNameColumn("disenchant_loot_template", "entry", textBoxItemTempDisenchantID.Text.Trim(), 1, true);
                dataGridViewItemMill.DataSource = DatabaseItemNameColumn("milling_loot_template", "entry", dataGridViewItemSearch.SelectedCells[0].Value.ToString(), 1, true);
                dataGridViewItemProspect.DataSource = DatabaseItemNameColumn("prospecting_loot_template", "entry", dataGridViewItemSearch.SelectedCells[0].Value.ToString(), 1, true);
            }
        }
        private void buttonItemTempGenerate_Click(object sender, EventArgs e)
        {
            if (textBoxItemTempEntry.Text != string.Empty)
            {
                textBoxItemScriptOutput.AppendText(DatabaseItemTempGenerate());

                tabControlCategoryItem.SelectedTab = tabPageItemScript;
            }
        }

        private void toolStripSplitButtonItemNew_ButtonClick(object sender, EventArgs e)
        {
            var list = new List<Tuple<TextBox, string>>();

            list.Add(new Tuple<TextBox, string>(textBoxItemTempName, ""));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempDescription, ""));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempReqRace, "1791"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempReqClass, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCD1, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCategoryCD1, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCD2, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCategoryCD2, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCD3, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCategoryCD3, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCD4, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCategoryCD4, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCD5, "-1"));
            list.Add(new Tuple<TextBox, string>(textBoxItemTempCategoryCD5, "-1"));

            DefaultValuesGenerate(tabPageItemTemplate);
            DefaultValuesOverride(list);

            tabControlCategoryItem.SelectedTab = tabPageItemTemplate;
        }
        private void toolStripSplitButtonItemDelete_ButtonClick(object sender, EventArgs e)
        {
            GenerateDeleteSelectedRow(dataGridViewItemSearch, "item_template", "entry", textBoxItemScriptOutput);
        }

        private void toolStripSplitButtonItemScriptSQLGenerate_ButtonClick(object sender, EventArgs e)
        {
            GenerateSQLFile("ITEM_", textBoxItemTempEntry.Text.Trim() + "-" + textBoxItemTempName.Text.Trim(), textBoxItemScriptOutput);
        }
        private void toolStripSplitButtonItemScriptUpdate_ButtonClick(object sender, EventArgs e)
        {
            var connect = new MySqlConnection(DatabaseString(FormMySQL.DatabaseWorld));

            if (ConnectionOpen(connect))
            {
                toolStripStatusLabelItemScriptRows.Text = "Row(s) Affected: " + DatabaseUpdate(connect, textBoxItemScriptOutput.Text).ToString();

                ConnectionClose(connect);
            }
        }

        #region Loot
        private void buttonItemLootAdd_Click(object sender, EventArgs e)
        {
            var values = new object[] {
                textBoxItemLootEntry.Text,
                textBoxItemLootItemID.Text,
                textBoxItemLootReference.Text,
                textBoxItemLootChance.Text,
                textBoxItemLootQR.Text,
                textBoxItemLootLM.Text,
                textBoxItemLootGID.Text,
                textBoxItemLootMIC.Text,
                textBoxItemLootMAC.Text
            };

            if (textBoxItemLootEntry.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewItemLoot.DataSource;
                existingData.Rows.Add(values);
                dataGridViewItemLoot.DataSource = existingData;
            }
        }
        private void buttonItemLootRefresh_Click(object sender, EventArgs e)
        {
            dataGridViewItemLoot.DataSource = DatabaseItemNameColumn("item_loot_template", "entry", (textBoxItemLootEntry.Text.Trim() != "") ? textBoxItemLootEntry.Text : textBoxItemTempEntry.Text, 1, true);
        }
        private void buttonItemLootDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewItemLoot.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewItemLoot.SelectedRows)
                {
                    dataGridViewItemLoot.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonItemLootGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("item_loot_template", dataGridViewItemLoot, textBoxItemScriptOutput);
        }
        #endregion
        #region Disenchant
        private void buttonItemDEAdd_Click(object sender, EventArgs e)
        {
            var values = new string[] {
                textBoxItemDEID.Text,
                textBoxItemDEItemID.Text,
                textBoxItemDEReference.Text,
                textBoxItemDEChance.Text,
                textBoxItemDEQR.Text,
                textBoxItemDELM.Text,
                textBoxItemDEGID.Text,
                textBoxItemDEMIC.Text,
                textBoxItemDEMAC.Text
            };

            if (textBoxItemDEID.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewItemDE.DataSource;
                existingData.Rows.Add(values);
                dataGridViewItemDE.DataSource = existingData;
            }
        }
        private void buttonItemDERefresh_Click(object sender, EventArgs e)
        {

            dataGridViewItemDE.DataSource = DatabaseItemNameColumn("disenchant_loot_template", "entry", (textBoxItemDEID.Text.Trim() != "") ? textBoxItemDEID.Text : textBoxItemTempDisenchantID.Text, 1, true);
        }
        private void buttonItemDEDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewItemDE.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewItemDE.SelectedRows)
                {
                    dataGridViewItemDE.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonItemDEGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("disenchant_loot_template", dataGridViewItemProspect, textBoxItemScriptOutput);
        }
        #endregion
        #region Milling
        private void buttonItemMillAdd_Click(object sender, EventArgs e)
        {
            var values = new string[] {
                textBoxItemMillEntry.Text,
                textBoxItemMillItemID.Text,
                textBoxItemMillReference.Text,
                textBoxItemMillChance.Text,
                textBoxItemMillQR.Text,
                textBoxItemMillLM.Text,
                textBoxItemMillGID.Text,
                textBoxItemMillMIC.Text,
                textBoxItemMillMAC.Text
            };

            if (textBoxItemMillEntry.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewItemMill.DataSource;
                existingData.Rows.Add(values);
                dataGridViewItemMill.DataSource = existingData;
            }
        }
        private void buttonItemMillRefresh_Click(object sender, EventArgs e)
        {
            dataGridViewItemMill.DataSource = DatabaseItemNameColumn("milling_loot_template", "entry", (textBoxItemMillEntry.Text.Trim() != "") ? textBoxItemMillEntry.Text.Trim() : textBoxItemTempEntry.Text, 1, true);
        }
        private void buttonItemMillDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewItemMill.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewItemMill.SelectedRows)
                {
                    dataGridViewItemMill.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonItemMillGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("milling_loot_template", dataGridViewItemMill, textBoxItemScriptOutput);
        }
        #endregion
        #region Prospecting
        private void buttonItemProspectAdd_Click(object sender, EventArgs e)
        {
            var values = new string[] {
                textBoxItemProspectEntry.Text,
                textBoxItemProspectItemID.Text,
                textBoxItemProspectReference.Text,
                textBoxItemProspectChance.Text,
                textBoxItemProspectQR.Text,
                textBoxItemProspectLM.Text,
                textBoxItemProspectGID.Text,
                textBoxItemProspectMIC.Text,
                textBoxItemProspectMAC.Text
            };

            if (textBoxItemProspectEntry.Text.Trim() != "")
            {
                var existingData = (DataTable)dataGridViewItemProspect.DataSource;
                existingData.Rows.Add(values);
                dataGridViewItemProspect.DataSource = existingData;
            }
        }
        private void buttonItemProspectRefresh_Click(object sender, EventArgs e)
        {
            dataGridViewItemProspect.DataSource = DatabaseItemNameColumn("prospecting_loot_template", "entry", (textBoxItemProspectEntry.Text.Trim() != "") ? textBoxItemProspectEntry.Text.Trim() : textBoxItemTempEntry.Text, 1, true);
        }
        private void buttonItemProspectDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewItemProspect.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewItemProspect.SelectedRows)
                {
                    dataGridViewItemProspect.Rows.RemoveAt(row.Index);
                }
            }
        }
        private void buttonItemProspectGenerate_Click(object sender, EventArgs e)
        {
            GenerateLootSQL("prospecting_loot_template", dataGridViewItemProspect, textBoxItemScriptOutput);
        }
        #endregion
        #endregion
        #region POPUPS
        // Search
        private void buttonItemSearchClass_Click(object sender, EventArgs e)
        {
            textBoxItemSearchClass.Text = CreatePopupSelection("Class Selection", DataItemClass(), textBoxItemSearchClass.Text);
        }
        private void buttonItemSearchSubclass_Click(object sender, EventArgs e)
        {
            textBoxItemSearchSubclass.Text = CreatePopupSelection("Subclass Selection", DataItemSubclass(textBoxItemSearchClass.Text.Trim()), textBoxItemSearchSubclass.Text);
        }
        // Template
        private void buttonItemTempTypeClass_Click(object sender, EventArgs e)
        {
            textBoxItemTempTypeClass.Text = CreatePopupSelection("Class Selection", DataItemClass(), textBoxItemTempTypeClass.Text);
        }
        private void buttonItemTempSubclass_Click(object sender, EventArgs e)
        {
            textBoxItemTempSubclass.Text = CreatePopupSelection("Subclass Selection", DataItemSubclass(textBoxItemTempTypeClass.Text.Trim()), textBoxItemTempSubclass.Text);
        }
        private void buttonItemTempDisplayID_Click(object sender, EventArgs e)
        {
            bool[] rButtons = { true, false, false };

            textBoxItemTempDisplayID.Text = CreatePopupEntity(textBoxItemTempDisplayID.Text, rButtons, false);
        }
        private void buttonItemTempQuality_Click(object sender, EventArgs e)
        {
            textBoxItemTempQuality.Text = CreatePopupSelection("Quality Selection", ReadExcelCSV("ItemQuality", 0, 1), textBoxItemTempQuality.Text);
        }
        private void buttonItemTempFlags_Click(object sender, EventArgs e)
        {
            textBoxItemTempFlags.Text = CreatePopupChecklist("Flags", ReadExcelCSV("ItemFlags", 0, 1), textBoxItemTempFlags.Text, true);
        }
        private void buttonItemTempEFlags_Click(object sender, EventArgs e)
        {
            textBoxItemTempEFlags.Text = CreatePopupChecklist("Extra Flags", ReadExcelCSV("ItemFlagsExtra", 0, 1), textBoxItemTempEFlags.Text, true);
        }
        private void buttonItemTempDmgType1_Click(object sender, EventArgs e)
        {
            textBoxItemTempDmgType1.Text = CreatePopupSelection("Damage Type I Selection", ReadExcelCSV("ItemDamageTypes", 0, 1), textBoxItemTempDmgType1.Text);
        }
        private void buttonItemTempDmgType2_Click(object sender, EventArgs e)
        {
            textBoxItemTempDmgType2.Text = CreatePopupSelection("Damage Type II Selection", ReadExcelCSV("ItemDamageTypes", 0, 1), textBoxItemTempDmgType2.Text);
        }
        private void buttonItemTempAmmoType_Click(object sender, EventArgs e)
        {
            textBoxItemTempAmmoType.Text = CreatePopupSelection("Ammo Types", ReadExcelCSV("ItemAmmoType", 0, 1), textBoxItemTempAmmoType.Text);
        }
        private void buttonItemTempItemSet_Click(object sender, EventArgs e)
        {
            textBoxItemTempItemSet.Text = CreatePopupSelection("ItemSet Selection", ReadExcelCSV("ItemSet", 0, 1), textBoxItemTempItemSet.Text);
        }
        private void buttonItemTempBonding_Click(object sender, EventArgs e)
        {
            textBoxItemTempBonding.Text = CreatePopupSelection("Bonding Selection", ReadExcelCSV("ItemBondings", 0, 1), textBoxItemTempBonding.Text);
        }
        private void buttonItemTempSheath_Click(object sender, EventArgs e)
        {
            textBoxItemTempSheath.Text = CreatePopupSelection("Sheath Selection", ReadExcelCSV("ItemSheaths", 0, 1), textBoxItemTempSheath.Text);
        }
        private void buttonItemTempColor1_Click(object sender, EventArgs e)
        {
            textBoxItemTempColor1.Text = CreatePopupSelection("Color Selection I", ReadExcelCSV("ItemSocketColors", 0, 1), textBoxItemTempColor1.Text);
        }
        private void buttonItemTempColor2_Click(object sender, EventArgs e)
        {
            textBoxItemTempColor2.Text = CreatePopupSelection("Color Selection II", ReadExcelCSV("ItemSocketColors", 0, 1), textBoxItemTempColor2.Text);
        }
        private void buttonItemTempColor3_Click(object sender, EventArgs e)
        {
            textBoxItemTempColor3.Text = CreatePopupSelection("Color Selection III", ReadExcelCSV("ItemSocketColors", 0, 1), textBoxItemTempColor3.Text);
        }
        private void buttonItemTempSocketBonus_Click(object sender, EventArgs e)
        {
            textBoxItemTempSocketBonus.Text = CreatePopupSelection("Socket Bonus Selection III", ReadExcelCSV("ItemSocketBonus", 0, 1), textBoxItemTempSocketBonus.Text);
        }
        private void buttonItemTempStatsType1_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType1.Text = CreatePopupSelection("Stat Selection I", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType1.Text);
        }
        private void buttonItemTempStatsType2_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType2.Text = CreatePopupSelection("Stat Selection II", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType2.Text);
        }
        private void buttonItemTempStatsType3_Click(object sender, EventArgs e)
        {
            CreatePopupSelection("Stat Selection III", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType3.Text);
        }
        private void buttonItemTempStatsType4_Click(object sender, EventArgs e)
        {
            CreatePopupSelection("Stat Selection IV", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType4.Text);
        }
        private void buttonItemTempStatsType5_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType5.Text = CreatePopupSelection("Stat Selection V", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType5.Text);
        }
        private void buttonItemTempStatsType6_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType6.Text = CreatePopupSelection("Stat Selection VI", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType6.Text);
        }
        private void buttonItemTempStatsType7_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType7.Text = CreatePopupSelection("Stat Selection VII", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType7.Text);
        }
        private void buttonItemTempStatsType8_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType8.Text = CreatePopupSelection("Stat Selection VIII", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType8.Text);
        }
        private void buttonItemTempStatsType9_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType9.Text = CreatePopupSelection("Stat Selection IX", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType9.Text);
        }
        private void buttonItemTempStatsType10_Click(object sender, EventArgs e)
        {
            textBoxItemTempStatsType10.Text = CreatePopupSelection("Stat Selection X", ReadExcelCSV("ItemStatTypes", 0, 1), textBoxItemTempStatsType10.Text);
        }
        private void buttonItemTempSpellID1_Click(object sender, EventArgs e)
        {
            textBoxItemTempSpellID1.Text = CreatePopupSelection("Required Spell", ReadExcelCSV("Spells", 0, 1), textBoxItemTempSpellID1.Text);
        }
        private void buttonItemTempTrigger1_Click(object sender, EventArgs e)
        {
            textBoxItemTempTrigger1.Text = CreatePopupSelection("Spell Trigger", ReadExcelCSV("ItemSpellTrigger", 0, 1), textBoxItemTempTrigger1.Text);
        }
        private void buttonItemTempReqRace_Click(object sender, EventArgs e)
        {
            textBoxItemTempReqRace.Text = CreatePopupChecklist("Race Requirement", ReadExcelCSV("ChrRaces", 0, 14), textBoxItemTempReqRace.Text, true);
        }
        private void buttonItemTempReqClass_Click(object sender, EventArgs e)
        {
            textBoxItemTempReqClass.Text = CreatePopupChecklist("Class Requirement", ReadExcelCSV("ChrClasses", 0, 4), textBoxItemTempReqClass.Text, true);
        }
        private void buttonItemTempReqSkill_Click(object sender, EventArgs e)
        {
            textBoxItemTempReqSkill.Text = CreatePopupSelection("Required Skill", ReadExcelCSV("SkillLine", 0, 3), textBoxItemTempReqSkill.Text);
        }
        private void buttonItemTempReqRepFaction_Click(object sender, EventArgs e)
        {
            textBoxItemTempReqRepFaction.Text = CreatePopupSelection("Required Reputation Faction", ReadExcelCSV("Faction", 0, 23), textBoxItemTempReqRepFaction.Text);
        }
        private void buttonItemTempReqRepRank_Click(object sender, EventArgs e)
        {
            textBoxItemTempReqRepRank.Text = CreatePopupSelection("Required Reputation Rank", ReadExcelCSV("ItemReqReputationRank", 0, 1), textBoxItemTempReqRepRank.Text);
        }
        private void buttonItemTempReqSpell_Click(object sender, EventArgs e)
        {
            textBoxItemTempReqSpell.Text = CreatePopupSelection("Required Spell", ReadExcelCSV("Spells", 0, 1), textBoxItemTempReqSpell.Text);
        }
        private void buttonItemTempMaterial_Click(object sender, EventArgs e)
        {
            textBoxItemTempMaterial.Text = CreatePopupSelection("Materials", ReadExcelCSV("ItemMaterial", 0, 1), textBoxItemTempMaterial.Text);
        }
        private void buttonItemTempFoodType_Click(object sender, EventArgs e)
        {
            textBoxItemTempFoodType.Text = CreatePopupSelection("Food Type", ReadExcelCSV("ItemFoodType", 0, 1), textBoxItemTempFoodType.Text);
        }
        private void buttonItemTempBagFamily_Click(object sender, EventArgs e)
        {
            textBoxItemTempBagFamily.Text = CreatePopupSelection("Bag Family", ReadExcelCSV("ItemBagFamily", 0, 1), textBoxItemTempBagFamily.Text);
        }
        private void buttonItemTempFlagsC_Click(object sender, EventArgs e)
        {
            textBoxItemTempFlagsC.Text = CreatePopupSelection("Custom Flags", ReadExcelCSV("ItemFlagsCustom", 0, 1), textBoxItemTempFlagsC.Text);
        }
        private void buttonItemTempTotemCategory_Click(object sender, EventArgs e)
        {
            textBoxItemTempTotemCategory.Text = CreatePopupSelection("Totem Category", ReadExcelCSV("ItemTotemCategory", 0, 1), textBoxItemTempTotemCategory.Text);
        }




        #endregion

        #endregion

        
    }
}