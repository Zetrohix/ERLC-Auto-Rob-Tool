using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ERLC
{
    public class OverlayForm : Form
    {
        private List<Action<Graphics>> _drawingActions = new List<Action<Graphics>>();

        public OverlayForm()
        {
            InitializeComponent();
            // Set a default font if not already set by InitializeComponent or designer
            if (this.Font == null)
            {
                this.Font = new Font("Arial", 12F); 
            }
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.Text = "ERLC Overlay"; 
            // DoubleBuffered can help reduce flicker
            this.DoubleBuffered = true; 
        }

        public void UpdateOverlay(List<Action<Graphics>> drawingActions)
        {
            _drawingActions = drawingActions ?? new List<Action<Graphics>>();
            this.Invalidate(); // Triggers a repaint
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); // Call the base class's OnPaint method

            // As per instructions, we don't clear with TransparencyKey here.
            // The OS handles transparency for areas matching BackColor/TransparencyKey.

            if (_drawingActions != null && _drawingActions.Count > 0)
            {
                foreach (var action in _drawingActions)
                {
                    action(e.Graphics);
                }
            }
            else
            {
                // If no drawing actions are provided, clear the overlay.
                // This uses BackColor, which is set to Magenta (our TransparencyKey).
                e.Graphics.Clear(this.BackColor);
            }
        }
    }
}
