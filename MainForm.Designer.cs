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
            enableVoiceCheckBox = new CheckBox();
            voiceNamesComboBox = new ComboBox();
            refreshVoiceNamesButton = new Button();
            suggestionLabel = new Label();
            generateVoiceFlowLayoutPanel = new FlowLayoutPanel();
            generateVoiceStyleComboBox = new ComboBox();
            tryVoiceButton = new Button();
            label7 = new Label();
            voiceRateTextBox = new TextBox();
            label8 = new Label();
            voiceVolumeTextBox = new TextBox();
            label9 = new Label();
            voicePitchTextBox = new TextBox();
            generateVoiceButton = new Button();
            generateTxtButton = new Button();
            generateVoiceMessageLabel = new Label();
            flowLayoutPanel4 = new FlowLayoutPanel();
            stratagemSetsComboBox = new ComboBox();
            saveStratagemSetButton = new Button();
            deleteStratagemSetButton = new Button();
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
            generateVoiceFlowLayoutPanel.SuspendLayout();
            flowLayoutPanel4.SuspendLayout();
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
            rootFlowLayoutPanel.Controls.Add(suggestionLabel);
            rootFlowLayoutPanel.Controls.Add(generateVoiceFlowLayoutPanel);
            rootFlowLayoutPanel.Controls.Add(flowLayoutPanel4);
            rootFlowLayoutPanel.Controls.Add(fKeysFlowLayoutPanel);
            rootFlowLayoutPanel.Controls.Add(stratagemGroupsFlowLayoutPanel);
            rootFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            rootFlowLayoutPanel.Location = new Point(0, 0);
            rootFlowLayoutPanel.Margin = new Padding(10);
            rootFlowLayoutPanel.Name = "rootFlowLayoutPanel";
            rootFlowLayoutPanel.Size = new Size(1011, 295);
            rootFlowLayoutPanel.TabIndex = 0;
            // 
            // buttonsFlowLayoutPanel
            // 
            buttonsFlowLayoutPanel.AutoSize = true;
            buttonsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonsFlowLayoutPanel.Controls.Add(flowLayoutPanel2);
            buttonsFlowLayoutPanel.Controls.Add(flowLayoutPanel3);
            buttonsFlowLayoutPanel.Controls.Add(enableVoiceCheckBox);
            buttonsFlowLayoutPanel.Controls.Add(voiceNamesComboBox);
            buttonsFlowLayoutPanel.Controls.Add(refreshVoiceNamesButton);
            buttonsFlowLayoutPanel.Location = new Point(3, 3);
            buttonsFlowLayoutPanel.Margin = new Padding(3, 3, 3, 10);
            buttonsFlowLayoutPanel.Name = "buttonsFlowLayoutPanel";
            buttonsFlowLayoutPanel.Size = new Size(758, 35);
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
            // enableVoiceCheckBox
            // 
            enableVoiceCheckBox.Anchor = AnchorStyles.Left;
            enableVoiceCheckBox.AutoSize = true;
            enableVoiceCheckBox.Checked = true;
            enableVoiceCheckBox.CheckState = CheckState.Checked;
            enableVoiceCheckBox.Location = new Point(383, 6);
            enableVoiceCheckBox.Name = "enableVoiceCheckBox";
            enableVoiceCheckBox.Size = new Size(145, 23);
            enableVoiceCheckBox.TabIndex = 6;
            enableVoiceCheckBox.Text = "呼叫时播放战略名字";
            enableVoiceCheckBox.UseVisualStyleBackColor = true;
            enableVoiceCheckBox.Click += enableVoiceCheckBox_Click;
            // 
            // voiceNamesComboBox
            // 
            voiceNamesComboBox.Anchor = AnchorStyles.Left;
            voiceNamesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            voiceNamesComboBox.FormattingEnabled = true;
            voiceNamesComboBox.Location = new Point(534, 6);
            voiceNamesComboBox.Name = "voiceNamesComboBox";
            voiceNamesComboBox.Size = new Size(140, 27);
            voiceNamesComboBox.TabIndex = 7;
            voiceNamesComboBox.SelectionChangeCommitted += voiceNamesComboBox_SelectionChangeCommitted;
            // 
            // refreshVoiceNamesButton
            // 
            refreshVoiceNamesButton.Anchor = AnchorStyles.Left;
            refreshVoiceNamesButton.AutoSize = true;
            refreshVoiceNamesButton.Location = new Point(680, 3);
            refreshVoiceNamesButton.Name = "refreshVoiceNamesButton";
            refreshVoiceNamesButton.Size = new Size(75, 29);
            refreshVoiceNamesButton.TabIndex = 9;
            refreshVoiceNamesButton.Text = "刷新";
            refreshVoiceNamesButton.UseVisualStyleBackColor = true;
            refreshVoiceNamesButton.Click += refreshVoiceNamesButton_Click;
            // 
            // suggestionLabel
            // 
            suggestionLabel.Anchor = AnchorStyles.Left;
            suggestionLabel.AutoSize = true;
            suggestionLabel.Location = new Point(3, 48);
            suggestionLabel.Name = "suggestionLabel";
            suggestionLabel.Size = new Size(399, 19);
            suggestionLabel.TabIndex = 4;
            suggestionLabel.Text = "强烈建议把呼叫战略的按键改为上下左右，可以在跑动中呼叫战略。";
            suggestionLabel.Click += suggestionLabel_Click;
            // 
            // generateVoiceFlowLayoutPanel
            // 
            generateVoiceFlowLayoutPanel.Anchor = AnchorStyles.Left;
            generateVoiceFlowLayoutPanel.AutoSize = true;
            generateVoiceFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            generateVoiceFlowLayoutPanel.Controls.Add(generateVoiceStyleComboBox);
            generateVoiceFlowLayoutPanel.Controls.Add(tryVoiceButton);
            generateVoiceFlowLayoutPanel.Controls.Add(label7);
            generateVoiceFlowLayoutPanel.Controls.Add(voiceRateTextBox);
            generateVoiceFlowLayoutPanel.Controls.Add(label8);
            generateVoiceFlowLayoutPanel.Controls.Add(voiceVolumeTextBox);
            generateVoiceFlowLayoutPanel.Controls.Add(label9);
            generateVoiceFlowLayoutPanel.Controls.Add(voicePitchTextBox);
            generateVoiceFlowLayoutPanel.Controls.Add(generateVoiceButton);
            generateVoiceFlowLayoutPanel.Controls.Add(generateTxtButton);
            generateVoiceFlowLayoutPanel.Controls.Add(generateVoiceMessageLabel);
            generateVoiceFlowLayoutPanel.Location = new Point(3, 70);
            generateVoiceFlowLayoutPanel.Name = "generateVoiceFlowLayoutPanel";
            generateVoiceFlowLayoutPanel.Size = new Size(856, 35);
            generateVoiceFlowLayoutPanel.TabIndex = 6;
            generateVoiceFlowLayoutPanel.Visible = false;
            // 
            // generateVoiceStyleComboBox
            // 
            generateVoiceStyleComboBox.Anchor = AnchorStyles.Left;
            generateVoiceStyleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            generateVoiceStyleComboBox.FormattingEnabled = true;
            generateVoiceStyleComboBox.Location = new Point(3, 4);
            generateVoiceStyleComboBox.Name = "generateVoiceStyleComboBox";
            generateVoiceStyleComboBox.Size = new Size(254, 27);
            generateVoiceStyleComboBox.TabIndex = 5;
            generateVoiceStyleComboBox.SelectionChangeCommitted += generateVoiceTypeComboBox_SelectionChangeCommitted;
            // 
            // tryVoiceButton
            // 
            tryVoiceButton.Anchor = AnchorStyles.Left;
            tryVoiceButton.AutoSize = true;
            tryVoiceButton.Location = new Point(263, 3);
            tryVoiceButton.Name = "tryVoiceButton";
            tryVoiceButton.Size = new Size(75, 29);
            tryVoiceButton.TabIndex = 8;
            tryVoiceButton.Text = "试听";
            tryVoiceButton.UseVisualStyleBackColor = true;
            tryVoiceButton.Click += tryVoiceButton_Click;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Left;
            label7.AutoSize = true;
            label7.Location = new Point(344, 8);
            label7.Name = "label7";
            label7.Size = new Size(38, 19);
            label7.TabIndex = 16;
            label7.Text = "速率:";
            // 
            // voiceRateTextBox
            // 
            voiceRateTextBox.Anchor = AnchorStyles.Left;
            voiceRateTextBox.Location = new Point(388, 5);
            voiceRateTextBox.Name = "voiceRateTextBox";
            voiceRateTextBox.Size = new Size(59, 25);
            voiceRateTextBox.TabIndex = 19;
            voiceRateTextBox.Text = "+0%";
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Left;
            label8.AutoSize = true;
            label8.Location = new Point(453, 8);
            label8.Name = "label8";
            label8.Size = new Size(48, 19);
            label8.TabIndex = 17;
            label8.Text = "音量：";
            // 
            // voiceVolumeTextBox
            // 
            voiceVolumeTextBox.Anchor = AnchorStyles.Left;
            voiceVolumeTextBox.Location = new Point(507, 5);
            voiceVolumeTextBox.Name = "voiceVolumeTextBox";
            voiceVolumeTextBox.Size = new Size(59, 25);
            voiceVolumeTextBox.TabIndex = 20;
            voiceVolumeTextBox.Text = "+0%";
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Left;
            label9.AutoSize = true;
            label9.Location = new Point(572, 8);
            label9.Name = "label9";
            label9.Size = new Size(48, 19);
            label9.TabIndex = 18;
            label9.Text = "音调：";
            // 
            // voicePitchTextBox
            // 
            voicePitchTextBox.Anchor = AnchorStyles.Left;
            voicePitchTextBox.Location = new Point(626, 5);
            voicePitchTextBox.Name = "voicePitchTextBox";
            voicePitchTextBox.Size = new Size(59, 25);
            voicePitchTextBox.TabIndex = 21;
            voicePitchTextBox.Text = "+0Hz";
            // 
            // generateVoiceButton
            // 
            generateVoiceButton.Anchor = AnchorStyles.Left;
            generateVoiceButton.AutoSize = true;
            generateVoiceButton.Location = new Point(691, 3);
            generateVoiceButton.Name = "generateVoiceButton";
            generateVoiceButton.Size = new Size(75, 29);
            generateVoiceButton.TabIndex = 15;
            generateVoiceButton.Text = "生成语音";
            generateVoiceButton.UseVisualStyleBackColor = true;
            generateVoiceButton.Click += generateVoiceButton_Click;
            // 
            // generateTxtButton
            // 
            generateTxtButton.Anchor = AnchorStyles.Left;
            generateTxtButton.AutoSize = true;
            generateTxtButton.Location = new Point(772, 3);
            generateTxtButton.Name = "generateTxtButton";
            generateTxtButton.Size = new Size(75, 29);
            generateTxtButton.TabIndex = 22;
            generateTxtButton.Text = "生成 txt";
            generateTxtButton.UseVisualStyleBackColor = true;
            generateTxtButton.Click += generateTxtButton_Click;
            // 
            // generateVoiceMessageLabel
            // 
            generateVoiceMessageLabel.Anchor = AnchorStyles.Left;
            generateVoiceMessageLabel.AutoSize = true;
            generateVoiceMessageLabel.Location = new Point(853, 8);
            generateVoiceMessageLabel.Name = "generateVoiceMessageLabel";
            generateVoiceMessageLabel.Size = new Size(0, 19);
            generateVoiceMessageLabel.TabIndex = 8;
            // 
            // flowLayoutPanel4
            // 
            flowLayoutPanel4.Anchor = AnchorStyles.Left;
            flowLayoutPanel4.AutoSize = true;
            flowLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel4.Controls.Add(stratagemSetsComboBox);
            flowLayoutPanel4.Controls.Add(saveStratagemSetButton);
            flowLayoutPanel4.Controls.Add(deleteStratagemSetButton);
            flowLayoutPanel4.Location = new Point(3, 111);
            flowLayoutPanel4.Name = "flowLayoutPanel4";
            flowLayoutPanel4.Size = new Size(1005, 35);
            flowLayoutPanel4.TabIndex = 5;
            // 
            // stratagemSetsComboBox
            // 
            stratagemSetsComboBox.Anchor = AnchorStyles.Left;
            stratagemSetsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            stratagemSetsComboBox.FormattingEnabled = true;
            stratagemSetsComboBox.Location = new Point(3, 4);
            stratagemSetsComboBox.Name = "stratagemSetsComboBox";
            stratagemSetsComboBox.Size = new Size(810, 27);
            stratagemSetsComboBox.TabIndex = 5;
            stratagemSetsComboBox.SelectedIndexChanged += stratagemSetsComboBox_SelectedIndexChanged;
            // 
            // saveStratagemSetButton
            // 
            saveStratagemSetButton.AutoSize = true;
            saveStratagemSetButton.Location = new Point(819, 3);
            saveStratagemSetButton.Name = "saveStratagemSetButton";
            saveStratagemSetButton.Size = new Size(88, 29);
            saveStratagemSetButton.TabIndex = 8;
            saveStratagemSetButton.Text = "保存配置(&S)";
            saveStratagemSetButton.UseVisualStyleBackColor = true;
            saveStratagemSetButton.Click += saveStratagemSetButton_Click;
            // 
            // deleteStratagemSetButton
            // 
            deleteStratagemSetButton.AutoSize = true;
            deleteStratagemSetButton.Location = new Point(913, 3);
            deleteStratagemSetButton.Name = "deleteStratagemSetButton";
            deleteStratagemSetButton.Size = new Size(89, 29);
            deleteStratagemSetButton.TabIndex = 9;
            deleteStratagemSetButton.Text = "删除配置(&D)";
            deleteStratagemSetButton.UseVisualStyleBackColor = true;
            deleteStratagemSetButton.Click += deleteStratagemSetButton_Click;
            // 
            // fKeysFlowLayoutPanel
            // 
            fKeysFlowLayoutPanel.AutoSize = true;
            fKeysFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fKeysFlowLayoutPanel.Controls.Add(fKeyFlowLayoutPanel);
            fKeysFlowLayoutPanel.Location = new Point(3, 152);
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
            stratagemGroupsFlowLayoutPanel.Location = new Point(3, 209);
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
            ClientSize = new Size(1127, 463);
            Controls.Add(rootFlowLayoutPanel);
            Font = new Font("Microsoft YaHei", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 134);
            KeyPreview = true;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HellDivers 2 一键战略";
            Deactivate += MainForm_Deactivate;
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
            generateVoiceFlowLayoutPanel.ResumeLayout(false);
            generateVoiceFlowLayoutPanel.PerformLayout();
            flowLayoutPanel4.ResumeLayout(false);
            flowLayoutPanel4.PerformLayout();
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
        private Label suggestionLabel;
        private FlowLayoutPanel flowLayoutPanel2;
        private Label label5;
        private RadioButton ctrlRadioButton;
        private RadioButton altRadioButton;
        private FlowLayoutPanel flowLayoutPanel3;
        private Label label6;
        private RadioButton wasdRadioButton;
        private RadioButton arrowRadioButton;
        private FlowLayoutPanel flowLayoutPanel4;
        private ComboBox stratagemSetsComboBox;
        private Button saveStratagemSetButton;
        private Button deleteStratagemSetButton;
        private FlowLayoutPanel generateVoiceFlowLayoutPanel;
        private ComboBox generateVoiceStyleComboBox;
        private CheckBox enableVoiceCheckBox;
        private Label generateVoiceMessageLabel;
        private Button tryVoiceButton;
        private Label label7;
        private TextBox voiceRateTextBox;
        private Label label8;
        private TextBox voiceVolumeTextBox;
        private Label label9;
        private TextBox voicePitchTextBox;
        private Button generateVoiceButton;
        private ComboBox voiceNamesComboBox;
        private Button refreshVoiceNamesButton;
        private Button generateTxtButton;
    }
}
