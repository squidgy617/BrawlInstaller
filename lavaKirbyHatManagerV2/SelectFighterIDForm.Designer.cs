
namespace lKHM
{
	partial class SelectFighterIDForm
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
			this.labelContext = new System.Windows.Forms.Label();
			this.numericUpDownFID = new System.Windows.Forms.NumericUpDown();
			this.buttonOkay = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.textBoxSlotName = new System.Windows.Forms.TextBox();
			this.checkBoxSetName = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownFID)).BeginInit();
			this.SuspendLayout();
			// 
			// labelContext
			// 
			this.labelContext.AutoSize = true;
			this.labelContext.Location = new System.Drawing.Point(13, 13);
			this.labelContext.Name = "labelContext";
			this.labelContext.Size = new System.Drawing.Size(46, 17);
			this.labelContext.TabIndex = 0;
			this.labelContext.Text = "label1";
			// 
			// numericUpDownFID
			// 
			this.numericUpDownFID.Hexadecimal = true;
			this.numericUpDownFID.Location = new System.Drawing.Point(16, 39);
			this.numericUpDownFID.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericUpDownFID.Name = "numericUpDownFID";
			this.numericUpDownFID.Size = new System.Drawing.Size(58, 22);
			this.numericUpDownFID.TabIndex = 0;
			this.numericUpDownFID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownFID.ValueChanged += new System.EventHandler(this.numericUpDownFID_ValueChanged);
			this.numericUpDownFID.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numericUpDownFID_KeyDown);
			// 
			// buttonOkay
			// 
			this.buttonOkay.Location = new System.Drawing.Point(119, 67);
			this.buttonOkay.Name = "buttonOkay";
			this.buttonOkay.Size = new System.Drawing.Size(80, 30);
			this.buttonOkay.TabIndex = 2;
			this.buttonOkay.Text = "Okay";
			this.buttonOkay.UseVisualStyleBackColor = true;
			this.buttonOkay.Click += new System.EventHandler(this.buttonOkay_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(205, 67);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 30);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// textBoxSlotName
			// 
			this.textBoxSlotName.Location = new System.Drawing.Point(80, 39);
			this.textBoxSlotName.Name = "textBoxSlotName";
			this.textBoxSlotName.ReadOnly = true;
			this.textBoxSlotName.Size = new System.Drawing.Size(205, 22);
			this.textBoxSlotName.TabIndex = 5;
			this.textBoxSlotName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// checkBoxSetName
			// 
			this.checkBoxSetName.AutoSize = true;
			this.checkBoxSetName.Location = new System.Drawing.Point(16, 73);
			this.checkBoxSetName.Name = "checkBoxSetName";
			this.checkBoxSetName.Size = new System.Drawing.Size(103, 21);
			this.checkBoxSetName.TabIndex = 1;
			this.checkBoxSetName.Text = "Copy Name";
			this.checkBoxSetName.UseVisualStyleBackColor = true;
			// 
			// SelectFighterIDForm
			// 
			this.AcceptButton = this.buttonOkay;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(297, 105);
			this.Controls.Add(this.checkBoxSetName);
			this.Controls.Add(this.textBoxSlotName);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOkay);
			this.Controls.Add(this.numericUpDownFID);
			this.Controls.Add(this.labelContext);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SelectFighterIDForm";
			this.Text = "Select Fighter ID";
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownFID)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelContext;
		private System.Windows.Forms.Button buttonOkay;
		private System.Windows.Forms.Button buttonCancel;
		public System.Windows.Forms.NumericUpDown numericUpDownFID;
		private System.Windows.Forms.TextBox textBoxSlotName;
		public System.Windows.Forms.CheckBox checkBoxSetName;
	}
}