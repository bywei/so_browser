using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace 浏览器
{
    public partial class Form_Search : Form
    {
        public Form_Search()
        {
            InitializeComponent();
        }

        private void Form_Search_Load(object sender, EventArgs e)
        {
            // TODO: 这行代码将数据加载到表“db1DataSet.searches”中。您可以根据需要移动或移除它。
            this.searchesTableAdapter.Fill(this.db1DataSet.searches);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void 保存SToolStripButton_Click(object sender, EventArgs e)
        {
            searchesTableAdapter.Update(db1DataSet.searches);
            DialogResult = DialogResult.OK;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4 && e.RowIndex != dataGridView1.Rows.Count - 1)
                dataGridView1.Rows.RemoveAt(e.RowIndex);
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                OpenFileDialog of = new OpenFileDialog();
                of.Filter = "图片文件|*.jpg;*.png;*.gif|所有文件|*.*";
                of.RestoreDirectory = true;
                if (of.ShowDialog() == DialogResult.OK)
                    dataGridView1.Rows[e.RowIndex].Cells[2].Value = of.FileName;
                e.Cancel = true;
            }
        }

        private void 帮助LToolStripButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("双击即可修改内容", "提示");
        }
    }
}