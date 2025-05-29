using System;
using System.Drawing;
using System.Windows.Forms;

namespace ERLC
{
    public class MainForm : Form
    {
        private CheckBox chkOverlayToggle;
        private Label lblStatus;
        private GroupBox grpLockpicking;
        private Button btnStartLockpick;
        private GroupBox grpAtmRobbery;
        private Button btnStartAtm;
        private GroupBox grpGlassCutting;
        private Button btnStartGlassCutting;
        private OverlayForm _overlayForm; // Added field for OverlayForm

        public MainForm()
        {
            InitializeComponent();
            _overlayForm = new OverlayForm(); // Instantiate OverlayForm
        }

        private void InitializeComponent()
        {
            // Form Properties
            this.Text = "ERLC Helper";
            this.Size = new Size(400, 350); // Adjusted size for better layout
            this.StartPosition = FormStartPosition.CenterScreen;

            // chkOverlayToggle
            this.chkOverlayToggle = new CheckBox();
            this.chkOverlayToggle.Text = "Enable Visual Overlay";
            this.chkOverlayToggle.Location = new Point(10, 10);
            this.chkOverlayToggle.AutoSize = true;
            this.chkOverlayToggle.CheckedChanged += new EventHandler(this.chkOverlayToggle_CheckedChanged); // Added event handler

            // lblStatus
            this.lblStatus = new Label();
            this.lblStatus.Text = "Status: Idle";
            this.lblStatus.Location = new Point(10, 40);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new Font(this.lblStatus.Font, FontStyle.Bold);


            // grpLockpicking
            this.grpLockpicking = new GroupBox();
            this.grpLockpicking.Text = "Lockpicking";
            this.grpLockpicking.Location = new Point(10, 70);
            this.grpLockpicking.Size = new Size(360, 70); // Adjusted size

            // btnStartLockpick
            this.btnStartLockpick = new Button();
            this.btnStartLockpick.Text = "Start Auto Lockpick";
            this.btnStartLockpick.Location = new Point(10, 20); // Relative to GroupBox
            this.btnStartLockpick.Size = new Size(150, 30);   // Adjusted size
            this.btnStartLockpick.Click += new EventHandler(this.btnStartLockpick_Click); // Added event handler
            this.grpLockpicking.Controls.Add(this.btnStartLockpick);

            // grpAtmRobbery
            this.grpAtmRobbery = new GroupBox();
            this.grpAtmRobbery.Text = "ATM Robbery";
            this.grpAtmRobbery.Location = new Point(10, 150); // Adjusted position
            this.grpAtmRobbery.Size = new Size(360, 70);    // Adjusted size

            // btnStartAtm
            this.btnStartAtm = new Button();
            this.btnStartAtm.Text = "Start Auto ATM";
            this.btnStartAtm.Location = new Point(10, 20);    // Relative to GroupBox
            this.btnStartAtm.Size = new Size(150, 30);      // Adjusted size
            this.grpAtmRobbery.Controls.Add(this.btnStartAtm);

            // grpGlassCutting
            this.grpGlassCutting = new GroupBox();
            this.grpGlassCutting.Text = "Glass Cutting";
            this.grpGlassCutting.Location = new Point(10, 230); // Adjusted position
            this.grpGlassCutting.Size = new Size(360, 70);     // Adjusted size

            // btnStartGlassCutting
            this.btnStartGlassCutting = new Button();
            this.btnStartGlassCutting.Text = "Start Auto Glass Cutting";
            this.btnStartGlassCutting.Location = new Point(10, 20); // Relative to GroupBox
            this.btnStartGlassCutting.Size = new Size(180, 30);   // Adjusted size for longer text
            this.grpGlassCutting.Controls.Add(this.btnStartGlassCutting);

            // Add Controls to Form
            this.Controls.Add(this.chkOverlayToggle);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grpLockpicking);
            this.Controls.Add(this.grpAtmRobbery);
            this.Controls.Add(this.grpGlassCutting);
        }

        private void chkOverlayToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (chkOverlayToggle.Checked)
            {
                _overlayForm.Show(); // Show the form if it's not visible
                _overlayForm.BringToFront(); // Ensure it's on top
            }
            else
            {
                _overlayForm.Hide(); // Hide the form
                // If an operation is not active, clear the overlay when it's hidden.
                // If an operation IS active, it will continue to update the (hidden) overlay.
                // The task's finally block for the operation should clear it.
                _overlayForm.UpdateOverlay(new System.Collections.Generic.List<Action<Graphics>>()); 
            }
        }

        private void btnStartLockpick_Click(object sender, EventArgs e)
        {
            Action<string> statusUpdate = (status) => {
                if (lblStatus.InvokeRequired) {
                    lblStatus.Invoke(new Action(() => lblStatus.Text = status));
                } else {
                    lblStatus.Text = status;
                }
            };

            Action uiUpdate = () => {
                btnStartLockpick.Enabled = !btnStartLockpick.Enabled;
                btnStartAtm.Enabled = !btnStartAtm.Enabled;
                btnStartGlassCutting.Enabled = !btnStartGlassCutting.Enabled;
                btnStartLockpick.Text = btnStartLockpick.Enabled ? "Start Auto Lockpick" : "Lockpicking Active...";
            };

            if (this.InvokeRequired) {
                this.Invoke(uiUpdate);
            } else {
                uiUpdate();
            }
            
            if (!chkOverlayToggle.Checked)
            {
                statusUpdate("Please enable the visual overlay first for lockpicking.");
                 if (this.InvokeRequired) { this.Invoke(uiUpdate); } else { uiUpdate(); } // Re-enable buttons
                return;
            }


            System.Threading.Tasks.Task.Run(() => {
                try {
                    var lockPicker = new ERLC.Robberies.LockPicking();
                    lockPicker.StartLockpickingWithOverlay(_overlayForm.UpdateOverlay, statusUpdate);
                    statusUpdate("Lockpicking finished successfully.");
                } catch (Exception ex) {
                    statusUpdate($"Error during lockpicking: {ex.Message.Split('\n')[0]}"); // Show first line of ex
                } finally {
                    if (this.InvokeRequired) {
                        this.Invoke(uiUpdate);
                        this.Invoke(new Action(() => _overlayForm.UpdateOverlay(new System.Collections.Generic.List<Action<Graphics>>()) ));
                    } else {
                        uiUpdate();
                        _overlayForm.UpdateOverlay(new System.Collections.Generic.List<Action<Graphics>>());
                    }
                }
            });
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _overlayForm?.Dispose(); // Dispose of the overlay form when MainForm closes
        }
    }
}
