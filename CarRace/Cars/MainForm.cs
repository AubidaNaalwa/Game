using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShadowEngine;
using Tao.OpenGl;
using ShadowEngine.OpenGL;
using ShadowEngine.ContentLoading;
using OpenTK.Graphics.ES10;

namespace CarRace
{
    public partial class MainForm : Form
    {
        uint hdc;
        int selectedCamara = 1;
        int count;
        Controller control = new Controller();
        int mostrado = 0;
        int moving;
        float []lightPosition;
        float x;
        float y;
        float z;
        float[] lightAmbient;
       
        public MainForm()
        {
            x = 0;
            y = 20;
            z = 20;
            InitializeComponent();
            //identificador del lugar en donde voy a dibujar
            hdc = (uint)this.Handle;
            //toma el error que sucedio
            string error = "";
            //Comando de inicializacion de la ventana grafica
            OpenGLControl.OpenGLInit(ref hdc, this.Width, this.Height, ref error);
            
            //inicia la posicion de la camara asi como define en angulo de perspectiva,etc etc
            control.Camara.SelectCamara(0);
            if (error != "")
            {
                MessageBox.Show("Ocurrio un error al inicializar openGL");
                this.Close();   
            }

            //Habilita las luces

            //{ 0.2f, 0.2f, 0.2f, 0.0F };
            lightAmbient = new float[4];
            lightAmbient[0] = 0.2F;
            lightAmbient[1] = 0.2F;
            lightAmbient[2] = 0.2F;
            lightAmbient[3] = 0.0F;


            lightPosition = new float[3];
            Lighting.LightAmbient = lightAmbient;
            lightPosition[0] = x;
            lightPosition[1] = y;
            lightPosition[2] = z;
            Lighting.AmbientLightPosition = lightPosition;
            Lighting.SetupLighting();  // encapsulado en el sahdow engine 
            
            ContentManager.SetTextureList("texturas\\");
            ContentManager.LoadTextures();
            ContentManager.SetModelList("modelos\\");
            ContentManager.LoadModels();  
            control.CreateObjects();

            //Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);   
        }
        bool Started = false;
        public void UpdateLogic()
        {
            if (!Started)
            {
                control.Camara.SelectCamara(selectedCamara - 1);
                Started = true;
            }
            if (moving == 1)
            {
                Gl.glTranslatef(0, 0, 0.35f);
            }
            else
                if (moving == -1)
            {
                Gl.glTranslatef(0, 0, -0.35f);
            }
            else
                if (moving == 2)
            {
                Gl.glTranslatef(-0.35f, 0, 0);
            }
            else if (moving == -2)
            {
                Gl.glTranslatef(0.35f, 0, 0);
            }
            count++;
            if (Controller.FinishedRace == true && mostrado == 0)
            {
                mostrado = 1;
                moving = 0;
                MessageBox.Show("The winner was the: " + lblPrimero.Text);
            }

            if (count == 10)
            {
                if (Controller.StartedRace == true && mostrado == 0)
                {
                    int primero = control.GetFirstPlace();
                    float distanciaRecorrida = control.GetDistanceInMeters(primero);
                    lblDistancia.Text = Convert.ToString((int)distanciaRecorrida);
                    switch (primero)
                    {
                        case 0:
                            {
                                lblPrimero.Text = "Blue car";
                                lblPrimero.ForeColor = Color.Blue;
                                break;
                            }
                        case 1:
                            {
                                lblPrimero.Text = "Red car";
                                lblPrimero.ForeColor = Color.Red;
                                break;
                            }
                        case 2:
                            {
                                lblPrimero.Text = "Black car";
                                lblPrimero.ForeColor = Color.Black;
                                break;
                            }
                    }
                }
                count = 0;
            }
        }


        private void tmrPaint_Tick(object sender, EventArgs e)
        {

            UpdateLogic(); 

            // clean opengl to draw
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            //draws the entire scene
            lightPosition[0] = x;
            lightPosition[1] = y;
            lightPosition[2] = z;
            Lighting.AmbientLightPosition = lightPosition;
            Lighting.SetupLighting();
            control.DrawScene();
            //change buffers
            Winapi.SwapBuffers(hdc);
            //tell opengl to drop any operation he is doing and to prepare for a new frame
            Gl.glFlush(); 
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            control.Camara.SelectCamara(0);

            Gl.glShadeModel(Gl.GL_SMOOTH);
            Gl.glClearColor(0.5f, 0.2f, 0.0f, 0.5f);
            Gl.glClearDepth(1.0f);

            Lighting.LightAmbient = lightAmbient;
            lightPosition[0] = x;
            lightPosition[1] = y;
            lightPosition[2] = z;
            Lighting.AmbientLightPosition = lightPosition;

            Gl.glEnable(Gl.GL_LIGHT0);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT_AND_DIFFUSE);



            Gl.glLoadIdentity();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            selectedCamara--;
            if (selectedCamara == 0)
            {
                selectedCamara = 4;
            }
            lblCamara.Text = Convert.ToString(selectedCamara);
            control.Camara.SelectCamara(selectedCamara - 1);    
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectedCamara++;
            if (selectedCamara == 5)
            {
                selectedCamara = 1;
            }
            lblCamara.Text = Convert.ToString(selectedCamara);
            control.Camara.SelectCamara(selectedCamara-1);   
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Controller.StartedRace = true;   
        }

        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            lblPrimero.Text = "None";
            lblDistancia.Text = "0";
            control.ResetRace();
            mostrado = 0;
            count = 0;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            Gl.glViewport(0, 0, this.Width, this.Height);
            //select the projection matrix
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            //la reseteo
            Gl.glLoadIdentity();
            //45 = angulo de vision
            //1  = proporcion de alto por ancho
            //0.1f = distancia minima en la que se pinta
            //1000 = distancia maxima
            Glu.gluPerspective(55, this.Width/(float)this.Height  , 0.1f, 1000);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            control.Camara.SelectCamara(selectedCamara - 1); 
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.W)
            {
                moving = 1;
            }
            if (e.KeyData == Keys.S)
            {
                moving = -1;
            }
            if(e.KeyData == Keys.A)
            {
                moving = -2;
            }
            if(e.KeyData == Keys.D)
            {
                moving = 2;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            moving = 0;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void vScrollBar3_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            x = 100 -hScrollBar1.Value -50;
            
            tmrPaint_Tick(sender,e);

        }

        private void hScrollBar2_Scroll_1(object sender, ScrollEventArgs e)
        {
            y =  100 -hScrollBar2.Value - 30 ;
            tmrPaint_Tick(sender, e);
        }

        private void hScrollBar3_Scroll_1(object sender, ScrollEventArgs e)
        {
            z =  100 - hScrollBar3.Value -30 ;
            tmrPaint_Tick(sender, e);
        }



        private void label10_Click(object sender, EventArgs e)
        {

        }
    }
}
