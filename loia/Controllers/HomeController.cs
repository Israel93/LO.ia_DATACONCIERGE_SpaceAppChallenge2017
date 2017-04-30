using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace loia.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Categoria = "";
            return View();
        }

        FileStream imagen;
        public ActionResult AnalizarImagen()
        {

            string directory = @"D:\Temp\";

            HttpPostedFileBase photo = Request.Files["photo"];

            if (photo != null && photo.ContentLength > 0)
            {
                var fileName = Path.GetFileName(photo.FileName);
                photo.SaveAs(Path.Combine(directory, fileName));
                imagen = new FileStream(Path.Combine(directory, fileName), FileMode.Open);
                AnalysisResult datosDeAnalisis = Task.Run(MetodoAnalisis).Result;

                List<string> listaDeEtiquetas = new List<string>();
                String Categoria = "";
                foreach (var item in datosDeAnalisis.Tags)
                {
                    listaDeEtiquetas.Add(item.Name);
                    if (item.Name.Equals("street"))
                    {
                        Categoria = "Earthquakes";
                    }
                    else if (item.Name.Equals("mountain"))
                    {
                        Categoria = "Volcanoes";
                    }
                    else if (item.Name.Equals("forest"))
                    {
                        Categoria = "Wildfires";
                    }
                }
                // return Json(listaDeEtiquetas, JsonRequestBehavior.AllowGet);

                ViewBag.Categoria = Categoria;
                ViewBag.imagen = "data:image/png;base64,"+ConvertirImagen(Path.Combine(directory, fileName));
                return View("Index");
            }
            else
            {
                ViewBag.imagen = "";
                ViewBag.Categoria = "";
                return View("Index");
            }
        }


        public string ConvertirImagen(String pt) {

            using (Image image = Image.FromFile(pt))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }

        }


        public async Task<AnalysisResult> MetodoAnalisis()
        {

            var visionClient = new VisionServiceClient("2822d90b11e9423ebfbc618fedb40b7c");
            AnalysisResult analysisResult;
            var features = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Description };

            using (var fs = imagen)
            {
                analysisResult = await visionClient.AnalyzeImageAsync(fs, features);
            }
            return analysisResult;
        }

    }
}