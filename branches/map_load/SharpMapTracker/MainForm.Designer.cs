namespace SharpMapTracker
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.traceTextBox = new System.Windows.Forms.TextBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.saveMapButton = new System.Windows.Forms.Button();
            this.topMostCheckBox = new System.Windows.Forms.CheckBox();
            this.trackcamButton = new System.Windows.Forms.Button();
            this.trackMoveableItemCheckBox = new System.Windows.Forms.CheckBox();
            this.trackSplashItemsCheckBox = new System.Windows.Forms.CheckBox();
            this.trackMonstersCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.trackOnlyCurrentFloorCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.creatureCountTextBox = new System.Windows.Forms.TextBox();
            this.tileCountTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.loadClientButton = new System.Windows.Forms.Button();
            this.trackNPCsCheckbox = new System.Windows.Forms.CheckBox();
            this.miniMap = new SharpMapTracker.MiniMap();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // traceTextBox
            // 
            this.traceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.traceTextBox.Location = new System.Drawing.Point(13, 289);
            this.traceTextBox.Multiline = true;
            this.traceTextBox.Name = "traceTextBox";
            this.traceTextBox.ReadOnly = true;
            this.traceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.traceTextBox.Size = new System.Drawing.Size(587, 131);
            this.traceTextBox.TabIndex = 0;
            // 
            // clearButton
            // 
            this.clearButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearButton.Location = new System.Drawing.Point(13, 52);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(129, 34);
            this.clearButton.TabIndex = 3;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // saveMapButton
            // 
            this.saveMapButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveMapButton.Location = new System.Drawing.Point(151, 52);
            this.saveMapButton.Name = "saveMapButton";
            this.saveMapButton.Size = new System.Drawing.Size(129, 34);
            this.saveMapButton.TabIndex = 4;
            this.saveMapButton.Text = "Save";
            this.saveMapButton.UseVisualStyleBackColor = true;
            this.saveMapButton.Click += new System.EventHandler(this.saveMapButton_Click);
            // 
            // topMostCheckBox
            // 
            this.topMostCheckBox.AutoSize = true;
            this.topMostCheckBox.Checked = true;
            this.topMostCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.topMostCheckBox.Location = new System.Drawing.Point(6, 65);
            this.topMostCheckBox.Name = "topMostCheckBox";
            this.topMostCheckBox.Size = new System.Drawing.Size(92, 17);
            this.topMostCheckBox.TabIndex = 5;
            this.topMostCheckBox.Text = "Always on top";
            this.topMostCheckBox.UseVisualStyleBackColor = true;
            // 
            // trackcamButton
            // 
            this.trackcamButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trackcamButton.Location = new System.Drawing.Point(151, 12);
            this.trackcamButton.Name = "trackcamButton";
            this.trackcamButton.Size = new System.Drawing.Size(129, 34);
            this.trackcamButton.TabIndex = 10;
            this.trackcamButton.Text = "Track TibiaCast";
            this.trackcamButton.UseVisualStyleBackColor = true;
            this.trackcamButton.Click += new System.EventHandler(this.trackcamButton_Click);
            // 
            // trackMoveableItemCheckBox
            // 
            this.trackMoveableItemCheckBox.AutoSize = true;
            this.trackMoveableItemCheckBox.Checked = true;
            this.trackMoveableItemCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.trackMoveableItemCheckBox.Location = new System.Drawing.Point(6, 19);
            this.trackMoveableItemCheckBox.Name = "trackMoveableItemCheckBox";
            this.trackMoveableItemCheckBox.Size = new System.Drawing.Size(130, 17);
            this.trackMoveableItemCheckBox.TabIndex = 13;
            this.trackMoveableItemCheckBox.Text = "Track moveable items";
            this.trackMoveableItemCheckBox.UseVisualStyleBackColor = true;
            // 
            // trackSplashItemsCheckBox
            // 
            this.trackSplashItemsCheckBox.AutoSize = true;
            this.trackSplashItemsCheckBox.Checked = true;
            this.trackSplashItemsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.trackSplashItemsCheckBox.Location = new System.Drawing.Point(147, 19);
            this.trackSplashItemsCheckBox.Name = "trackSplashItemsCheckBox";
            this.trackSplashItemsCheckBox.Size = new System.Drawing.Size(98, 17);
            this.trackSplashItemsCheckBox.TabIndex = 14;
            this.trackSplashItemsCheckBox.Text = "Track splashes";
            this.trackSplashItemsCheckBox.UseVisualStyleBackColor = true;
            // 
            // trackMonstersCheckBox
            // 
            this.trackMonstersCheckBox.AutoSize = true;
            this.trackMonstersCheckBox.Checked = true;
            this.trackMonstersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.trackMonstersCheckBox.Location = new System.Drawing.Point(147, 42);
            this.trackMonstersCheckBox.Name = "trackMonstersCheckBox";
            this.trackMonstersCheckBox.Size = new System.Drawing.Size(99, 17);
            this.trackMonstersCheckBox.TabIndex = 16;
            this.trackMonstersCheckBox.Text = "Track monsters";
            this.trackMonstersCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.trackNPCsCheckbox);
            this.groupBox1.Controls.Add(this.trackOnlyCurrentFloorCheckBox);
            this.groupBox1.Controls.Add(this.trackMoveableItemCheckBox);
            this.groupBox1.Controls.Add(this.trackMonstersCheckBox);
            this.groupBox1.Controls.Add(this.trackSplashItemsCheckBox);
            this.groupBox1.Controls.Add(this.topMostCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(14, 94);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 89);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // trackOnlyCurrentFloorCheckBox
            // 
            this.trackOnlyCurrentFloorCheckBox.AutoSize = true;
            this.trackOnlyCurrentFloorCheckBox.Location = new System.Drawing.Point(6, 42);
            this.trackOnlyCurrentFloorCheckBox.Name = "trackOnlyCurrentFloorCheckBox";
            this.trackOnlyCurrentFloorCheckBox.Size = new System.Drawing.Size(135, 17);
            this.trackOnlyCurrentFloorCheckBox.TabIndex = 17;
            this.trackOnlyCurrentFloorCheckBox.Text = "Track only current floor";
            this.trackOnlyCurrentFloorCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.creatureCountTextBox);
            this.groupBox2.Controls.Add(this.tileCountTextBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(14, 189);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 65);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Statistics";
            // 
            // creatureCountTextBox
            // 
            this.creatureCountTextBox.Location = new System.Drawing.Point(113, 35);
            this.creatureCountTextBox.Name = "creatureCountTextBox";
            this.creatureCountTextBox.ReadOnly = true;
            this.creatureCountTextBox.Size = new System.Drawing.Size(59, 20);
            this.creatureCountTextBox.TabIndex = 3;
            this.creatureCountTextBox.Text = "0";
            // 
            // tileCountTextBox
            // 
            this.tileCountTextBox.Location = new System.Drawing.Point(113, 13);
            this.tileCountTextBox.Name = "tileCountTextBox";
            this.tileCountTextBox.ReadOnly = true;
            this.tileCountTextBox.Size = new System.Drawing.Size(59, 20);
            this.tileCountTextBox.TabIndex = 2;
            this.tileCountTextBox.Text = "0";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Creature Count:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tile Count:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // loadClientButton
            // 
            this.loadClientButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadClientButton.Location = new System.Drawing.Point(13, 12);
            this.loadClientButton.Name = "loadClientButton";
            this.loadClientButton.Size = new System.Drawing.Size(129, 34);
            this.loadClientButton.TabIndex = 19;
            this.loadClientButton.Text = "Load Client";
            this.loadClientButton.UseVisualStyleBackColor = true;
            this.loadClientButton.Click += new System.EventHandler(this.loadClientButton_Click);
            // 
            // trackNPCsCheckbox
            // 
            this.trackNPCsCheckbox.AutoSize = true;
            this.trackNPCsCheckbox.Checked = true;
            this.trackNPCsCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.trackNPCsCheckbox.Location = new System.Drawing.Point(147, 65);
            this.trackNPCsCheckbox.Name = "trackNPCsCheckbox";
            this.trackNPCsCheckbox.Size = new System.Drawing.Size(80, 17);
            this.trackNPCsCheckbox.TabIndex = 18;
            this.trackNPCsCheckbox.Text = "Track npcs";
            this.trackNPCsCheckbox.UseVisualStyleBackColor = true;
            // 
            // miniMap
            // 
            this.miniMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.miniMap.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.miniMap.CenterLocation = null;
            this.miniMap.Floor = 0;
            this.miniMap.Location = new System.Drawing.Point(286, 12);
            this.miniMap.Name = "miniMap";
            this.miniMap.Size = new System.Drawing.Size(314, 271);
            this.miniMap.TabIndex = 15;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 433);
            this.Controls.Add(this.loadClientButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.miniMap);
            this.Controls.Add(this.trackcamButton);
            this.Controls.Add(this.saveMapButton);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.traceTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "SharpMapTracker";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox traceTextBox;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button saveMapButton;
        private System.Windows.Forms.CheckBox topMostCheckBox;
        private System.Windows.Forms.Button trackcamButton;
        private System.Windows.Forms.CheckBox trackMoveableItemCheckBox;
        private System.Windows.Forms.CheckBox trackSplashItemsCheckBox;
        private MiniMap miniMap;
        private System.Windows.Forms.CheckBox trackMonstersCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox creatureCountTextBox;
        private System.Windows.Forms.TextBox tileCountTextBox;
        private System.Windows.Forms.Button loadClientButton;
        private System.Windows.Forms.CheckBox trackOnlyCurrentFloorCheckBox;
        private System.Windows.Forms.CheckBox trackNPCsCheckbox;
    }
}