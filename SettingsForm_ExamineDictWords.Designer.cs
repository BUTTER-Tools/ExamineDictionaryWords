namespace ExamineDictWords
{
    partial class SettingsForm_ExamineDictWords
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm_ExamineDictWords));
            this.OKButton = new System.Windows.Forms.Button();
            this.LoadDictionaryButton = new System.Windows.Forms.Button();
            this.RawCountsCheckbox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SelectedFileTextbox = new System.Windows.Forms.TextBox();
            this.IncludeStDevCheckbox = new System.Windows.Forms.CheckBox();
            this.RoundingToNParameter = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.RoundingToNParameter)).BeginInit();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OKButton.Location = new System.Drawing.Point(214, 281);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(118, 40);
            this.OKButton.TabIndex = 6;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // LoadDictionaryButton
            // 
            this.LoadDictionaryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadDictionaryButton.Location = new System.Drawing.Point(15, 93);
            this.LoadDictionaryButton.Name = "LoadDictionaryButton";
            this.LoadDictionaryButton.Size = new System.Drawing.Size(118, 40);
            this.LoadDictionaryButton.TabIndex = 1003;
            this.LoadDictionaryButton.Text = "Load External Dictionary";
            this.LoadDictionaryButton.UseVisualStyleBackColor = true;
            this.LoadDictionaryButton.Click += new System.EventHandler(this.LoadDictionaryButton_Click);
            // 
            // RawCountsCheckbox
            // 
            this.RawCountsCheckbox.AutoSize = true;
            this.RawCountsCheckbox.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RawCountsCheckbox.Location = new System.Drawing.Point(15, 155);
            this.RawCountsCheckbox.Name = "RawCountsCheckbox";
            this.RawCountsCheckbox.Size = new System.Drawing.Size(254, 20);
            this.RawCountsCheckbox.TabIndex = 1004;
            this.RawCountsCheckbox.Text = "Provide output as raw frequencies";
            this.RawCountsCheckbox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(15, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 16);
            this.label1.TabIndex = 1006;
            this.label1.Text = "Dictionary File:";
            // 
            // SelectedFileTextbox
            // 
            this.SelectedFileTextbox.Enabled = false;
            this.SelectedFileTextbox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectedFileTextbox.Location = new System.Drawing.Point(15, 54);
            this.SelectedFileTextbox.MaxLength = 2147483647;
            this.SelectedFileTextbox.Name = "SelectedFileTextbox";
            this.SelectedFileTextbox.Size = new System.Drawing.Size(516, 23);
            this.SelectedFileTextbox.TabIndex = 1005;
            // 
            // IncludeStDevCheckbox
            // 
            this.IncludeStDevCheckbox.AutoSize = true;
            this.IncludeStDevCheckbox.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IncludeStDevCheckbox.Location = new System.Drawing.Point(15, 190);
            this.IncludeStDevCheckbox.Name = "IncludeStDevCheckbox";
            this.IncludeStDevCheckbox.Size = new System.Drawing.Size(280, 20);
            this.IncludeStDevCheckbox.TabIndex = 1007;
            this.IncludeStDevCheckbox.Text = "Include Standard Deviations in Output";
            this.IncludeStDevCheckbox.UseVisualStyleBackColor = true;
            // 
            // StDevRoundUpDown
            // 
            this.RoundingToNParameter.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RoundingToNParameter.Location = new System.Drawing.Point(237, 222);
            this.RoundingToNParameter.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.RoundingToNParameter.Name = "StDevRoundUpDown";
            this.RoundingToNParameter.Size = new System.Drawing.Size(61, 26);
            this.RoundingToNParameter.TabIndex = 1008;
            this.RoundingToNParameter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.RoundingToNParameter.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(15, 226);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(216, 16);
            this.label2.TabIndex = 1009;
            this.label2.Text = "Round All Values to N decimals:";
            // 
            // SettingsForm_ExamineDictWords
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 333);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RoundingToNParameter);
            this.Controls.Add(this.IncludeStDevCheckbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SelectedFileTextbox);
            this.Controls.Add(this.RawCountsCheckbox);
            this.Controls.Add(this.LoadDictionaryButton);
            this.Controls.Add(this.OKButton);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm_ExamineDictWords";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Plugin Name";
            ((System.ComponentModel.ISupportInitialize)(this.RoundingToNParameter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button LoadDictionaryButton;
        private System.Windows.Forms.CheckBox RawCountsCheckbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SelectedFileTextbox;
        private System.Windows.Forms.CheckBox IncludeStDevCheckbox;
        private System.Windows.Forms.NumericUpDown RoundingToNParameter;
        private System.Windows.Forms.Label label2;
    }
}