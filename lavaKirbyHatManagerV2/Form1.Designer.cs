
namespace lKHM
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.labelKirbyHats = new System.Windows.Forms.Label();
			this.treeViewKirbyHats = new System.Windows.Forms.TreeView();
			this.buttonCreateHat = new System.Windows.Forms.Button();
			this.buttonCloneHat = new System.Windows.Forms.Button();
			this.buttonDeleteHat = new System.Windows.Forms.Button();
			this.buttonMoveHat = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveModuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportHatsToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.importHatsFromXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.importHatsFromTXTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonExpandContract = new System.Windows.Forms.Button();
			this.labelHatSpecs = new System.Windows.Forms.Label();
			this.propertyGridHatDetails = new System.Windows.Forms.PropertyGrid();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelKirbyHats
			// 
			this.labelKirbyHats.AutoSize = true;
			this.labelKirbyHats.Location = new System.Drawing.Point(13, 38);
			this.labelKirbyHats.Name = "labelKirbyHats";
			this.labelKirbyHats.Size = new System.Drawing.Size(77, 17);
			this.labelKirbyHats.TabIndex = 39;
			this.labelKirbyHats.Text = "Kirby Hats:";
			// 
			// treeViewKirbyHats
			// 
			this.treeViewKirbyHats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.treeViewKirbyHats.HideSelection = false;
			this.treeViewKirbyHats.Location = new System.Drawing.Point(13, 60);
			this.treeViewKirbyHats.Name = "treeViewKirbyHats";
			this.treeViewKirbyHats.ShowPlusMinus = false;
			this.treeViewKirbyHats.ShowRootLines = false;
			this.treeViewKirbyHats.Size = new System.Drawing.Size(270, 565);
			this.treeViewKirbyHats.TabIndex = 0;
			this.treeViewKirbyHats.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewKirbyHats_AfterSelect);
			// 
			// buttonCreateHat
			// 
			this.buttonCreateHat.Location = new System.Drawing.Point(290, 60);
			this.buttonCreateHat.Margin = new System.Windows.Forms.Padding(4);
			this.buttonCreateHat.Name = "buttonCreateHat";
			this.buttonCreateHat.Size = new System.Drawing.Size(134, 28);
			this.buttonCreateHat.TabIndex = 1;
			this.buttonCreateHat.Text = "New Hat";
			this.buttonCreateHat.UseVisualStyleBackColor = true;
			this.buttonCreateHat.Click += new System.EventHandler(this.buttonCreateHat_Click);
			// 
			// buttonCloneHat
			// 
			this.buttonCloneHat.Location = new System.Drawing.Point(290, 130);
			this.buttonCloneHat.Margin = new System.Windows.Forms.Padding(4);
			this.buttonCloneHat.Name = "buttonCloneHat";
			this.buttonCloneHat.Size = new System.Drawing.Size(134, 28);
			this.buttonCloneHat.TabIndex = 3;
			this.buttonCloneHat.Text = "Copy Selected";
			this.buttonCloneHat.UseVisualStyleBackColor = true;
			this.buttonCloneHat.Click += new System.EventHandler(this.buttonCloneHat_Click);
			// 
			// buttonDeleteHat
			// 
			this.buttonDeleteHat.Location = new System.Drawing.Point(290, 95);
			this.buttonDeleteHat.Margin = new System.Windows.Forms.Padding(4);
			this.buttonDeleteHat.Name = "buttonDeleteHat";
			this.buttonDeleteHat.Size = new System.Drawing.Size(134, 28);
			this.buttonDeleteHat.TabIndex = 2;
			this.buttonDeleteHat.Text = "Delete Selected";
			this.buttonDeleteHat.UseVisualStyleBackColor = true;
			this.buttonDeleteHat.Click += new System.EventHandler(this.buttonDeleteHat_Click);
			// 
			// buttonMoveHat
			// 
			this.buttonMoveHat.Location = new System.Drawing.Point(290, 165);
			this.buttonMoveHat.Margin = new System.Windows.Forms.Padding(4);
			this.buttonMoveHat.Name = "buttonMoveHat";
			this.buttonMoveHat.Size = new System.Drawing.Size(134, 28);
			this.buttonMoveHat.TabIndex = 4;
			this.buttonMoveHat.Text = "Move Selected";
			this.buttonMoveHat.UseVisualStyleBackColor = true;
			this.buttonMoveHat.Click += new System.EventHandler(this.buttonMoveHat_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(822, 28);
			this.menuStrip1.TabIndex = 47;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openModuleToolStripMenuItem,
            this.saveModuleToolStripMenuItem,
            this.exportHatsToXMLToolStripMenuItem,
            this.importHatsFromXMLToolStripMenuItem,
            this.importHatsFromTXTToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// openModuleToolStripMenuItem
			// 
			this.openModuleToolStripMenuItem.Name = "openModuleToolStripMenuItem";
			this.openModuleToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
			this.openModuleToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openModuleToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
			this.openModuleToolStripMenuItem.Text = "Open Module";
			this.openModuleToolStripMenuItem.Click += new System.EventHandler(this.openModuleToolStripMenuItem_Click);
			// 
			// saveModuleToolStripMenuItem
			// 
			this.saveModuleToolStripMenuItem.Name = "saveModuleToolStripMenuItem";
			this.saveModuleToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
			this.saveModuleToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveModuleToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
			this.saveModuleToolStripMenuItem.Text = "Save Module";
			this.saveModuleToolStripMenuItem.Click += new System.EventHandler(this.saveModuleToolStripMenuItem_Click);
			// 
			// exportHatsToXMLToolStripMenuItem
			// 
			this.exportHatsToXMLToolStripMenuItem.Name = "exportHatsToXMLToolStripMenuItem";
			this.exportHatsToXMLToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
			this.exportHatsToXMLToolStripMenuItem.Text = "Export Hats to .XML";
			this.exportHatsToXMLToolStripMenuItem.Click += new System.EventHandler(this.exportHatsToXMLToolStripMenuItem_Click);
			// 
			// importHatsFromXMLToolStripMenuItem
			// 
			this.importHatsFromXMLToolStripMenuItem.Name = "importHatsFromXMLToolStripMenuItem";
			this.importHatsFromXMLToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
			this.importHatsFromXMLToolStripMenuItem.Text = "Import Hats from .XML";
			this.importHatsFromXMLToolStripMenuItem.Click += new System.EventHandler(this.importHatsFromXMLToolStripMenuItem_Click);
			// 
			// importHatsFromTXTToolStripMenuItem
			// 
			this.importHatsFromTXTToolStripMenuItem.Name = "importHatsFromTXTToolStripMenuItem";
			this.importHatsFromTXTToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
			this.importHatsFromTXTToolStripMenuItem.Text = "Import Hats from .TXT";
			this.importHatsFromTXTToolStripMenuItem.Click += new System.EventHandler(this.importHatsFromTXTToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(133, 26);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// buttonExpandContract
			// 
			this.buttonExpandContract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExpandContract.Location = new System.Drawing.Point(780, 27);
			this.buttonExpandContract.Margin = new System.Windows.Forms.Padding(4);
			this.buttonExpandContract.Name = "buttonExpandContract";
			this.buttonExpandContract.Size = new System.Drawing.Size(32, 28);
			this.buttonExpandContract.TabIndex = 5;
			this.buttonExpandContract.Text = "<<";
			this.buttonExpandContract.UseVisualStyleBackColor = true;
			this.buttonExpandContract.Click += new System.EventHandler(this.buttonExpandContract_Click);
			// 
			// labelHatSpecs
			// 
			this.labelHatSpecs.AutoSize = true;
			this.labelHatSpecs.Location = new System.Drawing.Point(429, 38);
			this.labelHatSpecs.Name = "labelHatSpecs";
			this.labelHatSpecs.Size = new System.Drawing.Size(81, 17);
			this.labelHatSpecs.TabIndex = 49;
			this.labelHatSpecs.Text = "Hat Details:";
			// 
			// propertyGridHatDetails
			// 
			this.propertyGridHatDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.propertyGridHatDetails.HelpVisible = false;
			this.propertyGridHatDetails.Location = new System.Drawing.Point(432, 60);
			this.propertyGridHatDetails.Name = "propertyGridHatDetails";
			this.propertyGridHatDetails.PropertySort = System.Windows.Forms.PropertySort.Categorized;
			this.propertyGridHatDetails.Size = new System.Drawing.Size(380, 565);
			this.propertyGridHatDetails.TabIndex = 6;
			this.propertyGridHatDetails.ToolbarVisible = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(822, 638);
			this.Controls.Add(this.propertyGridHatDetails);
			this.Controls.Add(this.labelHatSpecs);
			this.Controls.Add(this.buttonExpandContract);
			this.Controls.Add(this.buttonMoveHat);
			this.Controls.Add(this.buttonDeleteHat);
			this.Controls.Add(this.buttonCloneHat);
			this.Controls.Add(this.buttonCreateHat);
			this.Controls.Add(this.treeViewKirbyHats);
			this.Controls.Add(this.labelKirbyHats);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "lavaKirbyHatManager";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label labelKirbyHats;
		private System.Windows.Forms.TreeView treeViewKirbyHats;
		private System.Windows.Forms.Button buttonCreateHat;
		private System.Windows.Forms.Button buttonCloneHat;
		private System.Windows.Forms.Button buttonDeleteHat;
		private System.Windows.Forms.Button buttonMoveHat;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openModuleToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveModuleToolStripMenuItem;
		private System.Windows.Forms.Button buttonExpandContract;
		private System.Windows.Forms.Label labelHatSpecs;
		private System.Windows.Forms.PropertyGrid propertyGridHatDetails;
		private System.Windows.Forms.ToolStripMenuItem importHatsFromTXTToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportHatsToXMLToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem importHatsFromXMLToolStripMenuItem;
	}
}

