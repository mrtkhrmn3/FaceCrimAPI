using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FaceCrimAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        [HttpPost("predict")]
        [Consumes("multipart/form-data")]
        public IActionResult PredictImage( IFormFile file)
        {
            try
            {
             /*Bu kullanım, kaynak yönetimini ifade eder. Bu yapı sayesinde kullanılan kaynak 
             (örneğin bir dosya, bellek, ağ bağlantısı) iş bittiğinde otomatik olarak serbest 
             bırakılır. Bu, IDisposable arayüzünü uygulayan nesnelerle kullanılır.

                Ne yapar?
                FileStream nesnesini oluşturur ve bir dosya yazma işlemi başlatır.
                İşlem bittiğinde, kullanılan kaynak (dosya akışı) otomatik olarak serbest bırakılır.
                Neden önemli?

                Bellek sızıntısını önler.
                Kodun güvenli ve verimli çalışmasını sağlar.*/

                // Gelen dosyayı kaydet
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }


                /*
                ProcessStartInfo:
                Python scriptini çalıştırmak için bir işlem (process) başlatır.
                FileName = "python": Python çalıştırıcısını kullanacağını belirtir.
                Arguments = $"predict.py {filePath}": predict.py adlı Python script’ine, geçici dosyanın yolunu argüman olarak gönderir.
                RedirectStandardOutput = true: Python script’inden dönen çıktıyı yakalar.
                UseShellExecute = false ve CreateNoWindow = true: Yeni bir komut istemcisi penceresi açmadan script’i çalıştırır.
                */
                // Python scriptini çalıştır

                string homePath = Environment.GetEnvironmentVariable("HOME");
                string azureHomePath = Path.Combine(homePath, "site", "wwwroot", "predict", "predict.py");
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "predict", "predict.py");

                var start = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"{azureHomePath} {filePath}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };


                /*
                Process.Start(start): Python scriptini başlatır.
                process.StandardOutput: Script’in ürettiği çıktıyı okur.
                reader.ReadToEnd(): Çıktıyı tamamen okur.
                 */
                using (var process = Process.Start(start))
                {
                    using (var reader = process.StandardOutput)
                    {
                        var result = reader.ReadToEnd();
                        if(result == null)
                        {
                                return Ok("amina koymussun");
                        }
                        return Ok(new { Prediction = result });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
