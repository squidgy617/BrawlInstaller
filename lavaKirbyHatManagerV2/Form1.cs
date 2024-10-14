using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lKHM
{
	public partial class Form1 : Form
	{
		public class PropertyGetter
		{
			public Form1 parent = null;

			const string categoryName = "Contents";
			[Category(categoryName)]
			public string Name
			{
				get
				{
					string result = "";

					if (parent != null && parent.getSelectedNode() != null)
					{
						result = HatNames.getNameFromFID(parent.getSelectedNode().FighterID);
					}

					return result;
				}
				set
				{
					if (parent != null && parent.getSelectedNode() != null)
					{
						HatNames.setFIDName(parent.getSelectedNode().FighterID, value, true);
						parent.getSelectedNode().updateTextField();
					}
				}
			}
			[Category(categoryName)]
			public HatInfoPack Hat
			{ 
				get 
				{
					HatInfoPack result = null; 

					if (parent != null && parent.getSelectedNode() != null)
					{
						result = parent.hatManager.fighterIDToInfoPacks[parent.getSelectedNode().FighterID];
					}

					return result;
				}
			}
		}
		PropertyGetter propGetterObj = new PropertyGetter();

		bool formExpanded = false;
		string[] defaultKirbyRelPaths = { "./ft_kirby.rel", "../pf/module/ft_kirby.rel" };
		internal KirbyHatManager hatManager = new KirbyHatManager();
		BrawlLib.SSBB.ResourceNodes.RELNode kirbyModule = null;

		HatNode getSelectedNode()
		{
			return treeViewKirbyHats.SelectedNode as HatNode;
		}

		bool tryLoadHatNamesFromBuildConfigFolder(string relPath)
		{
			bool result = false;

			string absolutePath = System.IO.Path.GetFullPath(relPath);

			int moduleDirPos = absolutePath.LastIndexOf("module" + System.IO.Path.DirectorySeparatorChar);
			if (moduleDirPos != -1)
			{
				string configFolderPath = absolutePath.Substring(0, moduleDirPos) + "BrawlEx/FighterConfig/";
				result = HatNames.tryPopulateFromFighterConfigFolder(configFolderPath, true);
			}

			return result;
		}

		bool loadModule(string filepath)
		{
			bool result = false;

			treeViewKirbyHats.Nodes.Clear();
			propertyGridHatDetails.SelectedObject = null;

			if (System.IO.File.Exists(filepath))
			{
				kirbyModule = new BrawlLib.SSBB.ResourceNodes.RELNode();
				kirbyModule.Replace(filepath);
				kirbyModule._origPath = filepath;

				if (hatManager.loadHatEntriesFromREL(kirbyModule))
				{
					HatNames.populateFromInternalList();
					tryLoadHatNamesFromBuildConfigFolder(filepath);
					HatNames.pruneNamesWithNoAssociatedHat(hatManager);
					populateTreeView();
					result = true;
				}
				else
				{
					kirbyModule = null;
				}
			}

			if (result)
			{
				enableControls();
			}
			else
			{
				disableControls();
			}

			return result;
		}
		bool selectKirbyHatFromFID(uint targetFID)
		{
			bool result = false;

			foreach (HatNode x in treeViewKirbyHats.Nodes)
			{
				if (x.FighterID < targetFID) continue;
				if (x.FighterID > targetFID) break;
				treeViewKirbyHats.SelectedNode = x;
			}

			return result;
		}
		void populateTreeView()
		{
			uint selectedNodeFID = uint.MaxValue;
			if (treeViewKirbyHats.SelectedNode != null)
			{
				selectedNodeFID = getSelectedNode().FighterID;
			}

			treeViewKirbyHats.BeginUpdate();
			treeViewKirbyHats.Nodes.Clear();
			foreach (var x in hatManager.fighterIDToInfoPacks)
			{
				HatNode newNode = new HatNode(x.Key);
				newNode.FighterID = x.Key;
				treeViewKirbyHats.Nodes.Add(newNode);
			}
			propertyGridHatDetails.SelectedObject = propGetterObj;
			treeViewKirbyHats.EndUpdate();

			if (selectedNodeFID != uint.MaxValue)
			{
				selectKirbyHatFromFID(selectedNodeFID);
				propertyGridHatDetails.Refresh();
			}
			else if (treeViewKirbyHats.Nodes.Count > 0x0)
			{
				treeViewKirbyHats.SelectedNode = treeViewKirbyHats.Nodes[0x0];
			}
		}
		void setControlsEnabled(bool enabledIn)
		{
			buttonCreateHat.Enabled = enabledIn;
			buttonCloneHat.Enabled = enabledIn;
			buttonDeleteHat.Enabled = enabledIn;
			buttonMoveHat.Enabled = enabledIn;
			buttonExpandContract.Enabled = enabledIn;
			propertyGridHatDetails.Enabled = enabledIn;
			saveModuleToolStripMenuItem.Enabled = enabledIn;
			exportHatsToXMLToolStripMenuItem.Enabled = enabledIn;
			importHatsFromXMLToolStripMenuItem.Enabled = enabledIn;
			importHatsFromTXTToolStripMenuItem.Enabled = enabledIn;
		}
		void enableControls()
		{
			setControlsEnabled(true);
		}
		void disableControls()
		{
			setControlsEnabled(false);
		}
		void expandForm()
		{
			var tempSize = this.Size;
			tempSize.Width = 840;
			buttonExpandContract.Text = "<<";
			propertyGridHatDetails.TabStop = true;
			formExpanded = true;
			this.Size = tempSize;
		}
		void contractForm()
		{
			var tempSize = this.Size;
			tempSize.Width = 450;
			buttonExpandContract.Text = ">>";
			propertyGridHatDetails.TabStop = false;
			formExpanded = false;
			this.Size = tempSize;
		}
		void setFormExpanded(bool expandedIn)
		{
			if (expandedIn)
			{
				expandForm();
			}
			else
			{
				contractForm();
			}
		}
		void invertFormExpanded()
		{
			setFormExpanded(!formExpanded);
		}

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			propGetterObj.parent = this;
			foreach (string defaultPath in defaultKirbyRelPaths)
			{
				if (loadModule(defaultPath)) break;
			}
			setFormExpanded(formExpanded);
			propertyGridHatDetails.SelectedObject = propGetterObj;
		}

		private void openModuleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Module File(*.rel)|*.rel";
			ofd.RestoreDirectory = true;
			ofd.InitialDirectory = Properties.Settings.Default.lastImportPath;
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				loadModule(ofd.FileName);
				Properties.Settings.Default.lastImportPath = System.IO.Path.GetDirectoryName(ofd.FileName);
			}
		}
		private void saveModuleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.DefaultExt = ".rel";
			sfd.Filter = "*Module File(*.rel)|*.rel";
			sfd.RestoreDirectory = true;
			sfd.InitialDirectory = Properties.Settings.Default.lastExportPath;
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				if (hatManager.writeTablesToREL(kirbyModule))
				{
					kirbyModule.Export(sfd.FileName);
				}
				Properties.Settings.Default.lastExportPath = System.IO.Path.GetDirectoryName(sfd.FileName);
			}
		}

		private void buttonDeleteHat_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Delete the selected hat?", "Confirm Delete", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
			if (treeViewKirbyHats.SelectedNode != null)
			{
				int selectedIndex = treeViewKirbyHats.SelectedNode.Index;

				uint targetFID = getSelectedNode().FighterID;
				if (hatManager.eraseHat(targetFID))
				{
					HatNames.eraseFIDName(targetFID);
				}

				populateTreeView();
				if (selectedIndex < treeViewKirbyHats.Nodes.Count)
				{
					treeViewKirbyHats.SelectedNode = treeViewKirbyHats.Nodes[selectedIndex];
				}
				else if (treeViewKirbyHats.Nodes.Count > 0)
				{
					treeViewKirbyHats.SelectedNode = treeViewKirbyHats.Nodes[treeViewKirbyHats.Nodes.Count - 1];
				}
				treeViewKirbyHats.Focus();
			}
		}
		private void buttonCloneHat_Click(object sender, EventArgs e)
		{
			var IDForm = new SelectFighterIDForm("Fighter ID to Clone to:", hatManager);
			IDForm.checkBoxSetName.Visible = true;
			if (IDForm.ShowDialog() != DialogResult.OK) return;

			uint sourceID = getSelectedNode().FighterID;
			uint destinationID = (uint)IDForm.numericUpDownFID.Value;

			if (hatManager.copyHatToSlot(sourceID, destinationID, true) && IDForm.checkBoxSetName.Checked)
			{
				HatNames.copyFIDName(sourceID, destinationID, true);
			}

			populateTreeView();
			selectKirbyHatFromFID(destinationID);
			treeViewKirbyHats.Focus();
		}
		private void buttonMoveHat_Click(object sender, EventArgs e)
		{
			var IDForm = new SelectFighterIDForm("Fighter ID to Move to:", hatManager);
			IDForm.checkBoxSetName.Visible = false;
			if (IDForm.ShowDialog() != DialogResult.OK) return;

			uint sourceID = getSelectedNode().FighterID;
			uint destinationID = (uint)IDForm.numericUpDownFID.Value;
			if (hatManager.moveHatToSlot(sourceID, destinationID, true))
			{
				HatNames.moveFIDName(sourceID, destinationID, true);
			}

			populateTreeView();
			selectKirbyHatFromFID(destinationID);
			treeViewKirbyHats.Focus();
		}
		private void buttonCreateHat_Click(object sender, EventArgs e)
		{
			var IDForm = new SelectFighterIDForm("Fighter ID for New Hat:", hatManager);
			if (IDForm.ShowDialog() != DialogResult.OK) return;

			uint destinationID = (uint)IDForm.numericUpDownFID.Value;
			hatManager.createNewHat(destinationID);

			populateTreeView();
			selectKirbyHatFromFID(destinationID);
			treeViewKirbyHats.Focus();
		}

		private void treeViewKirbyHats_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (treeViewKirbyHats.SelectedNode != null)
			{
				propertyGridHatDetails.ExpandAllGridItems();
				propertyGridHatDetails.Refresh();
				treeViewKirbyHats.Focus();
			}
		}

		private void buttonExpandContract_Click(object sender, EventArgs e)
		{
			invertFormExpanded();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var aboutForm = new AboutForm();
			aboutForm.ShowDialog();
		}

		private void exportHatsToXMLToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var exportForm = new HatExportForm(treeViewKirbyHats);
			if (exportForm.ShowDialog() == DialogResult.OK)
			{
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.DefaultExt = ".xml";
				sfd.Filter = "Hat List File(*.xml)|*.xml";
				sfd.RestoreDirectory = true;
				sfd.InitialDirectory = Properties.Settings.Default.lastExportPath;
				if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					List<uint> IDsToExport = new List<uint>();

					foreach (HatNode currNode in exportForm.treeViewHats.Nodes)
					{
						if (!currNode.Checked) continue;
						IDsToExport.Add(currNode.FighterID);
					}

					HatXMLParser.exportHatsToXML(sfd.FileName, IDsToExport.ToArray(), 
						hatManager.fighterIDToInfoPacks, HatNames.fighterIDsToNames);
					populateTreeView();
					Properties.Settings.Default.lastExportPath = System.IO.Path.GetDirectoryName(sfd.FileName);
				}
			}
		}
		private void importHatsFromXMLToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Multiselect = true;
			ofd.Filter = "Hat List File(*.xml)|*.xml";
			ofd.RestoreDirectory = true;
			ofd.InitialDirectory = Properties.Settings.Default.lastImportPath;
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				HatXMLParser.importHatsFromXMLs(hatManager, ofd.FileNames);
				populateTreeView();
				Properties.Settings.Default.lastImportPath = System.IO.Path.GetDirectoryName(ofd.FileName);
			}
		}
		private void importHatsFromTXTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Multiselect = true;
			ofd.Filter = "Hat List File(*.txt)|*.txt";
			ofd.RestoreDirectory = true;
			ofd.InitialDirectory = Properties.Settings.Default.lastImportPath;
			if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				HatTXTParser.importHatsFromTXTs(hatManager, ofd.FileNames);
				populateTreeView();
				Properties.Settings.Default.lastImportPath = System.IO.Path.GetDirectoryName(ofd.FileName);
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			Properties.Settings.Default.Save();
		}
	}
}
