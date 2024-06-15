using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HellDivers2OneKeyStratagem
{
    public partial class CalibrateVoiceDialog : Form
    {
        public CalibrateVoiceDialog()
        {
            InitializeComponent();
        }

        private void ModalTip_Load(object sender, EventArgs e)
        {

        }
        public bool TryClosed = false;
        public CalibrateVoiceDialog SetTipString(string tipString)
        {
            label1.Text = tipString;
            return this;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ModalTip_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TryClosed)
            {
                DialogResult = DialogResult.OK;
                return;
            }
            if (MessageBox.Show(@"取消校准？", @"提示", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                DialogResult = DialogResult.None;
                e.Cancel = true;
            }
            DialogResult = DialogResult.Cancel;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (TryClosed)
            {
                Close();
            }
        }
    }
}
