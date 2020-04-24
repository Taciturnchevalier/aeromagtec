﻿
namespace aeromagtec.Controls
{
    partial class QuickView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelWithPseudoOpacity1 = new System.Windows.Forms.Label();
  
            this.labelWithPseudoOpacity2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
          
            this.SuspendLayout();
            // 
            // labelWithPseudoOpacity1
            // 
            this.labelWithPseudoOpacity1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWithPseudoOpacity1.AutoSize = false;
            this.labelWithPseudoOpacity1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWithPseudoOpacity1.Location = new System.Drawing.Point(3, 0);
            this.labelWithPseudoOpacity1.Name = "labelWithPseudoOpacity1";
            this.labelWithPseudoOpacity1.Size = new System.Drawing.Size(118, 16);
            this.labelWithPseudoOpacity1.TabIndex = 0;
            this.labelWithPseudoOpacity1.Text = "Altitude:";
            this.labelWithPseudoOpacity1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.labelWithPseudoOpacity1, "Double click to change");
            // 

            // 
            // labelWithPseudoOpacity2
            // 
            this.labelWithPseudoOpacity2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWithPseudoOpacity2.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWithPseudoOpacity2.Location = new System.Drawing.Point(3, 20);
            this.labelWithPseudoOpacity2.Name = "labelWithPseudoOpacity2";
            this.labelWithPseudoOpacity2.Size = new System.Drawing.Size(118, 34);
            this.labelWithPseudoOpacity2.TabIndex = 2;
            this.labelWithPseudoOpacity2.Text = "0000.00";
            this.labelWithPseudoOpacity2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.labelWithPseudoOpacity2, "Double click to change");
            // 
            // QuickView
            // 
            this.Controls.Add(this.labelWithPseudoOpacity1);
            this.Controls.Add(this.labelWithPseudoOpacity2);
            this.Name = "QuickView";
            this.Size = new System.Drawing.Size(122, 54);
            this.Resize += new System.EventHandler(this.QuickView_Resize);

            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelWithPseudoOpacity1;

        private System.Windows.Forms.Label labelWithPseudoOpacity2;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
