using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using BrawlLib.Internal.Windows.Forms.Ookii.Dialogs;

namespace lKHM
{
	public partial class HatExportForm : Form
	{
		public HatExportForm(TreeView sourceTree)
		{
			InitializeComponent();

			treeViewHats.BeginUpdate();
			foreach (HatNode sourceNode in sourceTree.Nodes)
			{
				HatNode newNode = new HatNode(sourceNode.FighterID);
				treeViewHats.Nodes.Add(newNode);
			}
			treeViewHats.EndUpdate();

			setNumCheckedText();
		}

		private uint numTreeNodesChecked()
		{
			uint numChecked = 0;
			foreach (TreeNode currNode in treeViewHats.Nodes)
			{
				if (currNode.Checked)
				{
					numChecked++;
				}
			}
			return numChecked;
		}
		private void setNumCheckedText()
		{
			groupBox1.Text = "Hats (" + numTreeNodesChecked().ToString() + " out of " + treeViewHats.Nodes.Count.ToString() + " checked):";
		}

		private void buttonSelectAll_Click(object sender, EventArgs e)
		{
			treeViewHats.SuspendLayout();
			treeViewHats.AfterCheck -= treeViewHats_AfterCheck;
			foreach (TreeNode group in treeViewHats.Nodes)
			{
				group.Checked = true;
			}
			treeViewHats.AfterCheck += treeViewHats_AfterCheck;
			treeViewHats.ResumeLayout();
			handleCheckUIUpdates();
		}
		private void buttonDeselectAll_Click(object sender, EventArgs e)
		{
			treeViewHats.SuspendLayout();
			treeViewHats.AfterCheck -= treeViewHats_AfterCheck;
			foreach (TreeNode group in treeViewHats.Nodes)
			{
				group.Checked = false;
			}
			treeViewHats.AfterCheck += treeViewHats_AfterCheck;
			treeViewHats.ResumeLayout();
			handleCheckUIUpdates();
		}
		private void buttonInvertSelection_Click(object sender, EventArgs e)
		{
			treeViewHats.SuspendLayout();
			treeViewHats.AfterCheck -= treeViewHats_AfterCheck;
			foreach (TreeNode group in treeViewHats.Nodes)
			{
				group.Checked = !group.Checked;
			}
			treeViewHats.AfterCheck += treeViewHats_AfterCheck;
			treeViewHats.ResumeLayout();
			handleCheckUIUpdates();
		}

		private void handleCheckUIUpdates()
		{
			if (numTreeNodesChecked() > 0)
			{
				buttonExport.Enabled = true;
			}
			else
			{
				buttonExport.Enabled = false;
			}
			setNumCheckedText();
		}
		private void treeViewHats_AfterCheck(object sender, TreeViewEventArgs e)
		{
			handleCheckUIUpdates();
		}
		private void treeViewHats_KeyDown(object sender, KeyEventArgs e)
		{
			TreeNode currNode = treeViewHats.SelectedNode;
			if (currNode != null && e.KeyCode == Keys.Enter)
			{
				currNode.Checked = !currNode.Checked;
				e.SuppressKeyPress = true;
			}
		}
		private void treeViewHats_DoubleClick(object sender, EventArgs e)
		{
			TreeNode currNode = treeViewHats.SelectedNode;
			if (currNode != null)
			{
				currNode.Checked = !currNode.Checked;
			}
		}

		private void textBoxExportDirectory_TextChanged(object sender, EventArgs e)
		{
			if (numTreeNodesChecked() > 0)
			{
				buttonExport.Enabled = true;
			}
		}

		private void buttonExport_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
