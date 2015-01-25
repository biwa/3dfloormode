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
			this.floorz = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.ceilingz = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.apply = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.ceiling = new System.Windows.Forms.CheckBox();
			this.floor = new System.Windows.Forms.CheckBox();
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
			this.positionx.TabIndex = 0;
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
			this.positiony.TabIndex = 1;
			// 
			// floorz
			// 
			this.floorz.AllowDecimal = false;
			this.floorz.AllowNegative = true;
			this.floorz.AllowRelative = true;
			this.floorz.BackColor = System.Drawing.Color.Transparent;
			this.floorz.ButtonStep = 8;
			this.floorz.ButtonStepFloat = 1F;
			this.floorz.ButtonStepsWrapAround = false;
			this.floorz.Location = new System.Drawing.Point(87, 111);
			this.floorz.Name = "floorz";
			this.floorz.Size = new System.Drawing.Size(115, 24);
			this.floorz.StepValues = null;
			this.floorz.TabIndex = 3;
			// 
			// ceilingz
			// 
			this.ceilingz.AllowDecimal = false;
			this.ceilingz.AllowNegative = true;
			this.ceilingz.AllowRelative = true;
			this.ceilingz.BackColor = System.Drawing.Color.Transparent;
			this.ceilingz.ButtonStep = 8;
			this.ceilingz.ButtonStepFloat = 1F;
			this.ceilingz.ButtonStepsWrapAround = false;
			this.ceilingz.Location = new System.Drawing.Point(87, 81);
			this.ceilingz.Name = "ceilingz";
			this.ceilingz.Size = new System.Drawing.Size(115, 24);
			this.ceilingz.StepValues = null;
			this.ceilingz.TabIndex = 2;
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
			this.apply.Location = new System.Drawing.Point(143, 178);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(112, 25);
			this.apply.TabIndex = 5;
			this.apply.Text = "OK";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(25, 178);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(112, 25);
			this.cancel.TabIndex = 4;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// ceiling
			// 
			this.ceiling.AutoSize = true;
			this.ceiling.Location = new System.Drawing.Point(11, 85);
			this.ceiling.Name = "ceiling";
			this.ceiling.Size = new System.Drawing.Size(70, 18);
			this.ceiling.TabIndex = 10;
			this.ceiling.Text = "Ceiling Z:";
			this.ceiling.UseVisualStyleBackColor = true;
			this.ceiling.CheckedChanged += new System.EventHandler(this.ceiling_CheckedChanged);
			// 
			// floor
			// 
			this.floor.Location = new System.Drawing.Point(11, 115);
			this.floor.Name = "floor";
			this.floor.Size = new System.Drawing.Size(70, 18);
			this.floor.TabIndex = 11;
			this.floor.Text = "Floor Z:";
			this.floor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.floor.UseVisualStyleBackColor = true;
			this.floor.CheckedChanged += new System.EventHandler(this.floor_CheckedChanged);
			// 
			// SlopeVertexEditForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(267, 215);
			this.Controls.Add(this.floor);
			this.Controls.Add(this.ceiling);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ceilingz);
			this.Controls.Add(this.floorz);
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
		public CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox floorz;
		public CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox ceilingz;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.CheckBox ceiling;
		private System.Windows.Forms.CheckBox floor;
	}
}