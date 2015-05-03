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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenusForm));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.floorslope = new System.Windows.Forms.ToolStripButton();
			this.ceilingslope = new System.Windows.Forms.ToolStripButton();
			this.floorandceilingslope = new System.Windows.Forms.ToolStripButton();
			this.updateslopes = new System.Windows.Forms.ToolStripButton();
			this.addsectorscontextmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ceilingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.floorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.removeSlopeFromCeilingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.removeSlopeFromFloorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1.SuspendLayout();
			this.addsectorscontextmenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.floorslope,
            this.ceilingslope,
            this.floorandceilingslope,
            this.updateslopes});
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
			// updateslopes
			// 
			this.updateslopes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.updateslopes.Image = ((System.Drawing.Image)(resources.GetObject("updateslopes.Image")));
			this.updateslopes.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.updateslopes.Name = "updateslopes";
			this.updateslopes.Size = new System.Drawing.Size(85, 22);
			this.updateslopes.Text = "Update slopes";
			this.updateslopes.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// addsectorscontextmenu
			// 
			this.addsectorscontextmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ceilingToolStripMenuItem,
            this.floorToolStripMenuItem,
            this.toolStripSeparator1,
            this.removeSlopeFromCeilingToolStripMenuItem,
            this.removeSlopeFromFloorToolStripMenuItem});
			this.addsectorscontextmenu.Name = "addsectorscontextmenu";
			this.addsectorscontextmenu.Size = new System.Drawing.Size(216, 120);
			this.addsectorscontextmenu.Opening += new System.ComponentModel.CancelEventHandler(this.addsectorscontextmenu_Opening);
			// 
			// ceilingToolStripMenuItem
			// 
			this.ceilingToolStripMenuItem.Name = "ceilingToolStripMenuItem";
			this.ceilingToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.ceilingToolStripMenuItem.Text = "Add slope to ceiling";
			this.ceilingToolStripMenuItem.Click += new System.EventHandler(this.ceilingToolStripMenuItem_Click);
			// 
			// floorToolStripMenuItem
			// 
			this.floorToolStripMenuItem.Name = "floorToolStripMenuItem";
			this.floorToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.floorToolStripMenuItem.Text = "Add slope to floor";
			this.floorToolStripMenuItem.Click += new System.EventHandler(this.floorToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(212, 6);
			// 
			// removeSlopeFromCeilingToolStripMenuItem
			// 
			this.removeSlopeFromCeilingToolStripMenuItem.Name = "removeSlopeFromCeilingToolStripMenuItem";
			this.removeSlopeFromCeilingToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.removeSlopeFromCeilingToolStripMenuItem.Text = "Remove slope from ceiling";
			this.removeSlopeFromCeilingToolStripMenuItem.Click += new System.EventHandler(this.removeSlopeFromCeilingToolStripMenuItem_Click);
			// 
			// removeSlopeFromFloorToolStripMenuItem
			// 
			this.removeSlopeFromFloorToolStripMenuItem.Name = "removeSlopeFromFloorToolStripMenuItem";
			this.removeSlopeFromFloorToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.removeSlopeFromFloorToolStripMenuItem.Text = "Remove slope from floor";
			this.removeSlopeFromFloorToolStripMenuItem.Click += new System.EventHandler(this.removeSlopeFromFloorToolStripMenuItem_Click);
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
			this.addsectorscontextmenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton floorslope;
		private System.Windows.Forms.ToolStripButton ceilingslope;
		private System.Windows.Forms.ToolStripButton floorandceilingslope;
		private System.Windows.Forms.ToolStripButton updateslopes;
		private System.Windows.Forms.ContextMenuStrip addsectorscontextmenu;
		private System.Windows.Forms.ToolStripMenuItem floorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ceilingToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem removeSlopeFromCeilingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem removeSlopeFromFloorToolStripMenuItem;
	}
}