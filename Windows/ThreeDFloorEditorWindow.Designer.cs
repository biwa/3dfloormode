namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	partial class ThreeDFloorEditorWindow
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.threeDFloorPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.addThreeDFloorButton = new System.Windows.Forms.Button();
			this.sharedThreeDFloorsCheckBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// threeDFloorPanel
			// 
			this.threeDFloorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.threeDFloorPanel.AutoScroll = true;
			this.threeDFloorPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.threeDFloorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.threeDFloorPanel.Location = new System.Drawing.Point(12, 12);
			this.threeDFloorPanel.Name = "threeDFloorPanel";
			this.threeDFloorPanel.Size = new System.Drawing.Size(760, 494);
			this.threeDFloorPanel.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(616, 513);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(697, 513);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// addThreeDFloorButton
			// 
			this.addThreeDFloorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addThreeDFloorButton.Location = new System.Drawing.Point(12, 513);
			this.addThreeDFloorButton.Name = "addThreeDFloorButton";
			this.addThreeDFloorButton.Size = new System.Drawing.Size(75, 23);
			this.addThreeDFloorButton.TabIndex = 1;
			this.addThreeDFloorButton.Text = "Add 3D floor";
			this.addThreeDFloorButton.UseVisualStyleBackColor = true;
			this.addThreeDFloorButton.Click += new System.EventHandler(this.addThreeDFloorButton_Click);
			// 
			// sharedThreeDFloorsCheckBox
			// 
			this.sharedThreeDFloorsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.sharedThreeDFloorsCheckBox.AutoSize = true;
			this.sharedThreeDFloorsCheckBox.Location = new System.Drawing.Point(93, 517);
			this.sharedThreeDFloorsCheckBox.Name = "sharedThreeDFloorsCheckBox";
			this.sharedThreeDFloorsCheckBox.Size = new System.Drawing.Size(155, 17);
			this.sharedThreeDFloorsCheckBox.TabIndex = 2;
			this.sharedThreeDFloorsCheckBox.Text = "Show shared 3D floors only";
			this.sharedThreeDFloorsCheckBox.UseVisualStyleBackColor = true;
			this.sharedThreeDFloorsCheckBox.CheckedChanged += new System.EventHandler(this.sharedThreeDFloorsCheckBox_CheckedChanged);
			// 
			// ThreeDFloorEditorWindow
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(784, 548);
			this.Controls.Add(this.sharedThreeDFloorsCheckBox);
			this.Controls.Add(this.addThreeDFloorButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.threeDFloorPanel);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(800, 2000);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 38);
			this.Name = "ThreeDFloorEditorWindow";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "3D floors";
			this.Load += new System.EventHandler(this.ThreeDFloorEditorWindow_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel threeDFloorPanel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button addThreeDFloorButton;
		private System.Windows.Forms.CheckBox sharedThreeDFloorsCheckBox;

	}
}