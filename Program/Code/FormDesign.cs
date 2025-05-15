using System.ComponentModel;

partial class Form1
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
    public Label Tbox;
    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        SuspendLayout();
        //Text Box
        Tbox = new Label();
        Tbox.Location = new Point(0, 0);
        Tbox.Size = new Size(90, 180);
        //Form
        this.Name = "TheWindow";
        this.Text = "TheWindowText";
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(900, 900);
        this.Controls.Add(this.Tbox);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
}
