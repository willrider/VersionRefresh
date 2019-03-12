using DgvFilterPopup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VersionRefresh
{
    public partial class Form1 : Form
    {
        DataTable MainTable = new DataTable();
        BindingSource MainBS = new BindingSource();

        DataTable SAP = new DataTable();
        DataTable EquiCards = new DataTable();
        DataTable PSCVersions = new DataTable();
        DataTable CustApp = new DataTable();
        DataTable tmpDT = new DataTable();
        DataTable Except = new DataTable();
        DataTable LostnFound = new DataTable();
        //DataTable myPSCVer = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(GetSAPConnectionString()))
            {
                con.Open();
                string sqlTrunc = "dbo.TruncateTable";
                SqlCommand cmd = new SqlCommand(sqlTrunc, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                con.Close();
            }



            RefreshData("RXCRMREF.tblPharmacy", GetRealSAPConnectionString(), SAP);
            RefreshData("RXCRMREF.tblCustApp", GetRealSAPConnectionString(), CustApp);
            RefreshData("RXCRMDB.PSC.VersionView", GetRealSAPConnectionString(), PSCVersions);
            RefreshData("RXCRMREF.tblServicesPharmacy", GetRealSAPConnectionString(), EquiCards);
            //RefreshData("dbo.PSCVersions", GetSAPConnectionString(), myPSCVer);
            RefreshData("dbo.Exceptions", GetSAPConnectionString(), Except);
            RefreshData("dbo.LostnFound", GetSAPConnectionString(), LostnFound);

            PopuplateEquip();
            //PopuplatePSCVersions();
            PopuplateSAPmain();
            PopulateExce();


            DgvFilterManager filterManager1 = new DgvFilterManager(dataGridView1);
        }

        public void PopulateExce()
        {
            tmpDT.Clear();
            tmpDT = new DataTable();

            tmpDT.Columns.Add("RXID", Type.GetType("System.Int32"));
            tmpDT.Columns.Add("VersionTypeId", Type.GetType("System.Int32"));
            tmpDT.Columns.Add("Orig_Value", Type.GetType("System.String"));
            tmpDT.Columns.Add("Value", Type.GetType("System.String"));
            tmpDT.Columns.Add("Override", Type.GetType("System.Boolean"));
            tmpDT.Columns.Add("Comment", Type.GetType("System.String"));

            IList<string> GuidsList = new List<string>();

            foreach (DataRow row in PSCVersions.Rows)
            {
                var tmpRow = new Object[6];

                //List<DataRow> foundRows = new List<DataRow>();
                //String expression;


                tmpRow[0] = row.ItemArray[0]; //RXID
                tmpRow[1] = row.ItemArray[5];
                tmpRow[2] = row.ItemArray[8];

                if (Convert.ToInt32(row.ItemArray[5]) == 5)
                {
                    continue;
                }

                List<DataRow> foundRowsInner = new List<DataRow>();
                String expressionInner;
                expressionInner = "RXID = '" + tmpRow[0].ToString() + "' AND VersionTypeId = '" +
                                  tmpRow[1].ToString() + "'";
                foundRowsInner = Except.Select(expressionInner).ToList();

                if (foundRowsInner.Count < 1)
                {
                    tmpRow[3] = row.ItemArray[8];
                    tmpRow[4] = false;
                    tmpRow[5] = "";
                    tmpDT.Rows.Add(tmpRow);

                }
                else
                {

                    if (Convert.ToBoolean(foundRowsInner[0].ItemArray[4]) == false)
                    {
                        tmpRow[3] = row.ItemArray[8];
                        tmpRow[4] = false;
                        tmpRow[5] = foundRowsInner[0].ItemArray[5];
                        tmpDT.Rows.Add(tmpRow);
                    }
                    else if (Convert.ToBoolean(foundRowsInner[0].ItemArray[4]) == true)
                    {
                        tmpRow[3] = foundRowsInner[0].ItemArray[3];
                        tmpRow[4] = true;
                        tmpRow[5] = foundRowsInner[0].ItemArray[5];
                        tmpDT.Rows.Add(tmpRow);
                    }
                }
            }


//foreach (string guid in GuidsList)
//            {
//                LostnFound.Rows.Add(guid);
//            }

            using (SqlConnection con = new SqlConnection(GetSAPConnectionString()))
            {
                con.Open();
                string sqlTrunc = "dbo.TruncateExceptions";
                SqlCommand cmd = new SqlCommand(sqlTrunc, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                con.Close();
            }

            PushData(GetSAPConnectionString(), tmpDT, "Exceptions");
            PushData(GetSAPConnectionString(), LostnFound, "LostnFound");
        }
    

    public void PopuplateSAPmain()
        {
            SAP.Columns.RemoveAt(4);
            SAP.Columns.RemoveAt(8);
            SAP.Columns.RemoveAt(9);
            SAP.Columns.RemoveAt(9);
            SAP.Columns.RemoveAt(9);
            SAP.Columns.RemoveAt(9);
            SAP.Columns.RemoveAt(10);
            SAP.Columns.RemoveAt(13);
            SAP.Columns.RemoveAt(14);
            SAP.Columns.RemoveAt(14);
            SAP.Columns.RemoveAt(15);
            SAP.Columns.RemoveAt(15);
            SAP.Columns.RemoveAt(14);

            tmpDT.Clear();
            tmpDT.Columns.Add("RXID", Type.GetType("System.Int32"));
            tmpDT.Columns.Add("PharmacyName", Type.GetType("System.String"));
            tmpDT.Columns.Add("GroupID", Type.GetType("System.String"));
            tmpDT.Columns.Add("NACS", Type.GetType("System.String"));
            tmpDT.Columns.Add("GUID", Type.GetType("System.Guid"));
            tmpDT.Columns.Add("IMSID", Type.GetType("System.String"));
            tmpDT.Columns.Add("AAHID", Type.GetType("System.String"));
            tmpDT.Columns.Add("SupplyData", Type.GetType("System.Boolean"));
            tmpDT.Columns.Add("isNuamrk", Type.GetType("System.Boolean"));
            tmpDT.Columns.Add("isActive", Type.GetType("System.Boolean"));
            tmpDT.Columns.Add("CompanyID", Type.GetType("System.Int32"));
            tmpDT.Columns.Add("Country", Type.GetType("System.String"));
            tmpDT.Columns.Add("isReceiveUpdate", Type.GetType("System.Boolean"));
            tmpDT.Columns.Add("isHeadOffice", Type.GetType("System.Boolean"));
            tmpDT.Columns.Add("NoItems", Type.GetType("System.Int32"));
            tmpDT.Columns.Add("AppTypeID", Type.GetType("System.String"));

            foreach (DataRow row in SAP.Rows)
            {
                var tmpRow = new Object[16];

                tmpRow[0] = row.ItemArray[0];

                tmpRow[1] = row.ItemArray[3];
                tmpRow[2] = row.ItemArray[7];
                tmpRow[3] = row.ItemArray[12];
                tmpRow[4] = row.ItemArray[10];
                tmpRow[5] = row.ItemArray[2];
                tmpRow[6] = row.ItemArray[1];
                tmpRow[7] = row.ItemArray[4];
                tmpRow[8] = row.ItemArray[5];
                tmpRow[9] = row.ItemArray[6];
                tmpRow[10] = row.ItemArray[8];
                tmpRow[11] = row.ItemArray[9];
                tmpRow[12] = row.ItemArray[11];
                tmpRow[13] = row.ItemArray[13];

                List<DataRow> foundRows = new List<DataRow>();
                String expression;

                expression = "RXPharmID = '" + tmpRow[0].ToString() + "'";
                foundRows = CustApp.Select(expression).ToList();

                if (foundRows.Count > 0)
                {
                    tmpRow[14] = foundRows[0].ItemArray[3];
                    tmpRow[15] = foundRows[0].ItemArray[2];
                }





                tmpDT.Rows.Add(tmpRow);
            }
            PushData(GetSAPConnectionString(),tmpDT,"SAPmain");

        }

        public void PopuplateEquip()
        {
            EquiCards.Columns.RemoveAt(1);
            EquiCards.Columns.RemoveAt(1);
            EquiCards.Columns.RemoveAt(1);
            EquiCards.Columns.RemoveAt(3);
            EquiCards.Columns.RemoveAt(3);
            EquiCards.Columns.RemoveAt(4);
            EquiCards.Columns.RemoveAt(4);
            EquiCards.Columns.RemoveAt(4);
            EquiCards.Columns.RemoveAt(4);
            EquiCards.Columns.RemoveAt(4);
            EquiCards.Columns.RemoveAt(4);
            PushData(GetSAPConnectionString(), EquiCards, "Equipment");
        }

        //public void PopuplatePSCVersions()
        //{
        //    PushData(GetSAPConnectionString(), PSCVersions, "PSCVersions");
        //}

        public string GetSAPConnectionString()
        {
            return
                @"Data Source=RXDEP01SERVER\RXDEPMSDPL;Initial Catalog=VersionControl; User Id=sa;Password=1852963;";
        }

        public string GetRealSAPConnectionString()
        {
            return
                @"Data Source=192.168.9.84\rxdistribution;Initial Catalog=RXCRMDB; User Id=managedservices;Password=@bonn1e;";
        }

        public void PushData(string ConnectionString, DataTable dataTable, string SQLTable)
        {

            using (var bulkCopy = new SqlBulkCopy(ConnectionString, SqlBulkCopyOptions.KeepIdentity))
            {
                // my DataTable column names match my SQL Column names, so I simply made this loop. However if your column names don't match, just pass in which datatable name matches the SQL column name in Column Mappings
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    bulkCopy.ColumnMappings.Add(i, i);
                }

                bulkCopy.BulkCopyTimeout = 600;
                bulkCopy.DestinationTableName = SQLTable;
                bulkCopy.WriteToServer(dataTable);
            }
        }
        

        public void PullData(string tablename, string connString, DataTable dataTable, BindingSource bs, DataGridView DGV)
        {
            dataTable.Clear();
            while (dataTable.Columns.Count > 0)
            {
                dataTable.Columns.RemoveAt(0);
            }
            
            
            string query = "select * from " + tablename;
            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dataTable);
            conn.Close();
            da.Dispose();
            BindToDGV(dataTable, bs, DGV);
        }

        public void RefreshData(string tablename, string connString, DataTable dataTable)
        {
            // dataTable = new DataTable();
            
            string query = "select * from " + tablename;
            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dataTable);
            conn.Close();
            da.Dispose();
            
        }

        public void BindToDGV(DataTable dt, BindingSource bs, DataGridView DGV)
        {
            bs.DataSource = dt;
            DGV.DataSource = bs;
            DGV.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    PullData("SAPmain", GetSAPConnectionString(), MainTable, MainBS, dataGridView1);
                    break;
                case 1:
                    PullData("PSCVersions", GetSAPConnectionString(), MainTable, MainBS, dataGridView1);
                    break;
                case 2:
                    PullData("Equipment", GetSAPConnectionString(), MainTable, MainBS, dataGridView1);
                    break;
                case 3:
                    PullData("Exceptions", GetSAPConnectionString(), MainTable, MainBS, dataGridView1);
                    break;
                default:
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    PushData(GetSAPConnectionString(), MainTable, "SAPmain");
                    break;
                case 1:
                    PushData(GetSAPConnectionString(), MainTable, "PSCVersions");
                    break;
                case 2:
                    PushData(GetSAPConnectionString(), MainTable, "Equipment");
                    break;
                case 3:
                    using (SqlConnection con = new SqlConnection(GetSAPConnectionString()))
                    {
                        con.Open();
                        string sqlTrunc = "dbo.TruncateExceptions";
                        SqlCommand cmd = new SqlCommand(sqlTrunc, con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    PushData(GetSAPConnectionString(), MainTable, "Exceptions");
                    break;
                default:
                    break;
            }

            MessageBox.Show("Changes Saved!");
        }
    }
}
