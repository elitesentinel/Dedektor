using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Math.Geometry;
using System.IO.Ports;
using Point = System.Drawing.Point; 

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCapTureDevices;
        private VideoCaptureDevice Finalvideo;
                  

        public Form1()
        {
            InitializeComponent();
        }

        int R; //Trackbarın değişkenleri
        int G;
        int B;
        int px;//kare boyutları için
        int py;
        int secim; // uzaklık değeri
        double ksdeger = 1.70; // kare sapma değeri
        double dsdeger = 1.50; // dikdörtgen sapma değeri
        int Adeger; // karşılaştıralacak değerler için
        int Hdeger;
      
       SerialPort port; 
        
        private void Form1_Load(object sender, EventArgs e)
        {
            button3.Enabled = false;
           // serial port nesnesinin bağlantısı
         port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
       port.Open();
            // donanımda ki kameraları belirlenmesi ve seçimi
            VideoCapTureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in VideoCapTureDevices)
            {

                comboBox1.Items.Add(VideoCaptureDevice.Name);

            }

            comboBox1.SelectedIndex = 0;
          

        }
        

        private void button1_Click(object sender, EventArgs e)
        {

           
            Finalvideo = new VideoCaptureDevice(VideoCapTureDevices[comboBox1.SelectedIndex].MonikerString);
            Finalvideo.NewFrame += new NewFrameEventHandler(Finalvideo_NewFrame);
            Finalvideo.DesiredFrameRate = 20;//saniyede kaç görüntü alsın istiyorsanız. FPS
            Finalvideo.DesiredFrameSize = new Size(320,240);//görüntü boyutları
            Finalvideo.Start();
        }

        void Finalvideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            Bitmap image1 = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = image;
                
                 
                EuclideanColorFiltering filter = new EuclideanColorFiltering();                
                filter.CenterColor = new RGB(Color.FromArgb(R, G, B));
                filter.Radius = 100;                
                filter.ApplyInPlace(image1);
                nesnebul(image1);

            

          
          
        }
        public void nesnebul(Bitmap image)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinWidth = 5;
            blobCounter.MinHeight = 5;
            blobCounter.FilterBlobs = true;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;
            
            
            

            BitmapData objectsData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            UnmanagedImage grayImage = grayscaleFilter.Apply(new UnmanagedImage(objectsData));
            
            image.UnlockBits(objectsData);


            blobCounter.ProcessImage(image);
            Rectangle[] rects = blobCounter.GetObjectsRectangles();
            Blob[] blobs = blobCounter.GetObjectsInformation();
            pictureBox2.Image = image;



                
                Rectangle objectRect;
                foreach (Rectangle recs in rects)
                {
                    if (rects.Length > 0)
                    {
                        objectRect = rects[0];
                        
                        Graphics g = pictureBox1.CreateGraphics();
                        using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                        {
                            g.DrawRectangle(pen, objectRect);
                        }

                        g.Dispose();

                        if (chkCisimEbat.Checked)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                richTextBox1.Text = "Genişlik : " + objectRect.Width.ToString() + ",\t \nYükseklik: " + objectRect.Height.ToString() + "\n" + richTextBox1.Text + "\n"; ;
                                
                                px = objectRect.Width;
                                py = objectRect.Height;

                            });
                        }
                    }
                 }
                
        }
        
        // renk ayarı
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            R = trackBar1.Value;
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            G = trackBar2.Value;
        }
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            B = trackBar3.Value;
        }
        // progmramı durdurmak için 
        private void button2_Click(object sender, EventArgs e)
        {
            button3.Enabled = true;
                // video başladıysa ve  çalışıyorsa
            if (Finalvideo != null && Finalvideo.IsRunning)
            {
                Finalvideo.Stop();

            }
        }


        // programı kapatmak için 
        private void button3_Click(object sender, EventArgs e)
        {
            // video başladıysa ve  çalışıyorsa
            if (Finalvideo != null && Finalvideo.IsRunning)
            {
                Finalvideo.Stop();

            }

            Application.Exit();
        }

       
        // boyut hesabı için timer başlatma
        private void button7_Click(object sender, EventArgs e)
        {
            textBox2.Text = "Başladı";

            // kare sapma değeri bir öncekiyle sıfırlamak için
            if (ksdeger !=1.70)
            {
                ksdeger = 1.70;
            }
            // kare sapma değeri oranı hesaplaması
            secim = comboBox2.SelectedIndex;
            
            switch (secim)
            {
                case 0: MessageBox.Show("Minimum yükseklik 70 cm olmalıdır !", "", MessageBoxButtons.OKCancel); break;
                case 1: ; break;
                case 2: ksdeger = ksdeger + 0.10; break;
                case 3: ksdeger = ksdeger + 0.20; break;
                case 4: ksdeger = ksdeger + 0.35; break;
                case 5: ksdeger = ksdeger + 0.45; break;
                case 6: ksdeger = ksdeger + 0.55; break;
                case 7: ksdeger = ksdeger + 1.15; break;
                case 8: ksdeger = ksdeger + 1.35; break;
                case 9: ksdeger = ksdeger + 1.45; break;


            }
            timer1.Interval = 1000;
            timer1.Start();
            
        } 
       
        // hesap fonskiyonuna değer gönderip textbox2 ye aktarmak için

        private void timer1_Tick(object sender, EventArgs e)
        {


            // cisim x ve y değeri ve uzaklak değeri hesaplatıp ekrana yazılaması
            textBox2.Text = hesaplama(px, py).ToString();
            richTextBox1.Clear();


        }
        // mesafeye göre cisim boyutu hesaplama
        public int hesaplama(int x, int y )
        {
            int fark;
            double sonuc;
            sonuc = 1.0;
            int yuvarlax;
            int yuvarlay;
            int eklex;
            int ekley;
            fark = x - y;
            
            
            // eğer uzaklık sensörü kullanılmıyorsa 70 cm de çalışacak kodlar
            
                
                // eğer kareyse (yaklaşık olarak)
                if (fark > -6 && fark < 6)
                {
                    //yuvarlamak için
                    yuvarlax = x % 10;
                    if (yuvarlax != 0)
                    {
                        eklex = 10 - yuvarlax;
                        x = x + eklex;
                    }
                    sonuc = x * 4;
                    //inç değeri için
                    sonuc = sonuc / 96;
                    // cm değeri için
                    sonuc = sonuc * 2.54;
                  
                    // 35 cm de alınan sonuç cismin gerçek boyutu
                    // uzaklık 70 cm e çıkarıldığında aradaki sapmayı kapatmak için
                    // yaklaşık 35 cm da bir 70 piksel az alınan piksel değeri
                    // farkını kapatmak için
                    label5.Text = " Hesaplamada kullanılan oran :  " + ksdeger.ToString();
                    sonuc = sonuc * ksdeger;


                }
                else
                {
                    //yuvarlama için
                    yuvarlax = x % 10;
                    yuvarlay = y % 10;
                    if (yuvarlax != 0)
                    {
                        eklex = 10 - yuvarlax;
                        x = x + eklex;
                    }
                    if (yuvarlay != 0)
                    {
                        ekley = 10 - yuvarlay;
                        x = x + ekley;
                    }
                    sonuc = (2 * x) + (2 * y);
                    sonuc = sonuc / 96;
                    sonuc = sonuc * 2.54;
                    sonuc = sonuc * dsdeger;



                }
            
            
            
            return Convert.ToInt32(sonuc);
            

        }


        // timer1 durdurmak için
        private void button8_Click(object sender, EventArgs e)
        {
            timer1.Stop();

        }
        

        // cisim boyut ile istenilen boyut eşleşmesini kontrol eden buton
        private void button4_Click(object sender, EventArgs e)
        {
            Adeger = int.Parse(textBox1.Text);
            Hdeger = int.Parse(textBox2.Text);
            int ydeger;
            ydeger = Adeger - Hdeger;

           
           
                if (ydeger > -3 && ydeger < 3)
                {
                    // serial porta gönderilen deger

                   port.Write("var");
                    

                    Tarama.Visible = true;
                    Tarama.Value = 100;
                    Tarama.Step = 1;
                    button5.Visible = true;
                    button6.Visible = false;
                    
                }
                else
                {
                    // serial porta gönderilen deger
                 
                   port.Write("yok");


                    Tarama.Value = 0;
                    button5.Visible = false;
                    button6.Visible = true;
                    Tarama.Visible = false;
                }

        }
     
       

       

       

       

       
 
        

        

       

       

       
      
    
        

  

  

      

       
       
            

    }


}


