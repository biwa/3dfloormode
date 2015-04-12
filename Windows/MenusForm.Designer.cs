namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	partial class MenusForm
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
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.floorslope = new System.Windows.Forms.ToolStripButton();
			this.ceilingslope = new System.Windows.Forms.ToolStripButton();
			this.floorandceilingslope = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.floorslope,
            this.ceilingslope,
            this.floorandceilingslope});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(284, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// floorslope
			// 
			this.floorslope.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.floorslope.Image = global::ThreeDFloorMode.Properties.Resources.Floor;
			this.floorslope.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.floorslope.Name = "floorslope";
			this.floorslope.Size = new System.Drawing.Size(23, 22);
			this.floorslope.Tag = "drawfloorslope";
			this.floorslope.Text = "Apply drawn slope to floor";
			this.floorslope.Click += new System.EventHandler(this.floorslope_Click);
			// 
			// ceilingslope
			// 
			this.ceilingslope.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ceilingslope.Image = global::ThreeDFloorMode.Properties.Resources.Ceiling;
			this.ceilingslope.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ceilingslope.Name = "ceilingslope";
			this.ceilingslope.Size = new System.Drawing.Size(23, 22);
			this.ceilingslope.Tag = "drawceilingslope";
			this.ceilingslope.Text = "Apply drawn slope to ceiling";
			this.ceilingslope.Click += new System.EventHandler(this.ceilingslope_Click);
			// 
			// floorandceilingslope
			// 
			this.floorandceilingslope.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.floorandceilingslope.Image = global::ThreeDFloorMode.Properties.Resources.FloorAndCeiling;
			this.floorandceilingslope.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.floorandceilingslope.Name = "floorandceilingslope";
			this.floorandceilingslope.Size = new System.Drawing.Size(23, 22);
			this.floorandceilingslope.Tag = "drawfloorandceilingslope";
			this.floorandceilingslope.Text = "Apply drawn slope to floor and ceiling";
			this.floorandceilingslope.Click += new System.EventHandler(this.floorandceilingslope_Click);
			// 
			// MenusForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.toolStrip1);
			this.Name = "MenusForm";
			this.Text = "MenusForm";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton floorslope;
		private System.Windows.Forms.ToolStripButton ceilingslope;
		private System.Windows.Forms.ToolStripButton floorandceilingslope;
	}
}