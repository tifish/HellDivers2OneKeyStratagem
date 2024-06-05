namespace HellDivers2OneKeyStratagem
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rootFlowLayoutPanel = new FlowLayoutPanel();
            buttonsFlowLayoutPanel = new FlowLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            label5 = new Label();
            ctrlRadioButton = new RadioButton();
            altRadioButton = new RadioButton();
            flowLayoutPanel3 = new FlowLayoutPanel();
            label6 = new Label();
            wasdRadioButton = new RadioButton();
            arrowRadioButton = new RadioButton();
            startButton = new Button();
            stopButton = new Button();
            runningLabel = new Label();
            label4 = new Label();
            fKeysFlowLayoutPanel = new FlowLayoutPanel();
            fKeyFlowLayoutPanel = new FlowLayoutPanel();
            label2 = new Label();
            label1 = new Label();
            stratagemGroupsFlowLayoutPanel = new FlowLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label3 = new Label();
            checkBox1 = new CheckBox();
            checkBox2 = new CheckBox();
            rootFlowLayoutPanel.SuspendLayout();
            buttonsFlowLayoutPanel.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            fKeysFlowLayoutPanel.SuspendLayout();
            fKeyFlowLayoutPanel.SuspendLayout();
            stratagemGroupsFlowLayoutPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            //
            // rootFlowLayoutPanel
            //
            rootFlowLayoutPanel.AutoSize = true;
            rootFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rootFlowLayoutPanel.Controls.Add(buttonsFlowLayoutPanel);
            rootFlowLayoutPanel.Controls.Add(label4);
            rootFlowLayoutPanel.Controls.Add(fKeysFlowLayoutPanel);
            rootFlowLayoutPanel.Controls.Add(stratagemGroupsFlowLayoutPanel);
            rootFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            rootFlowLayoutPanel.Location = new Point(0, 0);
            rootFlowLayoutPanel.Margin = new Padding(10);
            rootFlowLayoutPanel.Name = "rootFlowLayoutPanel";
            rootFlowLayoutPanel.Size = new Size(611, 213);
            rootFlowLayoutPanel.TabIndex = 0;
            //
            // buttonsFlowLayoutPanel
            //
            buttonsFlowLayoutPanel.AutoSize = true;
            buttonsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonsFlowLayoutPanel.Controls.Add(flowLayoutPanel2);
            buttonsFlowLayoutPanel.Controls.Add(flowLayoutPanel3);
            buttonsFlowLayoutPanel.Controls.Add(startButton);
            buttonsFlowLayoutPanel.Controls.Add(stopButton);
            buttonsFlowLayoutPanel.Controls.Add(runningLabel);
            buttonsFlowLayoutPanel.Location = new Point(3, 3);
            buttonsFlowLayoutPanel.Margin = new Padding(3, 3, 3, 10);
            buttonsFlowLayoutPanel.Name = "buttonsFlowLayoutPanel";
            buttonsFlowLayoutPanel.Size = new Size(605, 35);
            buttonsFlowLayoutPanel.TabIndex = 3;
            //
            // flowLayoutPanel2
            //
            flowLayoutPanel2.Anchor = AnchorStyles.Left;
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel2.Controls.Add(label5);
            flowLayoutPanel2.Controls.Add(ctrlRadioButton);
            flowLayoutPanel2.Controls.Add(altRadioButton);
            flowLayoutPanel2.Location = new Point(3, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(185, 29);
            flowLayoutPanel2.TabIndex = 5;
            //
            // label5
            //
            label5.Anchor = AnchorStyles.Left;
            label5.AutoSize = true;
            label5.Location = new Point(3, 5);
            label5.Name = "label5";
            label5.Size = new Size(74, 19);
            label5.TabIndex = 5;
            label5.Text = "触发按键：";
            //
            // ctrlRadioButton
            //
            ctrlRadioButton.Anchor = AnchorStyles.Left;
            ctrlRadioButton.AutoSize = true;
            ctrlRadioButton.Checked = true;
            ctrlRadioButton.Location = new Point(83, 3);
            ctrlRadioButton.Name = "ctrlRadioButton";
            ctrlRadioButton.Size = new Size(49, 23);
            ctrlRadioButton.TabIndex = 3;
            ctrlRadioButton.TabStop = true;
            ctrlRadioButton.Text = "Ctrl";
            ctrlRadioButton.UseVisualStyleBackColor = true;
            ctrlRadioButton.Click += ctrlRadioButton_Click;
            //
            // altRadioButton
            //
            altRadioButton.Anchor = AnchorStyles.Left;
            altRadioButton.AutoSize = true;
            altRadioButton.Location = new Point(138, 3);
            altRadioButton.Name = "altRadioButton";
            altRadioButton.Size = new Size(44, 23);
            altRadioButton.TabIndex = 4;
            altRadioButton.Text = "Alt";
            altRadioButton.UseVisualStyleBackColor = true;
            altRadioButton.Click += ctrlRadioButton_Click;
            //
            // flowLayoutPanel3
            //
            flowLayoutPanel3.Anchor = AnchorStyles.Left;
            flowLayoutPanel3.AutoSize = true;
            flowLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel3.Controls.Add(label6);
            flowLayoutPanel3.Controls.Add(wasdRadioButton);
            flowLayoutPanel3.Controls.Add(arrowRadioButton);
            flowLayoutPanel3.Location = new Point(194, 3);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(183, 29);
            flowLayoutPanel3.TabIndex = 6;
            //
            // label6
            //
            label6.Anchor = AnchorStyles.Left;
            label6.AutoSize = true;
            label6.Location = new Point(3, 5);
            label6.Name = "label6";
            label6.Size = new Size(19, 19);
            label6.TabIndex = 5;
            label6.Text = "+";
            //
            // wasdRadioButton
            //
            wasdRadioButton.Anchor = AnchorStyles.Left;
            wasdRadioButton.AutoSize = true;
            wasdRadioButton.Checked = true;
            wasdRadioButton.Location = new Point(28, 3);
            wasdRadioButton.Name = "wasdRadioButton";
            wasdRadioButton.Size = new Size(67, 23);
            wasdRadioButton.TabIndex = 3;
            wasdRadioButton.TabStop = true;
            wasdRadioButton.Text = "WASD";
            wasdRadioButton.UseVisualStyleBackColor = true;
            wasdRadioButton.Click += wasdRadioButton_Click;
            //
            // arrowRadioButton
            //
            arrowRadioButton.Anchor = AnchorStyles.Left;
            arrowRadioButton.AutoSize = true;
            arrowRadioButton.Location = new Point(101, 3);
            arrowRadioButton.Name = "arrowRadioButton";
            arrowRadioButton.Size = new Size(79, 23);
            arrowRadioButton.TabIndex = 4;
            arrowRadioButton.Text = "上下左右";
            arrowRadioButton.UseVisualStyleBackColor = true;
            arrowRadioButton.Click += wasdRadioButton_Click;
            //
            // startButton
            //
            startButton.AutoSize = true;
            startButton.Location = new Point(383, 3);
            startButton.Name = "startButton";
            startButton.Size = new Size(75, 29);
            startButton.TabIndex = 7;
            startButton.Text = "启动(&S)";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            //
            // stopButton
            //
            stopButton.AutoSize = true;
            stopButton.Enabled = false;
            stopButton.Location = new Point(464, 3);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(75, 29);
            stopButton.TabIndex = 8;
            stopButton.Text = "停止(&D)";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click += stopButton_Click;
            //
            // runningLabel
            //
            runningLabel.Anchor = AnchorStyles.Left;
            runningLabel.AutoSize = true;
            runningLabel.Location = new Point(545, 8);
            runningLabel.Name = "runningLabel";
            runningLabel.Size = new Size(57, 19);
            runningLabel.TabIndex = 9;
            runningLabel.Text = "修养中...";
            //
            // label4
            //
            label4.AutoSize = true;
            label4.Location = new Point(3, 48);
            label4.Name = "label4";
            label4.Size = new Size(399, 19);
            label4.TabIndex = 4;
            label4.Text = "强烈建议把呼叫战略的按键改为上下左右，可以在跑动中呼叫战略。";
            //
            // fKeysFlowLayoutPanel
            //
            fKeysFlowLayoutPanel.AutoSize = true;
            fKeysFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fKeysFlowLayoutPanel.Controls.Add(fKeyFlowLayoutPanel);
            fKeysFlowLayoutPanel.Location = new Point(3, 70);
            fKeysFlowLayoutPanel.Margin = new Padding(3, 3, 3, 10);
            fKeysFlowLayoutPanel.Name = "fKeysFlowLayoutPanel";
            fKeysFlowLayoutPanel.Size = new Size(73, 44);
            fKeysFlowLayoutPanel.TabIndex = 1;
            //
            // fKeyFlowLayoutPanel
            //
            fKeyFlowLayoutPanel.AutoSize = true;
            fKeyFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fKeyFlowLayoutPanel.Controls.Add(label2);
            fKeyFlowLayoutPanel.Controls.Add(label1);
            fKeyFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            fKeyFlowLayoutPanel.Location = new Point(3, 3);
            fKeyFlowLayoutPanel.Name = "fKeyFlowLayoutPanel";
            fKeyFlowLayoutPanel.Size = new Size(67, 38);
            fKeyFlowLayoutPanel.TabIndex = 2;
            //
            // label2
            //
            label2.Anchor = AnchorStyles.Top;
            label2.AutoSize = true;
            label2.Location = new Point(21, 0);
            label2.Name = "label2";
            label2.Size = new Size(24, 19);
            label2.TabIndex = 0;
            label2.Text = "F1";
            //
            // label1
            //
            label1.Anchor = AnchorStyles.Top;
            label1.AutoSize = true;
            label1.Location = new Point(3, 19);
            label1.Name = "label1";
            label1.Size = new Size(61, 19);
            label1.TabIndex = 1;
            label1.Text = "飞鹰空袭";
            //
            // stratagemGroupsFlowLayoutPanel
            //
            stratagemGroupsFlowLayoutPanel.AutoSize = true;
            stratagemGroupsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            stratagemGroupsFlowLayoutPanel.Controls.Add(flowLayoutPanel1);
            stratagemGroupsFlowLayoutPanel.Location = new Point(3, 127);
            stratagemGroupsFlowLayoutPanel.Name = "stratagemGroupsFlowLayoutPanel";
            stratagemGroupsFlowLayoutPanel.Size = new Size(118, 83);
            stratagemGroupsFlowLayoutPanel.TabIndex = 2;
            //
            // flowLayoutPanel1
            //
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(label3);
            flowLayoutPanel1.Controls.Add(checkBox1);
            flowLayoutPanel1.Controls.Add(checkBox2);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(112, 77);
            flowLayoutPanel1.TabIndex = 3;
            //
            // label3
            //
            label3.Anchor = AnchorStyles.Top;
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft YaHei", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label3.Location = new Point(16, 0);
            label3.Name = "label3";
            label3.Size = new Size(80, 19);
            label3.TabIndex = 0;
            label3.Text = "进攻型·飞鹰";
            //
            // checkBox1
            //
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(3, 22);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(80, 23);
            checkBox1.TabIndex = 1;
            checkBox1.Text = "飞鹰空袭";
            checkBox1.UseVisualStyleBackColor = true;
            //
            // checkBox2
            //
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(3, 51);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(106, 23);
            checkBox2.TabIndex = 2;
            checkBox2.Text = "飞鹰集束炸弹";
            checkBox2.UseVisualStyleBackColor = true;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(940, 463);
            Controls.Add(rootFlowLayoutPanel);
            Font = new Font("Microsoft YaHei", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 134);
            KeyPreview = true;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HellDivers 2 一键战略";
            FormClosed += MainForm_FormClosed;
            Load += MainForm_Load;
            KeyDown += MainForm_KeyDown;
            rootFlowLayoutPanel.ResumeLayout(false);
            rootFlowLayoutPanel.PerformLayout();
            buttonsFlowLayoutPanel.ResumeLayout(false);
            buttonsFlowLayoutPanel.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            flowLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.PerformLayout();
            fKeysFlowLayoutPanel.ResumeLayout(false);
            fKeysFlowLayoutPanel.PerformLayout();
            fKeyFlowLayoutPanel.ResumeLayout(false);
            fKeyFlowLayoutPanel.PerformLayout();
            stratagemGroupsFlowLayoutPanel.ResumeLayout(false);
            stratagemGroupsFlowLayoutPanel.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel rootFlowLayoutPanel;
        private FlowLayoutPanel fKeysFlowLayoutPanel;
        private FlowLayoutPanel stratagemGroupsFlowLayoutPanel;
        private FlowLayoutPanel fKeyFlowLayoutPanel;
        private Label label2;
        private Label label1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label3;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private FlowLayoutPanel buttonsFlowLayoutPanel;
        private Label label4;
        private FlowLayoutPanel flowLayoutPanel2;
        private Label label5;
        private RadioButton ctrlRadioButton;
        private RadioButton altRadioButton;
        private FlowLayoutPanel flowLayoutPanel3;
        private Label label6;
        private RadioButton wasdRadioButton;
        private RadioButton arrowRadioButton;
        private Button startButton;
        private Button stopButton;
        private Label runningLabel;
    }
}
