using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lKHM
{
	public class HatNode : TreeNode
	{
		private uint _fighterID = uint.MaxValue;

		public HatNode(uint fighterIDIn)
		{
			_fighterID = fighterIDIn;
			updateTextField();
		}
		public void updateTextField()
		{
			Text = "[" + _fighterID.ToString("X2") + "] " + HatName;
		}

		public uint FighterID
		{
			get
			{
				return _fighterID;
			}
			set
			{
				_fighterID = value;
				updateTextField();
			}
		}
		public string HatName
		{
			get
			{
				return HatNames.getNameFromFID(_fighterID);
			}
			set
			{
				HatNames.setFIDName(_fighterID, value, true);
				updateTextField();
			}
		}
	}
}
