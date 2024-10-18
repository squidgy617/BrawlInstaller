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
	public partial class SelectFighterIDForm : Form
	{
		KirbyHatManager sourceManager = null;

		bool hatSlotIsPopulated(uint FID)
		{
			return sourceManager.fighterIDToInfoPacks.ContainsKey(FID);
		}
		void updateSlotNameText()
		{
			uint selectedHatID = (uint)numericUpDownFID.Value;
			if (hatSlotIsPopulated(selectedHatID))
			{
				textBoxSlotName.Text = HatNames.getNameFromFID(selectedHatID);
			}
			else
			{
				textBoxSlotName.Text = "EMPTY_SLOT";
			}
		}

		public SelectFighterIDForm(string contextStringIn, KirbyHatManager managerIn)
		{
			InitializeComponent();

			labelContext.Text = contextStringIn;
			TextBox numTextBox = numericUpDownFID.Controls[1] as TextBox;
			numTextBox.CharacterCasing = CharacterCasing.Upper;
			numTextBox.MaxLength = 2;
			sourceManager = managerIn;

			updateSlotNameText();
		}

		private void buttonOkay_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			if (hatSlotIsPopulated((uint)numericUpDownFID.Value))
			{
				DialogResult = MessageBox.Show("Destination Hat Slot is already configured! Overwrite the associated configuration?",
					"Confirm Overwrite", MessageBoxButtons.OKCancel);
			}

			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void numericUpDownFID_KeyDown(object sender, KeyEventArgs e)
		{
			string allowedChars = "0123456789ABCDEF";
			Keys[] allowedKeys = { Keys.Back, Keys.Delete, Keys.Shift, Keys.Left, Keys.Right, Keys.Up, Keys.Down };
			if (!allowedChars.Contains((char)e.KeyCode) && !allowedKeys.Contains(e.KeyCode))
			{
				e.SuppressKeyPress = true;
			}
		}

		private void numericUpDownFID_ValueChanged(object sender, EventArgs e)
		{
			updateSlotNameText();
		}
	}
}
