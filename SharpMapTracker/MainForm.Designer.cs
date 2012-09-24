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
            this.miniMap = new SharpMapTracker.MiniMap();
            this.trackCreaturesCheckBox = new System.Windows.Forms.CheckBox();
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
            this.traceTextBox.Size = new System.Drawing.Size(504, 131);
            this.traceTextBox.TabIndex = 0;
            // 
            // clearButton
            // 
            this.clearButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearButton.Location = new System.Drawing.Point(13, 12);
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
            this.saveMapButton.Location = new System.Drawing.Point(13, 52);
            this.saveMapButton.Name = "saveMapButton";
            this.saveMapButton.Size = new System.Drawing.Size(129, 34);
            this.saveMapButton.TabIndex = 4;
            this.saveMapButton.Text = "Save";
            this.saveMapButton.UseVisualStyleBackColor = true;
            this.saveMapButton.Click += new System.EventHandler(this.saveMapButton_Click);
            // 
            // topMostCheckBox
            // 
            this.topMostCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.topMostCheckBox.AutoSize = true;
            this.topMostCheckBox.Checked = true;
            this.topMostCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.topMostCheckBox.Location = new System.Drawing.Point(13, 266);
            this.topMostCheckBox.Name = "topMostCheckBox";
            this.topMostCheckBox.Size = new System.Drawing.Size(92, 17);
            this.topMostCheckBox.TabIndex = 5;
            this.topMostCheckBox.Text = "Always on top";
            this.topMostCheckBox.UseVisualStyleBackColor = true;
            // 
            // trackcamButton
            // 
            this.trackcamButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trackcamButton.Location = new System.Drawing.Point(13, 92);
            this.trackcamButton.Name = "trackcamButton";
            this.trackcamButton.Size = new System.Drawing.Size(129, 50);
            this.trackcamButton.TabIndex = 10;
            this.trackcamButton.Text = "Track TibiaCast File";
            this.trackcamButton.UseVisualStyleBackColor = true;
            this.trackcamButton.Click += new System.EventHandler(this.trackcamButton_Click);
            // 
            // trackMoveableItemCheckBox
            // 
            this.trackMoveableItemCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.trackMoveableItemCheckBox.AutoSize = true;
            this.trackMoveableItemCheckBox.Checked = true;
            this.trackMoveableItemCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.trackMoveableItemCheckBox.Location = new System.Drawing.Point(13, 177);
            this.trackMoveableItemCheckBox.Name = "trackMoveableItemCheckBox";
            this.trackMoveableItemCheckBox.Size = new System.Drawing.Size(130, 17);
            this.trackMoveableItemCheckBox.TabIndex = 13;
            this.trackMoveableItemCheckBox.Text = "Track moveable items";
            this.trackMoveableItemCheckBox.UseVisualStyleBackColor = true;
            // 
            // trackSplashItemsCheckBox
            // 
            this.trackSplashItemsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.trackSplashItemsCheckBox.AutoSize = true;
            this.trackSplashItemsCheckBox.Checked = true;
            this.trackSplashItemsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.trackSplashItemsCheckBox.Location = new System.Drawing.Point(13, 200);
            this.trackSplashItemsCheckBox.Name = "trackSplashItemsCheckBox";
            this.trackSplashItemsCheckBox.Size = new System.Drawing.Size(98, 17);
            this.trackSplashItemsCheckBox.TabIndex = 14;
            this.trackSplashItemsCheckBox.Text = "Track splashes";
            this.trackSplashItemsCheckBox.UseVisualStyleBackColor = true;
            // 
            // miniMap
            // 
            this.miniMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.miniMap.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.miniMap.Location = new System.Drawing.Point(165, 12);
            this.miniMap.Name = "miniMap";
            this.miniMap.Size = new System.Drawing.Size(352, 271);
            this.miniMap.TabIndex = 15;
            // 
            // trackCreaturesCheckBox
            // 
            this.trackCreaturesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.trackCreaturesCheckBox.AutoSize = true;
            this.trackCreaturesCheckBox.Checked = true;
            this.trackCreaturesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.trackCreaturesCheckBox.Location = new System.Drawing.Point(13, 223);
            this.trackCreaturesCheckBox.Name = "trackCreaturesCheckBox";
            this.trackCreaturesCheckBox.Size = new System.Drawing.Size(101, 17);
            this.trackCreaturesCheckBox.TabIndex = 16;
            this.trackCreaturesCheckBox.Text = "Track creatures";
            this.trackCreaturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 433);
            this.Controls.Add(this.trackCreaturesCheckBox);
            this.Controls.Add(this.miniMap);
            this.Controls.Add(this.trackSplashItemsCheckBox);
            this.Controls.Add(this.trackMoveableItemCheckBox);
            this.Controls.Add(this.trackcamButton);
            this.Controls.Add(this.topMostCheckBox);
            this.Controls.Add(this.saveMapButton);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.traceTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "SharpMapTracker v0.1";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MainForm_Load);
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
        private System.Windows.Forms.CheckBox trackCreaturesCheckBox;
    }
}