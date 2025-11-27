using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SlotMachineFinal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (numCycles != null)
            {
                numCycles.Minimum = 1;
                numCycles.Value = 20;
            }
        }

        private void btnSpin_Click(object sender, EventArgs e)
        {
            int spins = 20; 
            if (numCycles != null) spins = (int)numCycles.Value;
            if (spins < 5) spins = 20; // Siguranță

            this.Hide();

            using (SlotWindow game = new SlotWindow(spins))
            {
                game.Run(60.0); 

                if (game.Won)
                    MessageBox.Show("JACKPOT! Ai câștigat!", "Rezultat");
                else
                    MessageBox.Show("Ai pierdut. Mai încearcă!", "Rezultat");
            }

            this.Show();
        }

        private void spinTimer_Tick(object sender, EventArgs e) { }
        private void numCycles_ValueChanged(object sender, EventArgs e) { }
    }

    class SlotWindow : GameWindow
    {
        int _spinsLeft;         
        int _cooldown = 180;    
        int _frameSkipCounter = 0; 

        int[] _slots = { 0, 0, 0 };
        int[] _textureIds = new int[4];
        Random _rand = new Random();
        public bool Won = false;

        public SlotWindow(int spins) : base(800, 300, GraphicsMode.Default, "Slot Machine - OpenGL")
        {
            _spinsLeft = spins;
            this.VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.CornflowerBlue);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            LoadTexture(0, "img0.png");
            LoadTexture(1, "img1.png");
            LoadTexture(2, "img2.png");
            LoadTexture(3, "img3.png");

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 300, 0, 100, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (_spinsLeft > 0)
            {
                _frameSkipCounter++;

                if (_frameSkipCounter >= 5)
                {
                    _slots[0] = _rand.Next(0, 4);
                    _slots[1] = _rand.Next(0, 4);
                    _slots[2] = _rand.Next(0, 4);

                    _spinsLeft--;      
                    _frameSkipCounter = 0; 
                }
            }
            else
            {
                if (_cooldown == 180)
                {
                    if (_slots[0] == _slots[1] && _slots[1] == _slots[2])
                        Won = true;
                    else
                        Won = false;
                }

                _cooldown--;

                if (_cooldown <= 0)
                {
                    this.Exit();
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            for (int i = 0; i < 3; i++)
            {
                DrawSlot(i, _slots[i]);
            }

            this.SwapBuffers();
        }

        private void DrawSlot(int pos, int texIndex)
        {
            if (_textureIds[texIndex] != 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, _textureIds[texIndex]);
                GL.Color3(Color.White);
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
                if (texIndex == 0) GL.Color3(Color.Red);
                else if (texIndex == 1) GL.Color3(Color.Yellow);
                else if (texIndex == 2) GL.Color3(Color.Green);
                else GL.Color3(Color.Purple);
            }

            double xStart = pos * 100;
            double xEnd = xStart + 100;

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(xStart, 0);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(xEnd, 0);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(xEnd, 100);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(xStart, 100);
            GL.End();

            GL.Enable(EnableCap.Texture2D);
        }

        private void LoadTexture(int index, string filename)
        {
            try
            {
                if (!System.IO.File.Exists(filename)) return;
                _textureIds[index] = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _textureIds[index]);
                Bitmap bmp = new Bitmap(filename);
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            catch { }
        }
    }
}