using System.Diagnostics;

namespace DemoIntroAsync1925597
{
    public partial class Form1 : Form
    {
        HttpClient cliente = new HttpClient();
        public Form1()
        {
            InitializeComponent();
        }

        //Peligroso: async void debe ser evitado, EXCEPTO en eventos.
        private async void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;

            var directorioActual = AppDomain.CurrentDomain.BaseDirectory;
            var destinoBaseSecuencial = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");
            var destinoBaseParalelo = Path.Combine(directorioActual, @"Imagenes\resultado-paralelo");
            PrepararEjecucion(destinoBaseParalelo, destinoBaseSecuencial);

            Console.WriteLine("Inicio");
            List<Imagen> imagenes = ObtenerImagenes();

            //Parte secuencial
            var sw = new Stopwatch();
            sw.Start();

            foreach(var imagen in imagenes)
            {
                await ProcesarImagen(destinoBaseSecuencial, imagen);
            }
            Console.WriteLine("Secuencial - duracion en segundos {0}", sw.ElapsedMilliseconds / 1000);

            sw.Reset();
            sw.Start();

            var tareasEnumerable = imagenes.Select(async imagen =>
            {
                await ProcesarImagen(destinoBaseParalelo, imagen);
            });

            await Task.WhenAll(tareasEnumerable);
            Console.WriteLine("Paralelo - duracion en segundos {0}", sw.ElapsedMilliseconds / 1000);

            sw.Stop();

            pictureBox1.Visible = false;
        }

        private async Task ProcesarImagen(string directorio, Imagen imagen)
        {
            var respuesta = await cliente.GetAsync(imagen.Url);
            var contenido = await respuesta.Content.ReadAsByteArrayAsync();

            Bitmap bitmap;
            using (var ms = new MemoryStream(contenido))
            {
                bitmap = new Bitmap(ms);
            }

            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            var destino = Path.Combine(directorio, imagen.Nombre);
            bitmap.Save(destino);
        }

        private static List<Imagen> ObtenerImagenes()
        {
            var imagenes = new List<Imagen>();
            for (int i = 0; i < 4; i++)
            {
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Cacicazgos {i}.png",
                        Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Copia_de_Cacicazgos_de_la_Hispaniola.png/800px-Copia_de_Cacicazgos_de_la_Hispaniola.png"
                    });
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Desangles {i}.jpg",
                        Url = "https://upload.wikimedia.org/wikipedia/commons/4/43/Desangles_Colon_engrillado.jpg"
                    });
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Alcazar {i}.jpg",
                        Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d7/Santo_Domingo_-_Alc%C3%A1zar_de_Col%C3%B3n_0777.JPG/800px-Santo_Domingo_-_Alc%C3%A1zar_de_Col%C3%B3n_0777.JPG"
                    });

                
            }
            return imagenes;
        }

        private void BorrarArchivos(string directorio)
        {
            var archivos = Directory.EnumerateFiles(directorio);
            foreach (var archivo in archivos)
            {
                File.Delete(archivo);
            }
        }

        private void PrepararEjecucion(string destinoBaseParalelo, string destinoBaseSecuencial)
        {
            if (!Directory.Exists(destinoBaseParalelo))
            {
                Directory.CreateDirectory(destinoBaseParalelo);
            }
            if (!Directory.Exists(destinoBaseSecuencial))
            {
                Directory.CreateDirectory(destinoBaseSecuencial);
            }

            BorrarArchivos(destinoBaseParalelo);
            BorrarArchivos(destinoBaseSecuencial);
        }

        private async Task<string> ProcesamientoLargo()
        {
            await Task.Delay(3000);  //Asíncrono
            return "Maynor";
        }

        private async Task RealizarProcesamientoLargoA()
        {
            await Task.Delay(1000); //Asíncrono
            Console.WriteLine("Proceso 'A' finalizado");
        }

        private async Task RealizarProcesamientoLargoB()
        {
            await Task.Delay(1000); //Asíncrono
            Console.WriteLine("Proceso 'B' finalizado");
        }

        private async Task RealizarProcesamientoLargoC()
        {
            await Task.Delay(1000); //Asíncrono
            Console.WriteLine("Proceso 'C' finalizado");
        }
    }
}
