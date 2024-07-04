namespace GameboyEmulator
{
    public partial class MainForm : Form
    {
        // Gameboy Emulator v0.1
        GameBoy _gameBoy = null;
        Thread machineThread;
        
        // rom file name
        string romFileName = "";

        // Widescreen & Scaling mode
        bool wMode = false;
        int sizeFactor = 1;
        int sizeFactorX = 1;
        public MainForm()
        {
            InitializeComponent();

            // Initialize Gameboy
            _gameBoy = new GameBoy(this);
            // Enable debug mode (for now)
            _gameBoy._debugMode = false;
            // Hook Event
            _gameBoy.GameBoyLogEvent += _gameBoy_GameBoyLogEvent;

            //
            resizeForm();
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {
            // Create open file dialog for loading gameboy rom files
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GAMEBOY ROM FILES| *.gb| GAMEBOY COLOR ROM FILES| *.gbc";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\" + "ROMS";

            // Show dialog and handle selecting rom
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {    
                // Remove Menu Bar
                //HideMenu();

                // Load rom filename into memory
                romFileName = openFileDialog.FileName;

                // Load rom catridge
                _gameBoy.Load(openFileDialog.FileName);
                _gameBoy.PowerOn();
            }
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            // Kill emulator
            _gameBoy.Stop();
            // Kill thread
            machineThread.Join();
            // Close this form window
            this.Close();
        }

        private void HideMenu()
        {
            // Remove Menu Bar
            this.Height -= toolStrip1.Height;
            toolStrip1.Visible = false;
        }

        private void ShowMenu()
        {
            // Show Menu Bar
            //this.Height += menuStrip1.Height;
            //menuStrip1.Visible = true;
        }

        private void debugBtn_Click(object sender, EventArgs e)
        {
            //
            _gameBoy._runAllOpCode = false;
            _gameBoy._executeNextOpCode = false;

            // Start Gameboy
            machineThread = new Thread(() => _gameBoy.Start());
            machineThread.Start();
        }

        private void _gameBoy_GameBoyLogEvent(object? sender, EventArgs e)
        {
            // Fire when log was created
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_gameBoy._programLoop)
                _gameBoy.JOYPAD.handleKeyDown(e);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (_gameBoy._programLoop)
                _gameBoy.JOYPAD.handleKeyUp(e);
        }

        bool CLOSEFLAG = false;

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Terminate function If no rom was loaded
            if (romFileName == "")
                return;

            if(!CLOSEFLAG)
            {
                // Cancel close event
                e.Cancel = true;

                // If gameboy rom running
                if (_gameBoy._programLoop)
                {
                    // Stop Gameboy
                    _gameBoy.Stop();

                    // Wait till power off
                    while(_gameBoy._powerOn)
                    {
                        // Wait 10 milliseconds
                        Thread.Sleep(100);
                    }

                    // Save gamestate 
                    _gameBoy.Save(romFileName);
                }

                // Reset Closeflag so form can close
                CLOSEFLAG = true;
                // Close form
                this.Close();
            }
        }

        private void palleteBtn_Click_1(object sender, EventArgs e)
        {
            if(_gameBoy._powerOn)
                this.palleteBtn.Text = "Pallete (" + _gameBoy.SwitchPallete() + ")";
        }

        private void enlargeBtn_Click(object sender, EventArgs e)
        {
            if (_gameBoy._powerOn)
            {
                sizeFactor = _gameBoy.EnlargeView() ;
                this.enlargeBtn.Text = "Enlarge (" + sizeFactor + "X)";
                resizeForm();
            }
        }

 
        private void widescreenBtn_Click(object sender, EventArgs e)
        {
            // Toggle Widescreen mode
            wMode = !wMode;

            // Update screenmode
            if (_gameBoy._powerOn)
            {
                // Set widescreen
                _gameBoy.WideScreenView();
                // Update Label
                this.widescreenBtn.Text = "Widescreen (" + (wMode ? "ON " : "OFF") + ")";
                // Resize form
                resizeForm();
            }
        }

        private void resizeForm()
        {
            // Scale factor
            sizeFactorX = sizeFactor;

            // Update scale factor
            if (wMode)
            {
                if (sizeFactor > 1)
                {
                    sizeFactorX = sizeFactor + (sizeFactor / 2);
                }
                else
                {
                    sizeFactorX = sizeFactor + sizeFactor;
                }
            }

            // Update Picture box dimensions
            this.Width = pictureBox1.Image.Width * sizeFactorX;
            this.Height = (pictureBox1.Image.Height * sizeFactor) + 36;
        }
    }
}