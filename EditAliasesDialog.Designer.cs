namespace HellDivers2OneKeyStratagem
{
    partial class EditAliasesDialog
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
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            label1 = new Label();
            systemAliasesTextBox = new TextBox();
            flowLayoutPanel3 = new FlowLayoutPanel();
            label2 = new Label();
            userAliasesTextBox = new TextBox();
            flowLayoutPanel4 = new FlowLayoutPanel();
            okButton = new Button();
            cancelButton = new Button();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            flowLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(flowLayoutPanel2);
            flowLayoutPanel1.Controls.Add(flowLayoutPanel3);
            flowLayoutPanel1.Controls.Add(flowLayoutPanel4);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(582, 123);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Anchor = AnchorStyles.Left;
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel2.Controls.Add(label1);
            flowLayoutPanel2.Controls.Add(systemAliasesTextBox);
            flowLayoutPanel2.Location = new Point(3, 4);
            flowLayoutPanel2.Margin = new Padding(3, 4, 3, 4);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(576, 32);
            flowLayoutPanel2.TabIndex = 1;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 6);
            label1.Name = "label1";
            label1.Size = new Size(156, 19);
            label1.TabIndex = 0;
            label1.Text = "预设的名称（用|分隔）：";
            // 
            // systemAliasesTextBox
            // 
            systemAliasesTextBox.Anchor = AnchorStyles.Left;
            systemAliasesTextBox.Location = new Point(165, 4);
            systemAliasesTextBox.Margin = new Padding(3, 4, 3, 4);
            systemAliasesTextBox.Name = "systemAliasesTextBox";
            systemAliasesTextBox.ReadOnly = true;
            systemAliasesTextBox.Size = new Size(408, 24);
            systemAliasesTextBox.TabIndex = 1;
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.Anchor = AnchorStyles.Left;
            flowLayoutPanel3.AutoSize = true;
            flowLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel3.Controls.Add(label2);
            flowLayoutPanel3.Controls.Add(userAliasesTextBox);
            flowLayoutPanel3.Location = new Point(3, 44);
            flowLayoutPanel3.Margin = new Padding(3, 4, 3, 4);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(576, 32);
            flowLayoutPanel3.TabIndex = 2;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 6);
            label2.Name = "label2";
            label2.Size = new Size(156, 19);
            label2.TabIndex = 2;
            label2.Text = "自定义名称（用|分隔）：";
            // 
            // userAliasesTextBox
            // 
            userAliasesTextBox.Anchor = AnchorStyles.Left;
            userAliasesTextBox.Location = new Point(165, 4);
            userAliasesTextBox.Margin = new Padding(3, 4, 3, 4);
            userAliasesTextBox.Name = "userAliasesTextBox";
            userAliasesTextBox.Size = new Size(408, 24);
            userAliasesTextBox.TabIndex = 3;
            // 
            // flowLayoutPanel4
            // 
            flowLayoutPanel4.Anchor = AnchorStyles.Right;
            flowLayoutPanel4.AutoSize = true;
            flowLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel4.Controls.Add(okButton);
            flowLayoutPanel4.Controls.Add(cancelButton);
            flowLayoutPanel4.Location = new Point(417, 84);
            flowLayoutPanel4.Margin = new Padding(3, 4, 3, 4);
            flowLayoutPanel4.Name = "flowLayoutPanel4";
            flowLayoutPanel4.Size = new Size(162, 35);
            flowLayoutPanel4.TabIndex = 4;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Left;
            okButton.AutoSize = true;
            okButton.Location = new Point(3, 3);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 29);
            okButton.TabIndex = 0;
            okButton.Text = "确定";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Left;
            cancelButton.AutoSize = true;
            cancelButton.Location = new Point(84, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 29);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "取消";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // EditAliasesForm
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CancelButton = cancelButton;
            ClientSize = new Size(914, 503);
            Controls.Add(flowLayoutPanel1);
            Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 134);
            Margin = new Padding(3, 4, 3, 4);
            Name = "EditAliasesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "自定义语音名称";
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            flowLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.PerformLayout();
            flowLayoutPanel4.ResumeLayout(false);
            flowLayoutPanel4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private FlowLayoutPanel flowLayoutPanel3;
        private Label label1;
        private TextBox systemAliasesTextBox;
        private Label label2;
        private TextBox userAliasesTextBox;
        private FlowLayoutPanel flowLayoutPanel4;
        private Button okButton;
        private Button cancelButton;
    }
}
