using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace lKHM
{
    public class ControlWriter : System.IO.TextWriter
    {
        private System.Windows.Forms.Control textbox;
        public ControlWriter(System.Windows.Forms.Control textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Text += value;
        }

        public override void Write(string value)
        {
            textbox.Text += value;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
