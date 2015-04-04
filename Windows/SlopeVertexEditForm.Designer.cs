namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	partial class SlopeVertexEditForm
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
			this.positionx = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.positiony = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.positionz = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.apply = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.planetype = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// positionx
			// 
			this.positionx.AllowDecimal = false;
			this.positionx.AllowNegative = true;
			this.positionx.AllowRelative = true;
			this.positionx.BackColor = System.Drawing.Color.Transparent;
			this.positionx.ButtonStep = 8;
			this.positionx.ButtonStepFloat = 1F;
			this.positionx.ButtonStepsWrapAround = false;
			this.positionx.Location = new System.Drawing.Point(87, 19);
			this.positionx.Name = "positionx";
			this.positionx.Size = new System.Drawing.Size(115, 24);
			this.positionx.StepValues = null;
			this.positionx.TabIndex = 4;
			// 
			// positiony
			// 
			this.positiony.AllowDecimal = false;
			this.positiony.AllowNegative = true;
			this.positiony.AllowRelative = true;
			this.positiony.BackColor = System.Drawing.Color.Transparent;
			this.positiony.ButtonStep = 8;
			this.positiony.ButtonStepFloat = 1F;
			this.positiony.ButtonStepsWrapAround = false;
			this.positiony.Location = new System.Drawing.Point(87, 51);
			this.positiony.Name = "positiony";
			this.positiony.Size = new System.Drawing.Size(115, 24);
			this.positiony.StepValues = null;
			this.positiony.TabIndex = 5;
			// 
			// positionz
			// 
			this.positionz.AllowDecimal = false;
			this.positionz.AllowNegative = true;
			this.positionz.AllowRelative = true;
			this.positionz.BackColor = System.Drawing.Color.Transparent;
			this.positionz.ButtonStep = 8;
			this.positionz.ButtonStepFloat = 1F;
			this.positionz.ButtonStepsWrapAround = false;
			this.positionz.Location = new System.Drawing.Point(87, 81);
			this.positionz.Name = "positionz";
			this.positionz.Size = new System.Drawing.Size(115, 24);
			this.positionz.StepValues = null;
			this.positionz.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(64, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 14);
			this.label1.TabIndex = 6;
			this.label1.Text = "X:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(64, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 14);
			this.label2.TabIndex = 7;
			this.label2.Text = "Y:";
			// 
			// apply
			// 
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.apply.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.apply.Location = new System.Drawing.Point(17, 178);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(112, 25);
			this.apply.TabIndex = 2;
			this.apply.Text = "OK";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(137, 178);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(112, 25);
			this.cancel.TabIndex = 3;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(64, 86);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(17, 14);
			this.label3.TabIndex = 8;
			this.label3.Text = "Z:";
			// 
			// planetype
			// 
			this.planetype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.planetype.FormattingEnabled = true;
			this.planetype.Items.AddRange(new object[] {
            "Ceiling",
            "Floor"});
			this.planetype.Location = new System.Drawing.Point(87, 112);
			this.planetype.Name = "planetype";
			this.planetype.Size = new System.Drawing.Size(95, 22);
			this.planetype.TabIndex = 1;
			// 
			// SlopeVertexEditForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(267, 215);
			this.Controls.Add(this.planetype);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.positionz);
			this.Controls.Add(this.positiony);
			this.Controls.Add(this.positionx);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SlopeVertexEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Slope Vertex";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox positionx;
		public CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox positiony;
		public CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox positionz;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox planetype;
	}
}